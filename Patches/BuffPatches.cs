using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using PvpArena.Data;
using System.Collections.Generic;
using PvpArena.GameModes;
using PvpArena.Services;
using PvpArena.Helpers;
using PvpArena.Factories;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(BuffDebugSystem), nameof(BuffDebugSystem.OnUpdate))]
public static class BuffDebugSystemPatch
{
	private static Dictionary<PrefabGUID, float> ArmorHpValues = new Dictionary<PrefabGUID, float>
	{
		{Prefabs.EquipBuff_Cloak_Base, 0},
		{Prefabs.EquipBuff_Chest_Base, 164.38138f},
		{Prefabs.EquipBuff_Legs_Base, 134.6411f},
		{Prefabs.EquipBuff_Boots_Base, 115.851f},
		{Prefabs.EquipBuff_Gloves_Base, 89.76075f},
	};

	public static void Prefix(BuffDebugSystem __instance)
	{
		if (__instance.__OnUpdate_LambdaJob0_entityQuery.HasAnyMatches)
		{
			var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{				
				try
				{
					var Character = entity.Read<EntityOwner>().Owner;
					if (Character.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(Character);
						GameEvents.RaisePlayerBuffed(player, entity);

						var prefabGuid = entity.Read<PrefabGUID>();
						if (prefabGuid == Prefabs.Item_EquipBuff_Shared_General)
						{
							var buffer = entity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
							buffer.Add(BuffModifiers.CloakHealth); //moving cloak hp onto the neck so people don't need to use cloaks
						}
						UpdateArmorMaxHealth(entity, prefabGuid);
					}
					else
					{
						GameEvents.RaiseUnitBuffed(Character, entity);
					}
				}
				catch (System.Exception e)
				{
					continue;
				}

			}
			entities.Dispose();
		}
	}

	[HarmonyPatch(typeof(UpdateBuffsBuffer_Destroy), nameof(UpdateBuffsBuffer_Destroy.OnUpdate))]
	public static class UpdateBuffsBuffer_DestroyPatch
	{
		public static void Prefix(UpdateBuffsBuffer_Destroy __instance)
		{
			var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				var owner = entity.Read<EntityOwner>().Owner;
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerBuffRemoved(player, entity);
					}
					else if (entity.Read<PrefabGUID>() == Helper.CustomBuff4 && UnitFactory.HasCategory(owner, "dummy"))
					{
						var spawnPosition = UnitFactory.GetSpawnPositionOfEntity(owner);
						owner.Teleport(spawnPosition);
						var health = owner.Read<Health>();
						health.Value = health.MaxHealth;
						health.MaxRecoveryHealth = health.MaxHealth;
						owner.Write(health);
					}
				}
			}
			entities.Dispose();
		}
	}

	private static void UpdateMaxHealthStat(Entity entity, float newValue)
	{
		var buffer = entity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
		for (int i = 0; i < buffer.Length; i++)
		{
			if (buffer[i].StatType == UnitStatType.MaxHealth)
			{
				var newStat = buffer[i];
				newStat.Value = newValue;
				buffer[i] = newStat;
				break;
			}
		}
	}

	private static void UpdateArmorMaxHealth(Entity entity, PrefabGUID prefabGuid)
	{
		if (ArmorHpValues.ContainsKey(prefabGuid))
		{
			UpdateMaxHealthStat(entity, ArmorHpValues[prefabGuid]);
		}
	}
}


