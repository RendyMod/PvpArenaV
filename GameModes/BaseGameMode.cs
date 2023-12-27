using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using PvpArena.Configs;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Patches;
using Unity.Entities;
using Unity.Mathematics;
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes;
public abstract class BaseGameMode
{
	public abstract void Initialize();
	public abstract void Dispose();
	public virtual void HandleOnPlayerDowned(Player player, Entity killer) { }
	public virtual void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
		player.ReceiveMessage("You died in an unexpected way. Let an admin know what happened!".Warning());
		Plugin.PluginLog.LogInfo($"{player.Name} has died in an unexpected way. Current game mode: {player.CurrentState}");
		player.CurrentState = Player.PlayerState.Normal;
		Helper.RevivePlayer(player, PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
		player.Reset();
		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
	}
	//public abstract void HandleOnPlayerRespawn(Player entity); 
	public virtual void HandleOnShapeshift(Player player, Entity eventEntity) { }
	public virtual void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
		}
	}
	public virtual void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		player.CurrentState = Player.PlayerState.Normal;
		player.Reset(Helper.ResetOptions.FreshMatch);
		player.TeleportToOfflinePosition();
	}
	public virtual void HandleOnItemWasDropped(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex)
    {
        if (player.CurrentState != PlayerGameModeType) return;

		var inventoryEntity = player.Character;
		if (eventEntity.Has<DropInventoryItemEvent>())
		{
			var dropInventoryItemEventData = eventEntity.Read<DropInventoryItemEvent>();
			Core.networkIdSystem._NetworkIdToEntityMap.TryGetValue(dropInventoryItemEventData.Inventory, out inventoryEntity);
		}

        Helper.RemoveItemAtSlotFromInventory(inventoryEntity, itemType, slotIndex);
		eventEntity.Destroy();
    }
	public virtual void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
    {
		if (player.CurrentState != PlayerGameModeType) return;

		if (!eventEntity.Exists()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();

		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			eventEntity.Destroy();
		}
	}

	public virtual void HandleOnUnitDamageDealt(Entity unit, Entity eventEntity)
	{
		if (!UnitFactory.HasGameMode(unit, UnitGameModeType)) return;
		if (!eventEntity.Exists()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();

		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			eventEntity.Destroy();
		}
	}

	public virtual void HandleOnPlayerPlacedStructure(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		if (!player.IsAdmin && !BuildingPermissions.AuthorizedBuilders.ContainsKey(player))
		{
			player.ReceiveMessage($"You do not have building permissions".Error());
			eventEntity.Destroy();
		}
	}

	public virtual void HandleOnPlayerPurchasedItem(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
		if (!eventEntity.Exists()) return;

		var purchaseEvent = eventEntity.Read<TraderPurchaseEvent>();
		Entity trader = Core.networkIdSystem._NetworkIdToEntityMap[purchaseEvent.Trader];
		TraderPurchaseSystemPatch.RefillStock(purchaseEvent, trader);
	}
    public virtual Player.PlayerState PlayerGameModeType { get; }
	public virtual string UnitGameModeType { get; set; }

	protected static HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"kit legendary",
		"jewel",
		"forfeit",
		"points",
		"lb ranked",
		"lb bullet",
		"bp",
		"tp-list",
	};
	public static HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}

	public static Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = true,
		RemoveShapeshifts = true
	};
}

