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
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes;
public static class GameEvents
{
	public delegate void PlayerRespawnHandler(Player player);
	public static event PlayerRespawnHandler OnPlayerRespawn;

	public delegate void PlayerDeathHandler(Player player, OnKillCallResult killCallResult);
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

	public delegate void PlayerUnbuffedHandler(Player player, Entity buffEntity);
	public static event PlayerUnbuffedHandler OnPlayerUnbuffed;

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

	public delegate void UnitDeathHandler(Entity unit, OnKillCallResult killCallResult);
	public static event UnitDeathHandler OnUnitDeath;

	public delegate void ItemWasThrownHandler(Player closestPlayer, Entity eventEntity);
	public static event ItemWasThrownHandler OnItemWasThrown;

	public delegate void PlayerDamageDealtHandler(Player player, Entity eventEntity);
	public static event PlayerDamageDealtHandler OnPlayerDamageDealt;

	public delegate void PlayerDamageReported(Player source, Player target, PrefabGUID type, float damageDealt);
	public static event PlayerDamageReported OnPlayerDamageReported;

	public delegate void PlayerDamageReceivedHandler(Player player, Entity eventEntity);
	public static event PlayerDamageReceivedHandler OnPlayerDamageReceived;

	public delegate void PlayerProjectileCreatedHandler(Player player, Entity projectile);
	public static event PlayerProjectileCreatedHandler OnProjectileCreated;

	public delegate void DelayedSpawnEventHandler(Unit unit, int timeUntilSpawn);
	public static event DelayedSpawnEventHandler OnDelayedSpawn;

	public delegate void GameFrameUpdateHandler();
	public static event GameFrameUpdateHandler OnGameFrameUpdate;

	public static void RaisePlayerRespawn(Player player)
	{
		OnPlayerRespawn?.Invoke(player);
	}

	public static void RaisePlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		OnPlayerDeath?.Invoke(player, killCallResult);
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

	public static void RaiseUnitDeath(Entity unit, OnKillCallResult killCallResult)
	{
		OnUnitDeath?.Invoke(unit, killCallResult);
	}

	public static void RaisePlayerDealtDamage(Player player, Entity eventEntity)
	{
		OnPlayerDamageDealt?.Invoke(player, eventEntity);
	}

	public static void RaisePlayerReceivedDamage(Player player, Entity eventEntity)
	{
		OnPlayerDamageReceived?.Invoke(player, eventEntity);
	}

	public static void RaiseItemWasThrown(Player closestPlayer, Entity eventEntity)
	{
		OnItemWasThrown?.Invoke(closestPlayer, eventEntity);
	}

	public static void RaiseProjectileCreated(Player player, Entity projectile)
	{
		OnProjectileCreated?.Invoke(player, projectile);
	}

	public static void RaiseDelayedSpawnEvent(Unit unit, int timeUntilSpawn)
	{
		OnDelayedSpawn?.Invoke(unit, timeUntilSpawn);
	}

	public static void RaisePlayerDamageReported(Player source, Player target, PrefabGUID type, float damage)
	{
		OnPlayerDamageReported?.Invoke(source, target, type, damage);
	}

	public static void RaiseGameFrameUpdate()
	{
		OnGameFrameUpdate?.Invoke();
	}
}
