using HarmonyLib;
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
using System;

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
				if (dealDamageEvent.Target.Exists())
				{
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
					if (dealDamageEvent.SpellSource.Exists())
					{
						var owner = dealDamageEvent.SpellSource.Read<EntityOwner>().Owner;
						if (owner.Exists())
						{
							if (owner.Has<PlayerCharacter>())
							{
								var player = PlayerService.GetPlayerFromCharacter(owner);
								GameEvents.RaisePlayerDealtDamage(player, entity);
							}
							else
							{
								GameEvents.RaiseUnitDealtDamage(owner, entity);
								if (entity.Exists() && dealDamageEvent.Target.Has<CastleHeartConnection>())
								{
									entity.Destroy();
								}
							}
							if (dealDamageEvent.Target.Has<PlayerCharacter>())
							{
								var player = PlayerService.GetPlayerFromCharacter(dealDamageEvent.Target);
								GameEvents.RaisePlayerReceivedDamage(player, entity);
							}
						}
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
		var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var statChangeEvent = entity.Read<StatChangeEvent>();
				var source = statChangeEvent.Source;
				if (source.Has<AbilityOwner>())
				{
					if (statChangeEvent.StatType == StatType.Health)
					{
						var damageDealerEntity = source.Read<EntityOwner>().Owner;
						if (damageDealerEntity.Exists() && damageDealerEntity.Has<PlayerCharacter>())
						{
							var damageDealerPlayer = PlayerService.GetPlayerFromCharacter(source.Read<EntityOwner>().Owner);
							var targetEntity = statChangeEvent.Entity;
							if (targetEntity.Exists())
							{
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
