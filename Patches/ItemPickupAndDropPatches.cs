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
using ProjectM.CastleBuilding;
using PvpArena.Helpers;
using ProjectM.CastleBuilding.Placement;
using Unity.Physics;
using ProjectM.Tiles;
using UnityEngine.Jobs;
using ProjectM.Shared;
using PvpArena.GameModes;
using PvpArena.Models;

namespace PvpArena.Patches;




[HarmonyPatch(typeof(DropItemThrowSystem), nameof(DropItemThrowSystem.OnUpdate))]
public static class DropItemThrowSystemPatch
{
	public static void Prefix(DropItemThrowSystem __instance)
	{
		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var dropItemAroundPosition = entity.Read<DropItemAroundPosition>();
			var onlinePlayers = PlayerService.OnlinePlayers.Keys;
			Player closestPlayerToEvent = null;
			foreach (var player in onlinePlayers)
			{
				if (math.all(math.abs(player.Position - dropItemAroundPosition.Position) < new float3(3f)))
				{
					closestPlayerToEvent = player;
					break;
				}
			}
			if (closestPlayerToEvent == null)
			{
				VWorld.Server.EntityManager.DestroyEntity(entity);
				continue;
			}
			else
			{
				GameEvents.RaiseItemWasThrown(closestPlayerToEvent, entity);
			}
		}
	}
}

/*[HarmonyPatch(typeof(ItemPickupSystem), nameof(ItemPickupSystem.OnUpdate))]
public static class ItemPickupSystemPatch
{
	public static void Prefix(ItemPickupSystem __instance)
	{
		__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			entity.LogComponentTypes();
		}
	}
}*/

/*[HarmonyPatch(typeof(DropInventoryItemSystem), nameof(DropInventoryItemSystem.OnUpdate))]
public static class DropInventoryItemSystemPatch
{
	public static void Prefix(DropInventoryItemSystem __instance)
	{
		__instance._Query.LogComponentTypes();
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var dropItemEvent = entity.Read<DropInventoryItemEvent>();
			Unity.Debug.Log(dropItemEvent.SlotIndex);
			Helper.networkIdSystem._NetworkIdToEntityMap[dropItemEvent.Inventory].LogComponentTypes();
			entity.LogComponentTypes();
		}
	}

}
*/