using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Unity.Collections;
using Bloodstone.API;
using ProjectM.Gameplay.Systems;
using PvpArena.Services;
using ProjectM.CastleBuilding;
using PvpArena.GameModes;
using PvpArena.Factories;
using PvpArena.Helpers;
using static DamageRecorderService;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(DealDamageSystem), nameof(DealDamageSystem.OnUpdate))]
public static class DealDamageSystemPatch
{
	public static void Prefix(DealDamageSystem __instance)
	{

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var dealDamageEvent = entity.Read<DealDamageEvent>();
				if (UnitFactory.HasCategory(dealDamageEvent.Target, "dummy"))
				{
					if (Helper.TryGetBuff(dealDamageEvent.Target, Helper.CustomBuff4, out var buffEntity))
					{
						var age = buffEntity.Read<Age>();
						age.Value = 0;
						buffEntity.Write(age);
					}
					else
					{
						Helper.BuffEntity(dealDamageEvent.Target, Helper.CustomBuff4, out buffEntity, Dummy.ResetTime);
					}
				}
				if (dealDamageEvent.SpellSource.Index > 0)
				{
					var owner = dealDamageEvent.SpellSource.Read<EntityOwner>().Owner;
					if (owner.Index > 0 && owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerDealtDamage(player, entity);
					}
					else if (dealDamageEvent.Target.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(dealDamageEvent.Target);
						GameEvents.RaisePlayerReceivedDamage(player, entity);
					}
					else if (dealDamageEvent.Target.Has<CastleHeartConnection>())
					{
						VWorld.Server.EntityManager.DestroyEntity(entity);
					}
				}
			}
			catch (System.Exception e)
			{
				Plugin.PluginLog.LogInfo($"An error occurred in the deal damage system: {e.ToString()}");
			}
		}
	}
}


[HarmonyPatch(typeof(AiDamageTakenEventSystem), nameof(AiDamageTakenEventSystem.OnUpdate))]
public static class AiDamageTakenEventSystemPatch
{
	
	public static void Prefix(AiDamageTakenEventSystem __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var aiDamageTakenEvent = entity.Read<AiDamageTakenEvent>();
			//Plugin.PluginLog.LogInfo(aiDamageTakenEvent.Amount);
		}

		
		entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var statChangeEvent = entity.Read<StatChangeEvent>();
			var source = statChangeEvent.Source;
			if (source.Has<AbilityOwner>())
			{
				if (statChangeEvent.StatType == StatType.Health)
				{
					var damageDealerEntity = source.Read<EntityOwner>().Owner;
					if (damageDealerEntity.Has<PlayerCharacter>())
					{
						var damageDealerPlayer = PlayerService.GetPlayerFromCharacter(source.Read<EntityOwner>().Owner);
						var targetEntity = statChangeEvent.Entity;
						var totalDamage = statChangeEvent.Change;
						float damageShielded = 0;
						float critDamage = 0;
						if (Math.Abs(statChangeEvent.OriginalChange) > Math.Abs(statChangeEvent.Change))
						{
							totalDamage = statChangeEvent.OriginalChange; //include shielded damage
							damageShielded = Math.Abs(statChangeEvent.OriginalChange) - Math.Abs(statChangeEvent.Change);
						}
						else if (Math.Abs(statChangeEvent.Change) > Math.Abs(statChangeEvent.OriginalChange))
						{
							critDamage = Math.Abs(statChangeEvent.Change) - Math.Abs(statChangeEvent.OriginalChange);
						}
						var damageInfo = new DamageInfo
						{
							TotalDamage = Math.Abs(totalDamage),
							CritDamage = Math.Abs(critDamage),
							DamageAbsorbed = Math.Abs(damageShielded)
						};

						GameEvents.RaisePlayerDamageReported(damageDealerPlayer, targetEntity, source.Read<PrefabGUID>(), damageInfo);
					}
				}
			}
		}
		entities.Dispose();
	}
}
