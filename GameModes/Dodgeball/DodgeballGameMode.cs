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
using static ProjectM.Debugging.DealDamageEventCommand;
using UnityEngine.UI;
using PvpArena.GameModes.Dodgeball;
using System;
using static PvpArena.GameModes.GameEvents;
using Unity.Scenes;

namespace PvpArena.GameModes.Dodgeball;

public class DodgeballGameMode : BaseGameMode
{
    
	public bool HasStarted = false;
	public static Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>();
	public static List<Timer> Timers = new List<Timer>();
	public Stopwatch stopwatch = new Stopwatch();
	public static Dictionary<Player, bool> IsGhost = new Dictionary<Player, bool>();
	public static Dictionary<int, Queue<Player>> TeamGhosts = new Dictionary<int, Queue<Player>>();
	public static Dictionary<int, int> TeamCountersHit = new Dictionary<int, int>();
	public override Player.PlayerState GameModeType => Player.PlayerState.Dodgeball;
	
    public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
    {
        RemoveConsumables = true,
        RemoveShapeshifts = true,
		BuffsToIgnore = new HashSet<PrefabGUID> { Prefabs.AB_Shapeshift_Mist_Buff, Prefabs.Buff_General_HideCorpse, Helper.CustomBuff4}
    };

	public static Helper.ResetOptions ResetOptionsRemoveGhost { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = true,
		RemoveShapeshifts = true,
		BuffsToIgnore = new HashSet<PrefabGUID> { Helper.CustomBuff4 }
	};

	public DodgeballGameMode()
	{
		
	}

	public override void Initialize()
	{
		/*GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;*/
		BaseInitialize();
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerDamageReceived += HandleOnPlayerDamageReceived;
        GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
        GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
        GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
        /*GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;*/
    }

	public void Initialize(List<Player> team1Players, List<Player> team2Players)
	{

        Action action = () =>
        {
            HasStarted = true;
        };
        var timer = ActionScheduler.RunActionOnceAfterDelay(action, 1);
        Timers.Add(timer);
		Teams[1] = team1Players;
		Teams[2] = team2Players;
		TeamGhosts[1] = new Queue<Player>();
		TeamGhosts[2] = new Queue<Player>();
		TeamCountersHit[1] = 0;
		TeamCountersHit[2] = 0;
		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				IsGhost[player] = false;
			}
		}
		
		Initialize();
	}
	public override void Dispose()
	{
		/*GameEvents.OnPlayerRespawn -= HandleOnPlayerRespawn;*/
		BaseDispose();
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerDamageReceived -= HandleOnPlayerDamageReceived;
        GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
        GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;
        GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
        /*GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;*/
        HasStarted = false;
		stopwatch.Reset();
		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
		Teams.Clear();
		TeamGhosts.Clear();
		TeamCountersHit.Clear();
		IsGhost.Clear();
	}

	private static HashSet<string> AllowedCommands = new HashSet<string>
	{

	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != this.GameModeType) return;

		EliminatePlayer(player);
	}
	public override void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
	{
		if (player.CurrentState != this.GameModeType) return;

		var pos = player.Position;
		Helper.RespawnPlayer(player, pos);
        var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		EliminatePlayer(player);
	}
	/*public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}*/
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (player.CurrentState != this.GameModeType) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		eventEntity.Destroy();
		player.ReceiveMessage("Cannot shapeshift during dodgeball".Error());
	}
	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != this.GameModeType) return;

		eventEntity.Destroy();
		player.ReceiveMessage("Cannot use consumeables during dodgeball".Error());
	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

	}

	public void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		var prefabGuid = buffEntity.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.AB_Blood_BloodRite_Immaterial)
		{
            TeamCountersHit[player.MatchmakingTeam]++;
			if (TeamCountersHit[player.MatchmakingTeam] % DodgeballConfig.Config.BlocksToRevive == 0)
			{
				TeamCountersHit[player.MatchmakingTeam] = 0;
				if (TeamGhosts[player.MatchmakingTeam].Count > 0)
				{
					ReviveDeadTeammate(player);
				}
				else
				{
					Helper.HealEntity(player.Character);
					Helper.MakeSCT(player, Prefabs.SCT_Type_MAX);
				}
			}
		}
		else if (prefabGuid == Prefabs.AB_Blood_BloodRite_SpellMod_Stealth)
		{
			Helper.DestroyBuff(buffEntity);
		}
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;

		if (player.MatchmakingTeam == 1)
		{
			player.Teleport(DodgeballConfig.Config.Team1StartPosition.ToFloat3());
		}
		else
		{
			player.Teleport(DodgeballConfig.Config.Team2StartPosition.ToFloat3());
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;

		base.HandleOnPlayerDisconnected(player);
		Helper.DestroyEntity(player.Character);
	}

	public void HandleOnPlayerDamageReceived(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var spell = damageDealtEvent.SpellSource.Read<PrefabGUID>();
		float damagePercent;
		if (spell == Prefabs.EH_Monster_EnergyBeam_Active && !IsGhost[player])
		{
			damagePercent = 4f;
			var damageDealtEventNew = new DealDamageEvent(damageDealtEvent.Target, damageDealtEvent.MainType, damageDealtEvent.MainFactor, damageDealtEvent.ResourceModifier, damageDealtEvent.MaterialModifiers, damageDealtEvent.SpellSource, 0, damagePercent, damageDealtEvent.Modifier, damageDealtEvent.DealDamageFlags);
			eventEntity.Write(damageDealtEventNew);
		}
		else
		{
			eventEntity.Destroy();
		}
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var spell = damageDealtEvent.SpellSource.Read<PrefabGUID>();
		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		float damagePercent;
		if (spell == Prefabs.AB_Blood_Shadowbolt_Projectile && !isStructure)
		{
			damagePercent = 1f / DodgeballConfig.Config.HitsToKill;
			var damageDealtEventNew = new DealDamageEvent(damageDealtEvent.Target, damageDealtEvent.MainType, damageDealtEvent.MainFactor, damageDealtEvent.ResourceModifier, damageDealtEvent.MaterialModifiers, damageDealtEvent.SpellSource, 0, damagePercent, damageDealtEvent.Modifier, damageDealtEvent.DealDamageFlags);
			eventEntity.Write(damageDealtEventNew);
		}
		else
		{
			eventEntity.Destroy();
		}
	}

