using PvpArena.Commands;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Network;
using PvpArena.Matchmaking;
using PvpArena.Services;
using ProjectM.UI;
using ProjectM.LightningStorm;
using ProjectM.Sequencer;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using ProjectM.Debugging;
using static ProjectM.Gameplay.Systems.DealDamageSystem;
using System.Runtime.InteropServices;
using ProjectM.CastleBuilding;
using ProjectM.Presentation;
using PvpArena.GameModes;
using PvpArena.Listeners;
using PvpArena.Factories;
using PvpArena.Helpers;
using UnityEngine.Jobs;
using ProjectM.Behaviours;
using Epic.OnlineServices.AntiCheatCommon;
using ProjectM.Gameplay.Scripting;
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
				if (dealDamageEvent.Target.Read<PrefabGUID>() == Prefabs.CHAR_TargetDummy_Footman)
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
			}
			catch
			{
				Plugin.PluginLog.LogInfo("An error occurred in the deal damage system");
			}
		}
	}
}

/*public class ScrollingCombatTextListener : EntityQueryListener
{
	
	public void OnNewMatchFound(Entity entity)
	{
		var sct = entity.Read<ScrollingCombatTextMessage>();
		if (sct.Value == 0) { return; }
		
		if (sct.Source._Entity.Has<PlayerCharacter>() && sct.Type == Prefabs.SCT_Type_Absorb && (sct.Target._Entity.Has<PlayerCharacter>() || (sct.Target._Entity.Read<PrefabGUID>() == Dummy.PrefabGUID)))
		{
			var sourceEntity = sct.Source._Entity;
			var sourcePlayer = PlayerService.GetPlayerFromCharacter(sourceEntity);

			var targetEntity = sct.Target._Entity;
			GameEvents.RaisePlayerDamageReported(sourcePlayer, targetEntity, sct.Type, sct.Value);
		}
	}

	public void OnNewMatchRemoved(Entity entity)
	{

	}
}*/


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
