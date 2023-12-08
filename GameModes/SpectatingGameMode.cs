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
		BaseInitialize();
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
	}
	public override void Dispose()
	{
		BaseDispose();
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
	}

	private static HashSet<string> AllowedCommands = new HashSet<string>
	{
		"spectate",
		"tp",
		"ttp",
		"tpa",
		"j",
		"lw",
		"tp list",
		"help",
		"bp",
		"points",
		"togglekillfeed",
		"discord",
		"ping",
		"kit",
	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
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

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		base.HandleOnPlayerDisconnected(player);
		player.CurrentState = Player.PlayerState.Normal;
		Helper.RemoveBuff(player.Character, Prefabs.Admin_Observe_Invisible_Buff);
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