/*	private bool IsOutOfBounds(Player player)
	{
		var enemyTeam = 1;
		if (player.MatchmakingTeam == 1)
		{
			enemyTeam = 2;
		}
		return FightZones[enemyTeam].Contains(player) && !IsGhost[player];
	}*/

/*	public void HandleOnGameFrameUpdate()
	{
		if (HasStarted)
		{
			foreach (var team in Teams.Values)
			{
				foreach (var player in team)
				{
					if (IsOutOfBounds(player))
					{
						player.ReceiveMessage("You have gone out of bounds!".Error());
						EliminatePlayer(player);
					}
				}
			}
		}
	}*/

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}

	private static void ReviveDeadTeammate(Player player)
	{
		var ghost = TeamGhosts[player.MatchmakingTeam].Dequeue();
		RevivePlayer(ghost);
	}

	private static void EliminatePlayer(Player player)
	{
		/*if (player.MatchmakingTeam == 1)
		{
			player.Teleport(DodgeballConfig.Config.Team1StartPosition.ToFloat3());
		}
		else
		{
			player.Teleport(DodgeballConfig.Config.Team2StartPosition.ToFloat3());
		}*/

		bool playerAlreadyGhost = IsGhost[player];
		IsGhost[player] = true;
		player.Reset(ResetOptions);
		var action = () => Helper.MakeGhostlySpectator(player);
		var timer = ActionScheduler.RunActionOnceAfterDelay(action, .05);
		Timers.Add(timer);

		if (!playerAlreadyGhost)
		{
			TeamGhosts[player.MatchmakingTeam].Enqueue(player);
		}
		
		foreach (var team in Teams.Values)
		{
			bool allGhosts = true;
			foreach (var teamPlayer in team)
			{
				if (!IsGhost[teamPlayer])
				{
					allGhosts = false;
				}
				bool isFriendly = teamPlayer.MatchmakingTeam == player.MatchmakingTeam;
				var nameColorized = isFriendly ? player.Name.FriendlyTeam() : player.Name.EnemyTeam();
				var resultColorized = isFriendly ? "eliminated".EnemyTeam() : "eliminated".FriendlyTeam();
				if (!playerAlreadyGhost)
				{
					teamPlayer.ReceiveMessage($"{nameColorized} has been {resultColorized}!".White());
				}
			}
			if (allGhosts)
			{
				var winningTeam = 1;
				if (player.MatchmakingTeam == 1) 
				{
					winningTeam = 2;
				}
				ReportStats(winningTeam);
				DodgeballHelper.EndMatch(winningTeam);
				return;
			}
		}
		
	}

	//make rendy improve this :P
	private static void ReportStats(int winningTeam)
	{
		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				string message;
				if (player.MatchmakingTeam == winningTeam)
				{
					message = "You won!".Success();
				}
				else
				{
					message = "You lost.".Error();
				}
				player.ReceiveMessage(message);
			}
		}
	}

	public static void RevivePlayer(Player player)
	{
		if (player.MatchmakingTeam == 1)
		{
			player.Teleport(DodgeballConfig.Config.Team1StartPosition.ToFloat3());
		}
		else
		{
			player.Teleport(DodgeballConfig.Config.Team2StartPosition.ToFloat3());
		}
		player.Reset(ResetOptionsRemoveGhost);
        DodgeballHelper.SetPlayerAbilities(player);
        var action = () =>
        {
            IsGhost[player] = false;
        };
        ActionScheduler.RunActionOnceAfterDelay(action, .05f);
		Helper.BuffPlayer(player, Prefabs.Buff_General_Phasing, out var buffEntity, 3);

        foreach (var team in Teams.Values)
		{
			foreach (var teamPlayer in team)
			{
				bool isFriendly = teamPlayer.MatchmakingTeam == player.MatchmakingTeam;
				var nameColorized = isFriendly ? player.Name.FriendlyTeam() : player.Name.EnemyTeam();
				var resultColorized = isFriendly ? "revived".FriendlyTeam() : "revived".EnemyTeam();
				teamPlayer.ReceiveMessage($"{nameColorized} has been {resultColorized}!".White());
			}
		}
	}

    public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
    {
		if (player.CurrentState != this.GameModeType) return;

		eventEntity.Destroy();
        player.ReceiveMessage("You may not invite players to your clan while in Dodgeball".Error());
    }

    public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity)
    {
		if (player.CurrentState != this.GameModeType) return;

		eventEntity.Destroy();
        player.ReceiveMessage("You may not kick players from your clan while in Dodgeball".Error());
    }

    public void HandleOnPlayerLeftClan(Player player, Entity eventEntity)
    {
		if (player.CurrentState != this.GameModeType) return;

		eventEntity.Destroy();
        player.ReceiveMessage("You may not leave your clan while in Dodgeball".Error());
    }
}

