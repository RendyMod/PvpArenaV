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

namespace PvpArena.Patches;


[HarmonyPatch(typeof(StartCharacterCraftingSystem), nameof(StartCharacterCraftingSystem.OnUpdate))]
public static class StartCharacterCraftingSystemPatch
{
	public static void Prefix(StartCharacterCraftingSystem __instance)
	{
		var entities = __instance._StartCharacterCraftItemEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);

			VWorld.Server.EntityManager.DestroyEntity(entity);
			player.ReceiveMessage($"Crafting is disabled.".Error());
		}
	}
}

//move this into game mode logic later
[HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
public static class StartCraftingSystemPatch
{
	public static void Prefix(StartCraftingSystem __instance)
	{
		__instance._StartCraftItemEventQuery.LogComponentTypes();
		var entities = __instance._StartCraftItemEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);

			VWorld.Server.EntityManager.DestroyEntity(entity);
			player.ReceiveMessage($"Crafting is disabled.".Error());
		}
	}
}



