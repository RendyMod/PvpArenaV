using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using ProjectM.Network;
using PvpArena.Data;
using Bloodstone.API;
using PvpArena.Helpers;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using System.Threading;
using PvpArena.Services;
using static PvpArena.Factories.UnitFactory;
using ProjectM.CastleBuilding;
using System.Linq;
using System.Diagnostics;

namespace PvpArena.GameModes.PrisonBreak;

public class PrisonBreakGameMode : BaseGameMode
{
/*	"Left": -400.00,
"Top": -325.00,
"Right": -345.00,
"Bottom": -355.00*/
	public override Player.PlayerState GameModeType => Player.PlayerState.PrisonBreak;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = false,
		RemoveShapeshifts = true,
		ResetCooldowns = false,
		BuffsToIgnore = new HashSet<PrefabGUID> { Prefabs.AB_Shapeshift_Mist_Buff, Prefabs.Buff_General_HideCorpse }
	};

	private static Stopwatch stopwatch = new Stopwatch();

	public static Dictionary<Player, bool> PlayersAlive = new Dictionary<Player, bool>();

	private static Dictionary<Player, int> playerKills = new Dictionary<Player, int>();

	public Dictionary<PrefabGUID, bool> allowedShapeshifts = new Dictionary<PrefabGUID, bool>
	{
		{Prefabs.AB_Shapeshift_BloodMend_Group, true }
	};

	public static HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"jewel",
		"forfeit",
		"points",
		"lb ranked",
		"bp",
	};

	public static List<Timer> Timers = new List<Timer>();
	public static Dictionary<Player, List<Timer>> PlayerRespawnTimers = new Dictionary<Player, List<Timer>>();

	public override void Initialize()
	{
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;

		foreach (var player in PlayerService.OnlinePlayers.Keys)
		{
			PlayersAlive[player] = true;
		}

		stopwatch.Start();
	}

	public void Initialize(List<Player> players)
	{
		foreach (var player in players)
		{
			PlayersAlive[player] = true;
			PlayerRespawnTimers[player] = new List<Timer>();
		}
		Initialize();
		playerKills.Clear();
	}
	public override void Dispose()
	{
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;

		PlayersAlive.Clear();
		foreach (var kvp in PlayerRespawnTimers)
		{
			foreach (var timer in kvp.Value)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
		}
		PlayerRespawnTimers.Clear();
		playerKills.Clear();

		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
	}

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

		if (killer.Exists())
		{
			Player killerPlayer = null;
			if (killer.Has<PlayerCharacter>())
			{
				killerPlayer = PlayerService.GetPlayerFromCharacter(killer);

				if (killer != player.Character)
				{
					if (playerKills.ContainsKey(killerPlayer))
					{
						playerKills[killerPlayer]++;
					}
					else
					{
						playerKills[killerPlayer] = 1;
					}
				}
			}

			foreach (var onlinePlayer in PlayersAlive.Keys)
			{
				string message = CreatePlayerDownedMessage(player, killerPlayer, onlinePlayer);
				onlinePlayer.ReceiveMessage(message);
			}
		}
		else
		{
			// Handle admin abuse case
			foreach (var onlinePlayer in PlayersAlive.Keys)
			{
				string coloredVictimName = $"{player.Name.Colorify(ExtendedColor.ClanNameColor)}";
				string message = $"{coloredVictimName} died to {"admin abuse".EnemyTeam()}".White();
				onlinePlayer.ReceiveMessage(message);
			}
		}

		player.Reset(ResetOptions);
		Timers.Add(Helper.MakeGhostlySpectator(player));
		PlayersAlive[player] = false;
		CheckForWinner();
	}

	private string CreatePlayerDownedMessage(Player victim, Player killer, Player observer)
	{
		string coloredVictimName = $"{victim.Name.Colorify(ExtendedColor.ClanNameColor)}".White();

		if (killer != null)
		{
			string coloredKillerName = $"{killer.Name.Colorify(ExtendedColor.ClanNameColor)}".White();
			return $"{coloredKillerName} killed {coloredVictimName}".White();
		}
		else
		{
			return $"{coloredVictimName} died to {"PvE".NeutralTeam()}".White();
		}
	}

	private void CheckForWinner()
	{
		if (IsMatchOver())
		{
			Player winner = null;
			foreach (var playerTemp in PlayersAlive.Keys)
			{
				if (PlayersAlive[playerTemp])
				{
					winner = playerTemp;
					ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"{winner.Name.Colorify(ExtendedColor.ClanNameColor)} has won and broken out of prison!".White());
					PrisonBreakHelper.EndMatch(winner);
					break;
				}
			}
			if (winner == null)
			{
				foreach (var playerTemp in PlayersAlive.Keys)
				{
					winner = playerTemp;
					ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"{winner.Name.Colorify(ExtendedColor.ClanNameColor)} has won and broken out of prison!".White());
					PrisonBreakHelper.EndMatch(winner);
					break;
				}
			}
		}
	}

	public bool IsMatchOver()
	{
		return PlayersAlive.Count(p => p.Value) <= 1;
	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		if (enterShapeshiftEvent.Shapeshift != Prefabs.AB_Shapeshift_BloodMend_Group)
		{
			eventEntity.Destroy();
			player.ReceiveMessage("You can't feel your vampire essence here...".Error());
		}
	}
	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != GameModeType) return;

		eventEntity.Destroy();
		player.ReceiveMessage("You can't drink those in prison!".Error());
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		base.HandleOnPlayerDisconnected(player);
		Helper.SoftKillPlayer(player);
	}

	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		eventEntity.Destroy();
		player.ReceiveMessage("You may not invite players to your clan while in prison".Error());
	}

	public void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (buffEntity.Read<PrefabGUID>() == Prefabs.AB_Shapeshift_BloodMend_Buff)
		{
			var buffer = buffEntity.ReadBuffer<ChangeBloodOnGameplayEvent>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var changeBloodOnGameplayEvent = buffer[i];
				changeBloodOnGameplayEvent.BloodValue = 10;
				buffer[i] = changeBloodOnGameplayEvent;
			}
		}
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}


