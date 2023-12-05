using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using ProjectM.Network;
using PvpArena.Data;
using Bloodstone.API;
using PvpArena.Configs;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;
using ProjectM.Gameplay.Systems;
using ProjectM.CastleBuilding;
using PvpArena.Services;

namespace PvpArena.GameModes;

public class SpectatingGameMode : BaseGameMode
{
	public override Player.PlayerState GameModeType => Player.PlayerState.Spectating;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = false,
		RemoveShapeshifts = true,
		ResetCooldowns = true
	};

	public override void Initialize()
	{
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown += HandleOnItemWasThrown;
	}
	public override void Dispose()
	{
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown -= HandleOnItemWasThrown;
	}

	private static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{
		{ "spectate", true },
		{ "tp", true },
		{ "ttp", true },
		{ "tpa", true },
		{ "j", true },
		{ "lw", true },
		{ "tp list", true },
		{ "help", true },
		{ "bp", true },
		{ "points", true },
		{ "togglekillfeed", true },
		{ "discord", true },
		{ "ping", true },
		{ "kit", true },
	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (player.CurrentState != GameModeType) return;

		player.Reset(SpectatingGameMode.ResetOptions);
		Helper.RespawnPlayer(player, PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
		player.CurrentState = Player.PlayerState.Normal;
		player.ReceiveMessage("You have died and are no longer spectating");
	}
	/*public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}*/
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		player.CurrentState = Player.PlayerState.Normal;
		Helper.RemoveBuff(player.Character, Prefabs.Admin_Observe_Invisible_Buff);
	}

	public override void HandleOnItemWasThrown(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var isStructure = (damageDealtEvent.Target.Has<CastleHeartConnection>());
		if (isStructure)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
	}

	public static new Dictionary<string, bool> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

