using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Mathematics;
using Bloodstone.API;
using PvpArena.Services;
using PvpArena.GameModes;
using PvpArena.Models;
using System;
using PvpArena.Data;
using PvpArena.Listeners;
using Unity.Entities;
using ProjectM.Network;

namespace PvpArena.Patches;



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

[HarmonyPatch(typeof(DropInventoryItemSystem), nameof(DropInventoryItemSystem.OnUpdate))]
public static class DropInventoryItemSystemPatch
{
	public static void Prefix(DropInventoryItemSystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var dropItemEvent = entity.Read<DropInventoryItemEvent>();
			var inventoryEntity = Core.networkIdSystem._NetworkIdToEntityMap[dropItemEvent.Inventory];
			var fromCharacter = entity.Read<FromCharacter>();
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, inventoryEntity, dropItemEvent.SlotIndex, out var inventoryBuffer))
			{
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				GameEvents.RaiseItemWasDropped(player, entity, inventoryBuffer.ItemType);
			}
		}
	}

}

