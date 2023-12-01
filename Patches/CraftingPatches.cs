using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Bloodstone.API;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
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



