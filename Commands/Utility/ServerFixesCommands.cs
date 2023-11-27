using System.Collections.Generic;
using ProjectM;
using PvpArena;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Collections;
using Unity.Entities;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class ServerFixesCommands
{
	[Command("fix-spawns", description: "Used for debugging", adminOnly: true)]
	public void FixSpawnsCommand(Player sender)
	{
		var entities = Helper.GetPrefabEntitiesByComponentTypes<DestroyOnSpawn, Health>();
		List<PrefabGUID> PrefabsToIgnore = new List<PrefabGUID>
		{
			Prefabs.CHAR_Illusion_Mosquito,
			/*Prefabs.CHAR_Unholy_FallenAngel,*/
			Prefabs.CHAR_Unholy_DeathKnight,
			Prefabs.CHAR_Unholy_Baneling,
			Prefabs.CHAR_Unholy_SkeletonWarrior_Summon,
			Prefabs.CHAR_Unholy_SkeletonApprentice_Summon,
			Prefabs.CHAR_Spectral_Guardian,
			Prefabs.CHAR_Spectral_SpellSlinger,
			Prefabs.CHAR_NecromancyDagger_SkeletonBerserker_Armored_Farbane,
			Prefabs.CHAR_Mount_Horse,
			Prefabs.AB_Vampire_VeilOfFrost_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfIllusion_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfStorm_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfBlood_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfBones_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfChaos_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfChaos_SpellMod_BonusDummy,
			Prefabs.AB_Storm_BallLightning_Projectile,
			Prefabs.CHAR_Trader_Farbane_RareGoods_T01,
			Prefabs.CHAR_Trader_Dunley_RareGoods_T02,
			Prefabs.CHAR_Cursed_Witch_Exploding_Mosquito
		};
		foreach (var entity in entities)
		{
			Plugin.PluginLog.LogInfo($"Fixing: {entity.Read<PrefabGUID>().LookupName()}");
			if (!entity.Has<AggroConsumer>())
			{
				entity.Remove<DestroyOnSpawn>();
			}
		}
		sender.ReceiveMessage("Spawns should work now");
	}
	[Command("modify-prefabs", description: "To avoid restarting server when changing a prefab", adminOnly:true)]
	public static void ModifyPrefabsCommand(Player sender)
	{
		Plugin.ModifyPrefabs();
		sender.ReceiveMessage("Done!");
	}
	
	[Command("update-items", description: "Used for debugging", adminOnly: true)]
	public static void UpdateItemsCommand(Player sender)
	{
		var entities = Helper.GetEntitiesByComponentTypes<PlayerCharacter>(true);

		foreach (var entity in entities)
		{
			var equipment = entity.Read<Equipment>();
			NativeList<Entity> equipmentEntities = new NativeList<Entity>(Allocator.Temp);
			equipment.GetAllEquipmentEntities(equipmentEntities);
			equipment.UnequipItem(EquipmentType.MagicSource);
			equipment.UnequipItem(EquipmentType.Cloak);
			entity.Write(equipment);
			foreach (var equipmentEntity in equipmentEntities)
			{
				var equippableData = equipmentEntity.Read<EquippableData>();
				if (equippableData.EquipmentType == EquipmentType.Cloak)
				{
					var cloakPrefab = equipmentEntity.Read<PrefabGUID>();
					Helper.AddItemToInventory(entity, cloakPrefab, 1, out var itemEntity);
				}
				else if (equippableData.EquipmentType == EquipmentType.MagicSource)
				{
					var neckPrefab = equipmentEntity.Read<PrefabGUID>();
					Helper.AddItemToInventory(entity, neckPrefab, 1, out var itemEntity);
				}
			}
			equipmentEntities.Dispose();
		}
		sender.ReceiveMessage("Items updated!".Success());
	}
}
