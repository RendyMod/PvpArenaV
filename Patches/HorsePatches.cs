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
using PvpArena.GameModes;
using PvpArena.Services;

namespace PvpArena.Patches;

//this is all just to make slow horses
[HarmonyPatch(typeof(GallopBuffSystem_Destroy), nameof(GallopBuffSystem_Destroy.OnUpdate))]
public static class GallopBuffSystem_DestroyPatch
{
	
	public static void Prefix(GallopBuffSystem_Destroy __instance)
	{	
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var player = PlayerService.GetPlayerFromCharacter(entity.Read<EntityOwner>().Owner);
			GameEvents.RaisePlayerWillLoseGallopBuff(player, entity);
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(MountBuffSpawnSystem_Shared), nameof(MountBuffSpawnSystem_Shared.OnUpdate))]
public static class MountBuffSpawnSystem_SharedPatch
{
	public static void Prefix(MountBuffSpawnSystem_Shared __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var character = entity.Read<EntityOwner>().Owner;
			var player = PlayerService.GetPlayerFromCharacter(character);
			GameEvents.RaisePlayerMounted(player, entity);
		}
		entities.Dispose();
	}
}


[HarmonyPatch(typeof(MountBuffDestroySystem_Shared), nameof(MountBuffDestroySystem_Shared.OnUpdate))]
public static class MountBuffDestroySystem_SharedPatch
{
	public static void Prefix(MountBuffDestroySystem_Shared __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var character = entity.Read<EntityOwner>().Owner;
			var player = PlayerService.GetPlayerFromCharacter(character);
			GameEvents.RaisePlayerDismounted(player, entity);
		}
		entities.Dispose();
	}
}
