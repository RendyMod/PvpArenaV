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

namespace PvpArena.Patches;
/*
[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.HandleBuildTileModelEvents))]
public static class PlaceTileModelSystemPatch
{
	public static void Prefix(PlaceTileModelSystem __instance, CollisionWorld collisionWorld, NativeHashMap<NetworkId, Entity> networkIdToEntityMap, GetPlacementResult.SystemData.PrepareJobData prepareJobData)
	{
		var entities = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var buildTileModelEvent = entity.Read<BuildTileModelEvent>();
			//entity.LogComponentTypes();
			//buildTileModelEvent.PrefabGuid.LogPrefabName();
		}
	}
}
*/
