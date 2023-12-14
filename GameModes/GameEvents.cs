using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Network;
using PvpArena.Factories;
using PvpArena.Models;
using Unity.Entities;
using static DamageRecorderService;
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes;
public static class GameEvents
{
	public delegate void PlayerRespawnHandler(Player player);
	public static event PlayerRespawnHandler OnPlayerRespawn;

	public delegate void PlayerDeathHandler(Player player, DeathEvent deathEvent);
	public static event PlayerDeathHandler OnPlayerDeath;

	public delegate void PlayerDownedHandler(Player player, Entity killer);
	public static event PlayerDownedHandler OnPlayerDowned;

	public delegate void PlayerShapeshiftHandler(Player player, Entity eventEntity);
	public static event PlayerShapeshiftHandler OnPlayerShapeshift;

	public delegate void PlayerChatCommandHandler(Player player, CommandAttribute command);
	public static event PlayerChatCommandHandler OnPlayerChatCommand;

	public delegate void PlayerUsedConsumableHandler(Player player, Entity eventEntity, InventoryBuffer item);
	public static event PlayerUsedConsumableHandler OnPlayerUsedConsumable;

	public delegate void PlayerBuffedHandler(Player player, Entity buffEntity);
	public static event PlayerBuffedHandler OnPlayerBuffed;

	public delegate void PlayerBuffRemovedHandler(Player player, Entity buffEntity);
	public static event PlayerBuffRemovedHandler OnPlayerBuffRemoved;

	public delegate void UnitBuffedHandler(Entity unit, Entity buffEntity);
	public static event UnitBuffedHandler OnUnitBuffed;

	public delegate void UnitBuffRemovedHandler(Entity unit, Entity buffEntity);
	public static event UnitBuffRemovedHandler OnUnitBuffRemoved;

	public delegate void PlayerWillLoseGallopBuffHandler(Player player, Entity eventEntity);
	public static event PlayerWillLoseGallopBuffHandler OnPlayerWillLoseGallopBuff;

	public delegate void PlayerMountedHandler(Player player, Entity eventEntity);
	public static event PlayerMountedHandler OnPlayerMounted;

	public delegate void PlayerDismountedHandler(Player player, Entity eventEntity);
	public static event PlayerDismountedHandler OnPlayerDismounted;

	public delegate void PlayerStartedCastingHandler(Player player, Entity eventEntity);
	public static event PlayerStartedCastingHandler OnPlayerStartedCasting;

	public delegate void PlayerConnectedHandler(Player player);
	public static event PlayerConnectedHandler OnPlayerConnected;

	public delegate void PlayerDisconnectedHandler(Player player);
	public static event PlayerDisconnectedHandler OnPlayerDisconnected;

	public delegate void PlayerInvitedToClanHandler(Player player, Entity eventEntity);
	public static event PlayerInvitedToClanHandler OnPlayerInvitedToClan;

	public delegate void PlayerKickedFromClanHandler(Player player, Entity eventEntity);
	public static event PlayerKickedFromClanHandler OnPlayerKickedFromClan;

	public delegate void PlayerLeftClanHandler(Player player, Entity eventEntity);
	public static event PlayerLeftClanHandler OnPlayerLeftClan;

	public delegate void UnitDeathHandler(Entity unit, DeathEvent deathEvent);
	public static event UnitDeathHandler OnUnitDeath;

	public delegate void ItemWasDroppedHandler(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex);
	public static event ItemWasDroppedHandler OnItemWasDropped;

	public delegate void PlayerDamageDealtHandler(Player player, Entity eventEntity);
	public static event PlayerDamageDealtHandler OnPlayerDamageDealt;

	public delegate void UnitDamageDealtHandler(Entity unit, Entity eventEntity);
	public static event UnitDamageDealtHandler OnUnitDamageDealt;

	public delegate void PlayerDamageReported(Player source, Entity target, PrefabGUID ability, DamageInfo damageInfo);
	public static event PlayerDamageReported OnPlayerDamageReported;

	public delegate void PlayerDamageReceivedHandler(Player player, Entity eventEntity);
	public static event PlayerDamageReceivedHandler OnPlayerDamageReceived;

	public delegate void PlayerProjectileCreatedHandler(Player player, Entity projectile);
	public static event PlayerProjectileCreatedHandler OnPlayerProjectileCreated;

	public delegate void UnitProjectileCreatedHandler(Entity unit, Entity projectile);
	public static event UnitProjectileCreatedHandler OnUnitProjectileCreated;

