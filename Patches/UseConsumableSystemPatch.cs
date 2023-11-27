using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Bloodstone.API;
using ProjectM.Network;
using PvpArena.Services;
using Unity.Entities;
using static ProjectM.AbilityCastStarted_SpawnPrefabSystem_Server;
using PvpArena.Data;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using PvpArena.GameModes;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(UseConsumableSystem), nameof(UseConsumableSystem.OnUpdate))]
public static class UseConsumableSystemPatch
{

	public static void Prefix(UseConsumableSystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var useItemEvent = entity.Read<UseItemEvent>();
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, fromCharacter.Character, useItemEvent.SlotIndex, out var item))
			{
				var Player = PlayerService.GetPlayerFromUser(fromCharacter.User);

				GameEvents.RaisePlayerUsedConsumable(Player, entity, item);
			}	
		}
			
		entities.Dispose();
	}
}
