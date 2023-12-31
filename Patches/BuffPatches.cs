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
using Unity.Transforms;
using PvpArena.Configs;
using Unity.Mathematics;
using PvpArena.Models;
using System;

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
					var prefabGuid = entity.Read<PrefabGUID>();
					var buffTarget = entity.Read<EntityOwner>().Owner;
					var buff = entity.Read<Buff>();
					if (buff.Target.Exists())
					{
						buffTarget = buff.Target;
					}
					if (buffTarget.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(buffTarget);
						GameEvents.RaisePlayerBuffed(player, entity);
						
						if (prefabGuid == Prefabs.Item_EquipBuff_Shared_General)
						{
							var buffer = entity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
							buffer.Add(BuffModifiers.CloakHealth); //moving cloak hp onto the neck so people don't need to use cloaks
						}
						UpdateArmorMaxHealth(entity, prefabGuid);
					}
					else
					{
						GameEvents.RaiseUnitBuffed(buffTarget, entity);
					}
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
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
				try
				{
					var buffTarget = entity.Read<EntityOwner>().Owner;
					var buff = entity.Read<Buff>();
					if (buff.Target.Exists())
					{
						buffTarget = buff.Target;
					}
					if (buffTarget.Exists())
					{
						if (buffTarget.Has<PlayerCharacter>())
						{
							var player = PlayerService.GetPlayerFromCharacter(buffTarget);
							GameEvents.RaisePlayerBuffRemoved(player, entity);
						}
						else
						{
							GameEvents.RaiseUnitBuffRemoved(buffTarget, entity);
						}
					}
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
					continue;
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