	public delegate void PlayerProjectileUpdateHandler(Player player, Entity projectile);
	public static event PlayerProjectileUpdateHandler OnPlayerProjectileUpdate;

	public delegate void UnitProjectileUpdateHandler(Entity unit, Entity projectile);
	public static event UnitProjectileUpdateHandler OnUnitProjectileUpdate;

	public delegate void PlayerAoeCreatedHandler(Player player, Entity aoe);
	public static event PlayerAoeCreatedHandler OnPlayerAoeCreated;

	public delegate void UnitAoeCreatedHandler(Entity unit, Entity aoe);
	public static event UnitAoeCreatedHandler OnUnitAoeCreated;

	public delegate void PlayerChatMessageHandler(Player player, Entity eventEntity);
	public static event PlayerChatMessageHandler OnPlayerChatMessage;

	public delegate void PlayerResetHandler(Player player);
	public static event PlayerResetHandler OnPlayerReset;

	public delegate void DelayedSpawnEventHandler(Unit unit, int timeUntilSpawn);
	public static event DelayedSpawnEventHandler OnDelayedSpawn;

	public delegate void PlayerFirstTimeSpawnHandler(Player player);
	public static event PlayerFirstTimeSpawnHandler OnPlayerFirstTimeSpawn;

	public delegate void PlayerSpawningHandler(Player player, SpawnCharacter spawnCharacter);
	public static event PlayerSpawningHandler OnPlayerSpawning;

    public delegate void PlayerHasNoControlledEntityHandler(Player player);
    public static event PlayerHasNoControlledEntityHandler OnPlayerHasNoControlledEntity;

	public delegate void PlayerHitColliderCastCreated(Player player, Entity hitCastCollider);
	public static event PlayerHitColliderCastCreated OnPlayerHitColliderCastCreated;

	public delegate void UnitHitCastColliderCreated(Entity unit, Entity hitCastCollider);
	public static event UnitHitCastColliderCreated OnUnitHitCastColliderCreated;

	public delegate void PlayerPlacedStructure(Player player, Entity eventEntity);
	public static event PlayerPlacedStructure OnPlayerPlacedStructure;

	public delegate void PlayerPurchasedItem(Player player, Entity eventEntity);
	public static event PlayerPurchasedItem OnPlayerPurchasedItem;

	public delegate void GameFrameUpdateHandler();
	public static event GameFrameUpdateHandler OnGameFrameUpdate;

	public delegate void AggroPostUpdateHandler(Entity entity);
	public static event AggroPostUpdateHandler OnAggroPostUpdate;

	public delegate void PlayerInteractedHandler(Player player, Interactor interactor);
	public static event PlayerInteractedHandler OnPlayerInteracted;

	public static void RaisePlayerRespawn(Player player)
	{
		OnPlayerRespawn?.Invoke(player);
	}

	public static void RaisePlayerDeath(Player player, DeathEvent deathEvent)
	{
		OnPlayerDeath?.Invoke(player, deathEvent);
	}

	public static void RaisePlayerDowned(Player player, Entity killer)
	{
		OnPlayerDowned?.Invoke(player, killer);
	}

