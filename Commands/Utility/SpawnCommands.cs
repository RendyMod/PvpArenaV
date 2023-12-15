using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Network;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using static PvpArena.Helpers.Helper;

namespace PvpArena.Commands.Utility;
internal class SpawnCommands
{
	[Command("spawn-turret", description: "Spawns a turret at your location", adminOnly: true)]
	public static void SpawnTurretCommand(Player sender, PrefabGUID _prefab, int spawnSnapMode = 5)
	{
		var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
		var turret = new Turret(_prefab);
		UnitFactory.SpawnUnit(turret, spawnPosition);
		sender.ReceiveMessage($"Spawned turret!".Success());
	}

	[Command("spawn-dummy-prefab", description: "Spawns a dummy at your location", adminOnly: true)]
	public static void SpawnDummyCommand(Player sender, PrefabGUID prefabGuid, int spawnSnapMode = 5)
	{
		var dummy = new Dummy(prefabGuid, true);
		
		var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
		UnitFactory.SpawnUnit(dummy, spawnPosition);
		sender.ReceiveMessage($"Spawned dummy!".Success());
	}

	[Command("spawn-dummy", description: "Spawns a dummy at your location", adminOnly: true)]
	public static void SpawnDummyCommand(Player sender, int spawnSnapMode = 5)
	{
		var dummy = new Dummy();
		
		var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
		UnitFactory.SpawnUnit(dummy, spawnPosition);
		sender.ReceiveMessage($"Spawned dummy!".Success());
	}

	[Command("spawn-boss", description: "Spawns a boss at your location", adminOnly: true)]
	public static void SpawnBossCommand(Player sender, PrefabGUID _prefab, int level = 100, int spawnSnapMode = 5, int hp = -1, bool rooted = false)
	{
		var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
		var boss = new Boss(_prefab);
		boss.IsRooted = rooted;
		if (hp > 0)
		{
			boss.MaxHealth = hp;
		}
		
		boss.Level = level;
		boss.AggroRadius = 3;
		boss.IsRooted = false;
		boss.MaxDistanceFromPreCombatPosition = 20;
		UnitFactory.SpawnUnit(boss, spawnPosition);
		sender.ReceiveMessage($"Spawned boss!".Success());
	}

	[Command("spawn-vampire", description: "Spawns a boss at your location", adminOnly: true)]
	public static void SpawnVampireCommand(Player sender, int team = 10, int level = 84)
	{
		PrefabSpawnerService.SpawnWithCallback(Prefabs.CHAR_VampireMale, sender.Position, (System.Action<Entity>)((e) => {
			e.Remove<PlayerCharacter>();
			e.Remove<RespawnCharacter>();
			e.Remove<PlayerMapIcon>();
			e.Remove<Immortal>();
			e.Add<BuffResistances>();
			e.Write(new BuffResistances
			{
				SettingsEntity = ModifiableEntity.CreateFixed(Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_Golem)),
				InitialSettingGuid = Prefabs.BuffResistance_Golem
			});
			Helper.BuffEntity(e, Helper.CustomBuff, out var buffEntity, (float)Helper.NO_DURATION, false);
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision);
		}), 0, -1);
		sender.ReceiveMessage($"Spawned vampire!".Success());
	}

	[Command("spawn-prefab", description: "Used for debugging", adminOnly: true)]
	public static void SpawnPrefabCommand(Player sender, PrefabGUID _prefab, int rotationMode = 1, int spawnSnapMode = 5)
	{
		var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
		PrefabSpawnerService.SpawnWithCallback(_prefab, spawnPosition, (System.Action<Entity>)((Entity e) =>
		{
			e.LogComponentTypes();
			sender.ReceiveMessage("Spawned prefab!".Success());
		}), rotationMode);
	}

	[Command("destroy-prefab", description: "Used for debugging", adminOnly: true)]
	public static void DestroyPrefabCommand(Player sender, PrefabGUID prefab = default)
	{
		if (prefab != default)
		{
			var entities = Helper.GetEntitiesNearPosition(sender, 100);
			foreach (var entity in entities)
			{
				if (entity.Has<User>()) continue;
				if (!entity.Has<PhysicsCollider>()) continue;
				if (entity.Read<PrefabGUID>() == prefab)
				{
					Helper.DestroyEntity(entity);
					sender.ReceiveMessage($"Killed entity: {entity.Read<PrefabGUID>().LookupName()}".Success());
				}
			}
		}
		else
		{
			Entity entity = Helper.GetHoveredEntity(sender.User);
			Helper.DestroyEntity(entity);
			sender.ReceiveMessage($"Killed entity: {entity.Read<PrefabGUID>().LookupName()}".Success());
		}
	}

	[Command("spawn-chest", usage: ".spawn-chest rage", adminOnly: true)]
	public static void SpawnChestCommand(Player sender, string potion, int rotationMode = 1, int quantity = 1)
	{
		var entity = Helper.GetHoveredEntity(sender.User);

		PrefabSpawnerService.SpawnWithCallback(Prefabs.TM_WorldChest_Epic_01_Full, sender.Position, (Entity e) =>
		{
			e.Remove<DestroyAfterTimeOnInventoryChange>();
			e.Remove<DropInInventoryOnSpawn>();
			if (potion == "rage")
			{
				Helper.AddItemToInventory(e, Prefabs.Item_Consumable_GlassBottle_PhysicalBrew_T02, quantity, out Entity itemEntity);
			}
			else
			{
				Helper.AddItemToInventory(e, Prefabs.Item_Consumable_GlassBottle_SpellBrew_T02, quantity, out Entity itemEntity);
			}
			sender.ReceiveMessage("Spawned chest!".Success());
		}, rotationMode);
	}

	[Command("spawn-merchant", usage: ".spawn-chest rage", adminOnly: true)]
	public static void SpawnMerchantCommand(Player sender, string merchantName, int spawnSnapMode = 5)
	{
		var spawnPosition = Helper.GetSnappedHoverPosition(sender, (SnapMode)spawnSnapMode);
		UnitFactory.SpawnMerchant(merchantName, spawnPosition);
	}
}
