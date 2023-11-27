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

namespace PvpArena.Patches;

[HarmonyPatch(typeof(DealDamageSystem), nameof(DealDamageSystem.OnUpdate))]
public static class DealDamageSystemPatch
{
	public static void Prefix(DealDamageSystem __instance)
	{

		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
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
		}
	}
}
