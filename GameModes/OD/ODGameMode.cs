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
using PvpArena.GameModes.Matchmaking1v1;
using Unity.DebugDisplay;

namespace PvpArena.GameModes.OD;

public class ODGameMode : BaseGameMode
{
	public override Player.PlayerState PlayerGameModeType => Player.PlayerState.OD;
	public override string UnitGameModeType => "OD";
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveShapeshifts = false,
		RemoveConsumables = false
	};

	public Dictionary<int, List<Player>> Teams = new();
	Dictionary<Player, bool> PlayerIsAlive = new();

	public static List<Timer> Timers = new List<Timer>();
	public int MatchNumber = 0;
	private static int lastId = 0;

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
		"tp-list",
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
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
	}

	public ODGameMode()
	{
		MatchNumber = ++lastId; // Increment the lastId and assign it to the new instance.
	}

	public void Initialize(List<Player> team1, List<Player> team2)
	{
		Teams[1] = team1;
		Teams[2] = team2;
		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				PlayerIsAlive[player] = true;
			}
		}
		Initialize();
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
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;

		Teams.Clear();
		PlayerIsAlive.Clear();
	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You are OD'd...".Error());
	}

	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		PlayerIsAlive[player] = false;
		bool allDead = true;
		foreach (var teamPlayer in Teams[player.MatchmakingTeam])
		{
			if (PlayerIsAlive.TryGetValue(teamPlayer, out var isAlive) && isAlive)
			{
				allDead = false;
				break;
			}
		}
		player.Reset(Helper.ResetOptions.FreshMatch);
		Helper.MakeGhostlySpectator(player, Helper.NO_DURATION);

		if (killer.Has<PlayerCharacter>())
		{
			var killerPlayer = PlayerService.GetPlayerFromCharacter(killer);

			if (player.ConfigOptions.SubscribeToKillFeed)
			{
				player.ReceiveMessage($"You were killed by {killerPlayer.Name.Error()}.".White());
			}
			if (killerPlayer.ConfigOptions.SubscribeToKillFeed)
			{
				killerPlayer.ReceiveMessage($"You killed {player.Name.Success()}!".White());
			}
		}
		if (allDead)
		{
			var winningTeam = 1;
			var losingTeam = 2;
			if (player.MatchmakingTeam == 1)
			{
				winningTeam = 2;
				losingTeam = 1;
			}
			Helper.SendSystemMessageToAllClients($"Team {Teams[winningTeam][0].Name.Colorify(ExtendedColor.ClanNameColor)} won against Team {Teams[losingTeam][0].Name.Colorify(ExtendedColor.ClanNameColor)} in an OD match".White());

			var action = () => ODManager.EndMatch(MatchNumber, winningTeam);
			ActionScheduler.RunActionOnceAfterDelay(action, 1);
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		base.HandleOnPlayerDisconnected(player);

		PlayerIsAlive[player] = false;
	}
	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not invite players to your clan while OD'd".Error());
	}

	public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not kick players from your clan while OD'd".Error());
	}

	public void HandleOnPlayerLeftClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
		if (!PlayerIsAlive.ContainsKey(player)) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not leave your clan while OD'd".Error());
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

