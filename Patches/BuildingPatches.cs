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
using PvpArena.Models;
using static ProjectM.Terrain.MapMaker.MapMakerDefinition;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
public static class BuildingPermissions
{
	public static void Prefix(PlaceTileModelSystem __instance)
	{
		var entities = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			if (!player.IsAdmin)
			{
				player.ReceiveMessage($"You do not have building permissions".Error());
				VWorld.Server.EntityManager.DestroyEntity(entity);
			}
		}
		entities.Dispose();

		entities = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			if (!player.IsAdmin)
			{
				player.ReceiveMessage($"You do not have building permissions".Error());
				VWorld.Server.EntityManager.DestroyEntity(entity);
			}
		}
		entities.Dispose();

		entities = __instance._MoveTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			if (!player.IsAdmin)
			{
				player.ReceiveMessage($"You do not have building permissions".Error());
				VWorld.Server.EntityManager.DestroyEntity(entity);
			}
		}
		entities.Dispose();

		entities = __instance._BuildWallpaperQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			if (!player.IsAdmin)
			{
				player.ReceiveMessage($"You do not have building permissions".Error());
				VWorld.Server.EntityManager.DestroyEntity(entity);
			}
		}
		entities.Dispose();
	}
}
