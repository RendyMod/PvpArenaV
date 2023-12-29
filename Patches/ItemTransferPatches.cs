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
using UnityEngine.Jobs;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(MoveItemBetweenInventoriesSystem), nameof(MoveItemBetweenInventoriesSystem.OnUpdate))]
public static class MoveItemBetweenInventoriesSystemPatch
{
	public static void Prefix(MoveItemBetweenInventoriesSystem __instance)
	{

		var entities = __instance._MoveItemBetweenInventoriesEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var moveItemBetweenInventoriesEvent = entity.Read<MoveItemBetweenInventoriesEvent>();
			if (Core.networkIdSystem._NetworkIdToEntityMap.TryGetValue(moveItemBetweenInventoriesEvent.ToInventory, out var targetInventory))
			{
				var targetPrefabGUID = targetInventory.Read<PrefabGUID>();
				if (!targetInventory.Has<InventoryItem>() && targetPrefabGUID != Prefabs.External_Inventory && targetPrefabGUID != Prefabs.CHAR_VampireMale) //dont block moving into bags or into their own inventory
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					player.ReceiveMessage("Transferring items is disabled".Error());
					entity.Destroy();
				}
			}
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(SmartMergeItemsBetweenInventoriesSystem), nameof(SmartMergeItemsBetweenInventoriesSystem.OnUpdate))]
public static class SmartMergeItemsBetweenInventoriesSystemPatch
{
	public static void Prefix(SmartMergeItemsBetweenInventoriesSystem __instance)
	{
		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			player.ReceiveMessage("Transferring items is disabled".Error());
			entity.Destroy();
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(UnEquipItemSystem), nameof(UnEquipItemSystem.OnUpdate))]
public static class UnEquipItemSystemPatch
{
	public static void Prefix(UnEquipItemSystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var unequipItemEvent = entity.Read<UnequipItemEvent>();
			if (Core.networkIdSystem._NetworkIdToEntityMap.TryGetValue(unequipItemEvent.ToInventory, out var targetInventory))
			{
				var targetPrefabGUID = targetInventory.Read<PrefabGUID>();

				if (!targetInventory.Has<InventoryItem>() && targetPrefabGUID != Prefabs.External_Inventory && targetPrefabGUID != Prefabs.CHAR_VampireMale)
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					player.ReceiveMessage("Transferring items is disabled".Error());
					entity.Destroy();
				}
				else if (targetPrefabGUID == Prefabs.External_Inventory)
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					if (InventoryUtilities.GetFreeSlotsCount(VWorld.Server.EntityManager, player.Character) == 0)
					{
						entity.Destroy();
					}
				}
			}
		}
		entities.Dispose();
	}
}

[HarmonyPatch(typeof(UnEquipBagIntoInventorySystem), nameof(UnEquipBagIntoInventorySystem.OnUpdate))]
public static class UnEquipBagIntoInventorySystemPatch
{
	public static void Prefix(UnEquipBagIntoInventorySystem __instance)
	{
		var entities = __instance._EventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var unequipItemEvent = entity.Read<UnequipBagIntoInventoryEvent>();
			if (Core.networkIdSystem._NetworkIdToEntityMap.TryGetValue(unequipItemEvent.ToInventory, out var targetInventory))
			{
				var targetPrefabGUID = targetInventory.Read<PrefabGUID>();

				if (!targetInventory.Has<InventoryItem>() && targetPrefabGUID != Prefabs.External_Inventory && targetPrefabGUID != Prefabs.CHAR_VampireMale)
				{
					var fromCharacter = entity.Read<FromCharacter>();
					var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
					player.ReceiveMessage("Transferring items is disabled".Error());
					entity.Destroy();
				}
			}
		}
		entities.Dispose();
	}
}
