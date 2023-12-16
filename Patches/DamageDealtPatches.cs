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
using PvpArena.Data;
using System.Collections.Generic;
using PvpArena.Models;
using Unity.Entities;

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
					if (dealDamageEvent.SpellSource.Exists())
					{
						var owner = dealDamageEvent.SpellSource.Read<EntityOwner>().Owner;
						if (owner.Exists())
						{
							if (owner.Has<EntityOwner>())
							{
								var minionOwner = owner.Read<EntityOwner>().Owner;
								if (minionOwner.Exists() && minionOwner.Has<PlayerCharacter>())
								{
									owner = minionOwner;
								}
							}
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
	//minions that spawn from other minions don't naturally preserve their link to the grandparent that spawned them, so we manually track it
	public static Dictionary<Entity, Entity> SummonToGrandparentPlayerCharacter = new();
	public static void Prefix(AiDamageTakenEventSystem __instance)
	{		
		var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var statChangeEvent = entity.Read<StatChangeEvent>();
				var source = statChangeEvent.Source;
				if (source.Exists() && source.Has<EntityOwner>())
				{
					if (statChangeEvent.StatType == StatType.Health)
					{
						var damageDealerEntity = source.Read<EntityOwner>().Owner;
						if (damageDealerEntity.Exists())
						{
							if (damageDealerEntity.Has<EntityOwner>())
							{
								var owner = damageDealerEntity.Read<EntityOwner>().Owner;
								if (owner.Exists()) //if it has a parent
								{
									damageDealerEntity = owner;
								}
								else if (SummonToGrandparentPlayerCharacter.TryGetValue(damageDealerEntity, out owner))
								{ 
									damageDealerEntity = owner;
								}
							}
							if (damageDealerEntity.Has<PlayerCharacter>())
							{
								var damageDealerPlayer = PlayerService.GetPlayerFromCharacter(damageDealerEntity);
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
									if (totalDamage > 0) continue;
									var damageInfo = new DamageInfo
									{
										TotalDamage = Math.Abs(totalDamage),
										CritDamage = Math.Abs(critDamage), //this isn't reliable, dmg can be higher without crit
										DamageAbsorbed = Math.Abs(damageShielded)
									};
									if (damageInfo.TotalDamage > 0)
									{
										GameEvents.RaisePlayerDamageReported(damageDealerPlayer, targetEntity, source.Read<PrefabGUID>(), damageInfo);
									}
								}
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
