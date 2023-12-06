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
		RemoveShapeshifts = false,
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

	public static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{
		{ "ping", true },
		{ "help", true },
		{ "legendary", true },
		{ "jewel", true },
		{ "forfeit", true },
		{ "points", true },
		{ "lb ranked", true },
		{ "bp", true },
	};

	public static List<Timer> Timers = new List<Timer>();
	public static Dictionary<Player, List<Timer>> PlayerRespawnTimers = new Dictionary<Player, List<Timer>>();

	public override void Initialize()
	{
		GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;

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
		GameEvents.OnPlayerRespawn -= HandleOnPlayerRespawn;
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
		GameEvents.OnUnitDeath -= HandleOnUnitDeath;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;

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
		Helper.MakeGhostlySpectator(player);
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
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (player.CurrentState != GameModeType) return;

		//clear out any queued up respawn actions since we will recreate them now that the player has died (in case they killed themselves twice in a row before the initial respawn actions finished)
		if (PlayerRespawnTimers.TryGetValue(player, out var respawnActions))
		{
			foreach (var respawnAction in respawnActions)
			{
				respawnAction?.Dispose();
			}
			respawnActions.Clear();
		}

		if (!BuffUtility.HasBuff(VWorld.Server.EntityManager, player.Character, Prefabs.Buff_General_Vampire_Wounded_Buff))
		{
			foreach (var alivePlayer in PlayersAlive.Keys)
			{
				string coloredVictimName = player.Name.Colorify(ExtendedColor.ClanNameColor);
				
				var message = $"{coloredVictimName} killed themselves".White();
				alivePlayer.ReceiveMessage(message);
			}
		}

		Helper.RevivePlayer(player);
		PlayersAlive[player] = false;
		CheckForWinner();
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

	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		if (enterShapeshiftEvent.Shapeshift != Prefabs.AB_Shapeshift_BloodMend_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage("You can't feel your vampire essence here...".Error());
		}
	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != GameModeType) return;
		
		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You can't drink those in prison!".Error());
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		Helper.DestroyEntity(player.Character);
	}

	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not invite players to your clan while in prison".Error());
	}

	public void HandleOnPlayerChatCommand(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;


	}

	public override void HandleOnItemWasDropped(Player player, Entity eventEntity, PrefabGUID itemType)
	{
		if (player.CurrentState != GameModeType) return;

		Helper.RemoveItemAtSlotFromInventory(player, itemType, eventEntity.Read<DropInventoryItemEvent>().SlotIndex);
		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		if (!eventEntity.Exists()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();

		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			eventEntity.Destroy();
		}
	}

	public void HandleOnPlayerRespawn(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		Helper.MakeGhostlySpectator(player);
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

	public static new Dictionary<string, bool> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}


