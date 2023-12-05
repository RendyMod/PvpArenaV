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

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
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
		InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, player.Character, itemSlot);
	}

	public static void RemoveItemAtSlotFromInventory(Player player, PrefabGUID itemPrefab, int itemSlot)
	{
		if (Helper.GetPrefabEntityByPrefabGUID(itemPrefab).Has<Relic>())
		{	
			if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, itemSlot, out InventoryBuffer item))
			{
				//InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, player.Character, itemSlot);
				if (item.ItemEntity._Entity.Exists())
				{
					Helper.DestroyEntity(item.ItemEntity._Entity);
				}
				else
				{
					ClearInventorySlot(player, itemSlot);
				}
			}
		}
		
		InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, player.Character, itemPrefab, 10); //this doesn't match the method name, fix later
		
	}

	public static void RemoveItemFromInventory(Player player, PrefabGUID itemPrefab)
	{
		if (InventoryUtilities.TryGetItemSlot(VWorld.Server.EntityManager, player.Character, itemPrefab, out var slot))
		{
			RemoveItemAtSlotFromInventory(player, itemPrefab, slot);
		}
	}

	public static bool PlayerHasItemInInventories(Player player, PrefabGUID itemPrefab)
	{
		NativeList<Entity> inventories = new NativeList<Entity>(Allocator.Temp);
		InventoryUtilities.TryGetInventoryEntities(VWorld.Server.EntityManager, player.Character, ref inventories);
		return InventoryUtilities.HasItemInInventories(VWorld.Server.EntityManager, inventories, itemPrefab, 1);
	}

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
		bool equip = true)
	{
		var gameData = VWorld.Server.GetExistingSystem<GameDataSystem>();
		var itemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, gameData.ItemHashLookupMap);
		itemSettings.EquipIfPossible = equip;
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
}
