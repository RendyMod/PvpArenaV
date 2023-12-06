using PvpArena.Data;
using Unity.Entities;
using ProjectM;
using System.Collections.Generic;
using Bloodstone.API;
using PvpArena;
using Unity.Collections;
using PvpArena.Services;
using PvpArena.Models;
using UnityEngine.TextCore;
using ProjectM.Network;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;

internal static class ItemCommands
{
	[Command("give-gearset", adminOnly: true)]
	public static void GiveGearSetCommand (Player sender, string kitType, Player _foundPlayer = null)
	{
		Player targetPlayer = sender;
		if (_foundPlayer != null)
		{
			targetPlayer = _foundPlayer;
		}
		if (!ItemSet.ItemSetDictionary.ContainsKey(kitType.ToLower()))
			return;

		ItemSet itemSetToSpawn = ItemSet.ItemSetDictionary[kitType.ToLower()];

		Helper.AddItemToInventory(targetPlayer.Character, itemSetToSpawn.chestPrefab, 1,
			out Entity itemChestEntity);
		Helper.AddItemToInventory(targetPlayer.Character, itemSetToSpawn.glovesPrefab, 1,
			out Entity itemGlovesEntity);
		Helper.AddItemToInventory(targetPlayer.Character, itemSetToSpawn.legsPrefab, 1,
			out Entity itemLegsEntity);
		Helper.AddItemToInventory(targetPlayer.Character, itemSetToSpawn.bootsPrefab, 1,
			out Entity itemBootsEntity);

		sender.ReceiveMessage(("Gave: " + kitType.Emphasize() + " set to " +
				   targetPlayer.Name.Emphasize() + ".").Success());
	}

	[Command("give-secretweapons", adminOnly: true)]
	public static void GiveSecretWeapons (Player sender, Player _foundPlayer)
	{
		Helper.AddItemToInventory(_foundPlayer.Character, Prefabs.Item_Weapon_Rapier_T05_Iron, 1,
			out Entity itemEntityRapier);
		Helper.AddItemToInventory(_foundPlayer.Character, Prefabs.Item_Weapon_NecromancyDagger_T08_Sanguine, 1,
			out Entity itemEntityDagger);
		Helper.AddItemToInventory(_foundPlayer.Character, Prefabs.Item_Weapon_Longbow_T08_Sanguine, 1,
			out Entity itemEntityLongbow);

		sender.ReceiveMessage(
			("Gave: " + "secret weapons".Emphasize() + " to " + _foundPlayer.Name.Emphasize() + ".").Success());
	}

	[Command(name:"give", description: "Gives the specified item to the player", usage:".give <PrefabGUID or name> [quantity=1]" , aliases: new string[] { "g" }, adminOnly: true)]
	public static void GiveItem (Player sender, ItemPrefabData item, int quantity = 1, Player player = null)
	{
		Player Player = player ?? sender;

		if (Helper.AddItemToInventory(Player.Character, item.PrefabGUID, quantity, out var entity))
		{
			var prefabSys = Core.prefabCollectionSystem;
			if (prefabSys.PrefabGuidToNameDictionary.TryGetValue(item.PrefabGUID, out var name))
			{
				sender.ReceiveMessage($"Gave : {quantity.ToString().Emphasize()} {name.White()} to {Player.Name.Emphasize()}"
				.Success());
			}
		}
	}

	[Command("remove-item", adminOnly: true)]
	public static void RemovePlayerItemFromInventory (Player sender, Player foundPlayer,
		PrefabGUID prefabGuid)
	{
		var inventoryResponse = InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager,
			foundPlayer.Character, prefabGuid, 1);
		if (inventoryResponse)
			sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " removed from " + foundPlayer.Name.Emphasize() +
			                                      "'s inventory.").Success());
		else
			sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize()  + " not removed from " + foundPlayer.Name.Emphasize() +
			          "'s inventory.").Error());
	}

	[Command("remove-item-from-everyone", adminOnly: true)]
	public static void RemoveItemFromAllInventories (Player sender, PrefabGUID prefabGuid)
	{
		bool inventoryResponse = false;
		foreach (var Player in PlayerService.CharacterCache.Values)
		{
			if (!Player.IsAdmin)
			{
				if (InventoryUtilitiesServer.TryRemoveItemFromInventories(VWorld.Server.EntityManager, Player.Character,
					    prefabGuid, 10))
				{
					inventoryResponse = true;
				}
			}
		}

		if (inventoryResponse)
			sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " removed from all inventories!").Success());
		else
			sender.ReceiveMessage(("Item " + prefabGuid.ToString().Emphasize() + " not removed from all inventories!").Error());
	}
	
	[Command("list-items", adminOnly: true)]
	public static void LogItems (Player sender, Player player)
	{
		ListItemsFromInventory(sender, player.Character);
	}
	public static void ListItemsFromInventory (Player sender, Entity _recipient)
	{
		List<string> itemNames = new List<string>();

		//I think this buffer contains ALL player inventories
		var buffer = _recipient.ReadBuffer<InventoryInstanceElement>();
		for (var i = 0; i < buffer.Length; i++)
		{
			var inventoryInstanceElement = buffer[i];
			if (inventoryInstanceElement.ExternalInventoryEntity._Entity.Exists())
			{
				//confirm the inventory belongs to the person
				if (inventoryInstanceElement.ExternalInventoryEntity._Entity.Read<InventoryConnection>()
					    .InventoryOwner == _recipient)
				{
					var inventoryBuffer = inventoryInstanceElement.ExternalInventoryEntity._Entity
						.ReadBuffer<InventoryBuffer>();
					foreach (var item in inventoryBuffer)
					{
						if (item.ItemEntity._Entity.Exists())
						{
							itemNames.Add(item.ItemEntity._Entity.LookupName().Split(" ")[0]);
						}
					}
				}
			}
		}

		var equipment = _recipient.Read<Equipment>();
		NativeList<Entity> equipmentEntities = new NativeList<Entity>(Allocator.Temp);
		equipment.GetAllEquipmentEntities(equipmentEntities);
		foreach (var equipmentEntity in equipmentEntities)
		{
			if (equipmentEntity.Exists())
			{
				if (equipmentEntity.Read<EquippableData>().EquipmentType != EquipmentType.Weapon)
				{
					itemNames.Add(equipmentEntity.LookupName().Split(" ")[0]);
				}
			}
		}

		equipmentEntities.Dispose();
		itemNames.Sort();
		foreach (var itemName in itemNames)
		{
			sender.ReceiveMessage(itemName.White());
		}
	}

	
}
