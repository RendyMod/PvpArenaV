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
using System.Threading;
using System.Diagnostics;
using static PvpArena.Configs.ConfigDtos;
using PvpArena.Factories;
using PvpArena.GameModes.BulletHell;
using PvpArena.Services.Moderation;
using AsmResolver;

namespace PvpArena.GameModes.Pacified;

public class PacifiedGameMode : BaseGameMode
{
	public override Player.PlayerState PlayerGameModeType => Player.PlayerState.Pacified;
	public override string UnitGameModeType => "pacified";
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveShapeshifts = false,
		RemoveConsumables = false
	};

	public static List<Timer> Timers = new List<Timer>();

	static new HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"kit",
		"kit legendary",
		"jewel",
		"forfeit",
		"points",
		"lb ranked",
		"lb bullet",
		"lb pancake",
		"bp",
		"tp-list",
		"tpa",
		"lw",
		"tp list",
		"togglekillfeed",
		"discord",
	};

	public override void Initialize()
	{
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
	}
	public override void Dispose()
	{
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You are pacified...".Error());
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

