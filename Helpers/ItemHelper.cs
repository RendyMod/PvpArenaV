using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using PvpArena.Services;
using PvpArena.Models;
using PvpArena.Data;
using PvpArena.Configs;
using Il2CppSystem;
using Unity.Physics;
using Unity.Jobs;
using UnityEngine.Jobs;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Epic.OnlineServices;

namespace PvpArena.Helpers;

public static partial class Helper
{
	public static List<EquipmentType> EquipmentTypes = new List<EquipmentType> 
	{
		EquipmentType.Headgear,
		EquipmentType.Chest,
		EquipmentType.Weapon,
		EquipmentType.MagicSource,
		EquipmentType.Footgear,
		EquipmentType.Legs,
		EquipmentType.Cloak,
		EquipmentType.Gloves,
	};

	//this is horribly inefficient, don't use this outside of one-off scripts
	public static bool TryFindOwnerOfItem(Entity itemEntity, out Player itemOwner, out int slot)
	{
		var allPlayers = PlayerService.UserCache.Values;
		foreach (var player in allPlayers)
		{
			for (var i = 0; i < 36; i++)
			{
				if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, i, out InventoryBuffer item))
				{
					if (item.ItemEntity._Entity == itemEntity)
					{
						itemOwner = player;
						slot = i;
						return true;
					}
				}
			}
		}
		itemOwner = default;
		slot = -1;
		return false;
	}

	public static void ClearInventory(Entity Character, bool all = false)
	{
		int start = 9;
		if (all)
		{
			start = 0;
		}

		for (int i = start; i < 36; i++)
		{
			InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, Character, i);
		}
	}

	public static void RepairGear(Entity Character, bool repair = true)
	{
		Equipment equipment = Character.Read<Equipment>();
		NativeList<Entity> equippedItems = new NativeList<Entity>(Allocator.Temp);
		equipment.GetAllEquipmentEntities(equippedItems);
		foreach (var equippedItem in equippedItems)
		{
			if (equippedItem.Has<Durability>())
			{
				var durability = equippedItem.Read<Durability>();
				if (repair)
				{
					durability.Value = durability.MaxDurability;
				}
				else
				{
					durability.Value = 0;
				}

				equippedItem.Write(durability);
			}
		}

		equippedItems.Dispose();

		for (int i = 0; i < 36; i++)
		{
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, Character, i,
					out InventoryBuffer item))
			{
				var itemEntity = item.ItemEntity._Entity;
				if (itemEntity.Has<Durability>())
				{
					var durability = itemEntity.Read<Durability>();
					if (repair)
					{
						durability.Value = durability.MaxDurability;
					}
					else
					{
						durability.Value = 0;
					}

					itemEntity.Write(durability);
				}
			}
		}
	}

	public static void ClearInventorySlot(Player player, int itemSlot)
	{
		ClearInventorySlot(player.Character, itemSlot);
	}

	public static void ClearInventorySlot(Entity inventoryEntity, int itemSlot)
	{
		InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, inventoryEntity, itemSlot);
	}

	public static void RemoveItemAtSlotFromInventory(Player player, PrefabGUID itemPrefab, int itemSlot)
	{
		RemoveItemAtSlotFromInventory(player.Character, itemPrefab, itemSlot);
	}

	public static void RemoveItemAtSlotFromInventory(Entity inventoryEntity, PrefabGUID itemPrefab, int itemSlot)
	{
		if (Helper.GetPrefabEntityByPrefabGUID(itemPrefab).Has<Relic>())
		{
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, inventoryEntity, itemSlot, out InventoryBuffer item))
			{
				if (item.ItemEntity._Entity.Exists())
				{
					Helper.DestroyEntity(item.ItemEntity._Entity);
				}
			}
		}
		ClearInventorySlot(inventoryEntity, itemSlot);
	}

	public static void CompletelyRemoveItemFromInventory(Player player, PrefabGUID itemPrefab)
	{
		while (Helper.GetPrefabEntityByPrefabGUID(itemPrefab).Has<Relic>() && InventoryUtilities.TryGetItemSlot(VWorld.Server.EntityManager, player.Character, itemPrefab, out var slot))
		{
			RemoveItemAtSlotFromInventory(player, itemPrefab, slot);
		}
		InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, player.Character, itemPrefab, 100000);
	}

	public static void RemoveItemFromInventory(Player player, PrefabGUID itemPrefab, int quantity = 1)
	{
		InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, player.Character, itemPrefab, quantity);
	}

	public static bool PlayerHasItemInInventories(Player player, PrefabGUID itemPrefab)
	{
		NativeList<Entity> inventories = new NativeList<Entity>(Allocator.Temp);
		InventoryUtilities.TryGetInventoryEntities(VWorld.Server.EntityManager, player.Character, ref inventories);
		return InventoryUtilities.HasItemInInventories(VWorld.Server.EntityManager, inventories, itemPrefab, 1);
	}

	//this won't get picked up by our current OnDrop listeners -- modify this to use DropInventoryItemEvent in the future
	public static void DropItemFromInventory(Player player, PrefabGUID item)
	{
		var entity = Helper.CreateEntityWithComponents<DropItemAroundPosition, FromCharacter>();
		var slot = InventoryUtilities.GetItemSlot(VWorld.Server.EntityManager, player.Character, item);
		if (slot > -1)
		{
			InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, slot, out var invBuffer);

			entity.Write(player.ToFromCharacter());
			entity.Write(new DropItemAroundPosition
			{
				ItemEntity = invBuffer.ItemEntity._Entity,
				ItemHash = invBuffer.ItemType,
				Amount = invBuffer.Amount,
				Position = player.Position
			});
			var action = () =>
			{
				Helper.ClearInventorySlot(player, slot); //delay this slightly so that the event has time to process
			};
			ActionScheduler.RunActionOnceAfterDelay(action, .1);	
		}
	}


	public static bool AddItemToInventory(Entity recipient, string needle, int amount, out Entity entity,
		bool equip = true)
	{
		if (TryGetItemPrefabDataFromString(needle, out PrefabData prefab))
		{
			return AddItemToInventory(recipient, prefab.PrefabGUID, amount, out entity, equip);
		}

		entity = default;
		return false;
	}


	public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount, out Entity entity,
		bool equip = true, int slot = 0)
	{
		var gameData = VWorld.Server.GetExistingSystem<GameDataSystem>();
		var itemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, gameData.ItemHashLookupMap);
		itemSettings.EquipIfPossible = equip;
		itemSettings.StartIndex = new Nullable_Unboxed<int>(slot);
		var inventoryResponse = InventoryUtilitiesServer.TryAddItem(itemSettings, recipient, guid, amount);
		if (inventoryResponse.Success)
		{
			entity = inventoryResponse.NewEntity;
			return true;
		}
		else
		{
			entity = new Entity();
			return false;
		}
	}

	public static void GiveStartingGear(Player player)
	{
		GiveArmorAndNecks(player);

		var action = new ScheduledAction(GiveDefaultLegendaries, new object[] { player });
		ActionScheduler.ScheduleAction(action, 3);
	}

	
	public static void GiveBags(Player player)
	{
		for (var i = 0; i < 4; i++)
		{
			AddItemToInventory(player.Character, Prefabs.Item_Bag_Grand_Coins, 1, out Entity itemEntity);
			for (var j = 0; j < 36; j++) {
				if (TryGetItemAtSlot(player, j, out var item))
				{
					if (item.ItemType == Prefabs.Item_Bag_Grand_Coins)
					{
						EquipBagAtSlot(player, j, i);
					}
				}
			}
		}
	}

	public static void GiveArmorAndNecks(Player player)
	{
		var equip = true;
		foreach (var item in Kit.Necks)
		{
			if (equip)
			{
				AddItemToInventory(player.Character, item, 1, out Entity itemEntity, true);
				equip = false;
			}
			else
			{
				AddItemToInventory(player.Character, item, 1, out Entity itemEntity, false);
			}
		}

		foreach (var item in Kit.StartingGear)
		{
			AddItemToInventory(player.Character, item, 1, out Entity itemEntity);
		}

		/*
		for (var i = 0; i < 4; i++)
		{
			AddItemToInventory(player.Character, Prefabs.Item_Bag_Grand_Coins, 1, out Entity itemEntity);
			for (var j = 0; j < 36; j++) {
				if (TryGetItemAtSlot(player, j, out var item))
				{
					if (item.ItemType == Prefabs.Item_Bag_Grand_Coins)
					{
						EquipBagAtSlot(player, j, i);
					}
				}
			}
		}
		*/
	}

	public static bool TryGetItemAtSlot(Player player, int slot, out InventoryBuffer item)
	{
		return InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, slot, out item);
	}

	public static void EquipJewelAtSlot(Player player, int inventoryIndex)
	{
		Entity equipJewelEventEntity = VWorld.Server.EntityManager.CreateEntity(
			ComponentType.ReadWrite<FromCharacter>(),
			ComponentType.ReadWrite<EquipJewelEvent>()
		);
		equipJewelEventEntity.Write(player.ToFromCharacter());
		equipJewelEventEntity.Write(new EquipJewelEvent
		{
			InventoryIndex = inventoryIndex,
			FromInventory = true
		});
	}

	public static void EquipBagAtSlot(Player player, int inventoryIndex, int targetBagSlot)
	{
		Entity equipBagEventEntity = VWorld.Server.EntityManager.CreateEntity(
			ComponentType.ReadWrite<FromCharacter>(),
			ComponentType.ReadWrite<EquipBagEvent>()
		);
		equipBagEventEntity.Write(player.ToFromCharacter());
		equipBagEventEntity.Write(new EquipBagEvent
		{
			FromInventoryIndex = inventoryIndex,
			ToBagSlotIndex = targetBagSlot
		});
	}

	public static void ClearAllItems(Player player)
	{
		var inventoryEntities = new NativeList<Entity>(Allocator.Temp);
		InventoryUtilities.TryGetInventoryEntities(VWorld.Server.EntityManager, player.Character, ref inventoryEntities);
		foreach (var inventoryEntity in inventoryEntities)
		{
			InventoryUtilitiesServer.ClearInventory(VWorld.Server.EntityManager, inventoryEntity);
		}
		var equipment = player.Character.Read<Equipment>();
		foreach (var equipmentType in EquipmentTypes)
		{
			equipment.UnequipItem(equipmentType);
		}
		player.Character.Write(equipment);
	}

	public static void UnequipItem(Player player, EquipmentType equipmentType, int slot = 0)
	{
		var entity = Helper.CreateEntityWithComponents<FromCharacter, UnequipItemEvent>();
		entity.Write(player.ToFromCharacter());
		entity.Write(new UnequipItemEvent
		{
			EquipmentType = equipmentType,
			ToInventory = player.Character.Read<NetworkId>(),
			ToSlotIndex = slot
		});
	}

	public static void UnequipAllItems(Player player)
	{
		for (var i = 0; i < EquipmentTypes.Count; i++)
		{
			Helper.UnequipItem(player, EquipmentTypes[i], i);
		}
	}

	//this assumes that the target inventories are empty
	public static void TransferAllPlayerItems(Player player, List<Entity> targetInventories)
	{
		int inventoryIndex;
		var inventoryEntities = new NativeList<Entity>(Allocator.Temp);
		InventoryUtilities.TryGetInventoryEntities(VWorld.Server.EntityManager, player.Character, ref inventoryEntities);
		foreach (var inventoryEntity in inventoryEntities)
		{
			if (!inventoryEntity.Exists()) continue;

			if (inventoryEntity.Read<PrefabGUID>() == Prefabs.External_Inventory)
			{
				inventoryIndex = 0;
			}
			else
			{
				inventoryIndex = 1;
			}
			var buffer = inventoryEntity.ReadBuffer<InventoryBuffer>();

			var index = 0;
			foreach (var item in buffer)
			{
				if (item.ItemEntity._Entity.Exists())
				{
					var result = InventoryUtilitiesServer.Internal_TryMoveItem(VWorld.Server.EntityManager, Core.gameDataSystem.ItemHashLookupMap, inventoryEntity, index, targetInventories[inventoryIndex]);
					Plugin.PluginLog.LogInfo($"{result.Result} {inventoryIndex}");
				}
				index++;
			}
		}
		Helper.UnequipAllItems(player);
		var buffer2 = inventoryEntities[0].ReadBuffer<InventoryBuffer>();
		var index2 = 0;
		foreach (var item in buffer2)
		{
			if (item.ItemEntity._Entity.Exists())
			{
				var result = InventoryUtilitiesServer.Internal_TryMoveItem(VWorld.Server.EntityManager, Core.gameDataSystem.ItemHashLookupMap, inventoryEntities[0], index2, targetInventories[1]);
			}
			index2++;
		}

	}

	public static void RetrieveAllItems(Player player, List<Entity> sourceInventories)
	{
		// Iterate over each chest and move its items to the player's inventory
		foreach (var chestInventory in sourceInventories)
		{
			var chestBuffer = chestInventory.ReadBuffer<InventoryBuffer>();
			for (int i = 0; i < chestBuffer.Length; i++)
			{
				var item = chestBuffer[i];
				if (item.ItemEntity._Entity.Exists())
				{
					// Attempt to transfer the item directly to the player's character
					var result = InventoryUtilitiesServer.Internal_TryMoveItem(
						VWorld.Server.EntityManager,
						Core.gameDataSystem.ItemHashLookupMap,
						chestInventory,
						i,
						player.Character);

					// Handle the result as needed (e.g., log success or failure)
				}
			}
		}
	}


}