	public static void RaisePlayerShapeshifted(Player player, Entity eventEntity)
	{
		OnPlayerShapeshift?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerBuffed(Player player, Entity buffEntity)
	{
		OnPlayerBuffed?.Invoke(player, buffEntity);
	}

	public static void RaisePlayerBuffRemoved(Player player, Entity buffEntity)
	{
		OnPlayerBuffRemoved?.Invoke(player, buffEntity);
	}

	public static void RaiseUnitBuffed(Entity unit, Entity buffEntity)
	{
		OnUnitBuffed?.Invoke(unit, buffEntity);
	}

	public static void RaiseUnitBuffRemoved(Entity unit, Entity buffEntity)
	{
		OnUnitBuffRemoved?.Invoke(unit, buffEntity);
	}

	public static void RaisePlayerUsedConsumable(Player player, Entity eventEntity, InventoryBuffer item)
	{
		OnPlayerUsedConsumable?.Invoke(player, eventEntity, item);
	}

	public static void RaisePlayerStartedCasting(Player player, Entity eventEntity)
	{
		OnPlayerStartedCasting?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerWillLoseGallopBuff(Player player, Entity eventEntity)
	{
		OnPlayerWillLoseGallopBuff?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerMounted(Player player, Entity eventEntity)
	{
		OnPlayerMounted?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerDismounted(Player player, Entity eventEntity)
	{
		OnPlayerDismounted?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerConnected(Player player)
	{
		OnPlayerConnected?.Invoke(player);
	}

	public static void RaisePlayerDisconnected(Player player)
	{
		OnPlayerDisconnected?.Invoke(player);
	}

	public static void RaisePlayerInvitedToClan(Player player, Entity eventEntity)
	{
		OnPlayerInvitedToClan?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerKickedFromClan(Player player, Entity eventEntity)
	{
		OnPlayerKickedFromClan?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerLeftClan(Player player, Entity eventEntity)
	{
		OnPlayerLeftClan?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerChatCommand(Player player, CommandAttribute command)
	{
		OnPlayerChatCommand?.Invoke(player, command);
	}

	public static void RaiseUnitDeath(Entity unit, DeathEvent deathEvent)
	{
		OnUnitDeath?.Invoke(unit, deathEvent);
	}

	public static void RaisePlayerDealtDamage(Player player, Entity eventEntity)
	{
		OnPlayerDamageDealt?.Invoke(player, eventEntity);
	}

	public static void RaiseUnitDealtDamage(Entity unit, Entity eventEntity)
	{
		OnUnitDamageDealt?.Invoke(unit, eventEntity);
	}


	public static void RaisePlayerReceivedDamage(Player player, Entity eventEntity)
	{
		OnPlayerDamageReceived?.Invoke(player, eventEntity);
	}

	public static void RaiseItemWasDropped(Player closestPlayer, Entity eventEntity, PrefabGUID itemType, int slotIndex)
	{
		OnItemWasDropped?.Invoke(closestPlayer, eventEntity, itemType, slotIndex);
	}

	public static void RaisePlayerProjectileCreated(Player player, Entity projectile)
	{
		OnPlayerProjectileCreated?.Invoke(player, projectile);
	}

	public static void RaiseUnitProjectileCreated(Entity unit, Entity projectile)
	{
		OnUnitProjectileCreated?.Invoke(unit, projectile);
	}

	public static void RaisePlayerProjectileUpdate(Player player, Entity projectile)
	{
		OnPlayerProjectileUpdate?.Invoke(player, projectile);
	}

	public static void RaiseUnitProjectileUpdate(Entity unit, Entity projectile)
	{
		OnUnitProjectileUpdate?.Invoke(unit, projectile);
	}

	public static void RaisePlayerAoeCreated(Player player, Entity aoe)
	{
		OnPlayerAoeCreated?.Invoke(player, aoe);
	}

	public static void RaiseUnitAoeCreated(Entity unit, Entity aoe)
	{
		OnUnitAoeCreated?.Invoke(unit, aoe);
	}

	public static void RaiseDelayedSpawnEvent(Unit unit, int timeUntilSpawn)
	{
		OnDelayedSpawn?.Invoke(unit, timeUntilSpawn);
	}

	public static void RaisePlayerDamageReported(Player source, Entity target, PrefabGUID ability, DamageInfo damageInfo)
	{
		OnPlayerDamageReported?.Invoke(source, target, ability, damageInfo);
	}

	public static void RaisePlayerChatMessage(Player player, Entity eventEntity)
	{
		OnPlayerChatMessage?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerReset(Player player)
	{
		OnPlayerReset?.Invoke(player);
	}

	public static void RaisePlayerFirstTimeSpawn(Player player)
	{
		OnPlayerFirstTimeSpawn?.Invoke(player);
	}

	public static void RaisePlayerSpawning(Player player, SpawnCharacter spawnCharacter)
	{
		OnPlayerSpawning?.Invoke(player, spawnCharacter);
	}

    public static void RaisePlayerHasNoControlledEntity(Player player)
    {
        OnPlayerHasNoControlledEntity?.Invoke(player);
    }

    public static void RaiseGameFrameUpdate()
	{
		OnGameFrameUpdate?.Invoke();
	}

	public static void RaiseAggroPostUpdate(Entity entity)
	{
		OnAggroPostUpdate?.Invoke(entity);
	}

	public static void RaisePlayerHitColliderCastCreated(Player player, Entity hitCastCollider)
	{
		OnPlayerHitColliderCastCreated?.Invoke(player, hitCastCollider);
	}

	public static void RaiseUnitHitColliderCastCreated(Entity unit, Entity hitCastCollider)
	{
		OnUnitHitCastColliderCreated?.Invoke(unit, hitCastCollider);
	}

	public static void RaisePlayerPlacedStructure(Player player, Entity eventEntity)
	{
		OnPlayerPlacedStructure?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerPurchasedItem(Player player, Entity eventEntity)
	{
		OnPlayerPurchasedItem?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerInteracted(Player player, Interactor interactor)
	{
		OnPlayerInteracted?.Invoke(player, interactor);
	}
}
