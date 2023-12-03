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

namespace PvpArena.GameModes.Dodgeball;

public class DodgeballGameMode : BaseGameMode
{
    
	public bool HasStarted = false;
	public static Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>();
	public static List<Timer> Timers = new List<Timer>();
	public Stopwatch stopwatch = new Stopwatch();
	public Dictionary<int, RectangleZone> FightZones = new Dictionary<int, RectangleZone>();
	public static Dictionary<Player, bool> IsGhost = new Dictionary<Player, bool>();
	public static Dictionary<int, Queue<Player>> TeamGhosts = new Dictionary<int, Queue<Player>>();
	public static Dictionary<int, int> TeamCountersHit = new Dictionary<int, int>();
    public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
    {
        RemoveConsumables = true,
        RemoveShapeshifts = true
    };

    public DodgeballGameMode()
	{
		
	}

	public override void Initialize()
	{
		/*GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown += HandleOnItemWasThrown;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
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
		FightZones[1] = DodgeballConfig.Config.Team1Zone.ToRectangleZone();
		FightZones[2] = DodgeballConfig.Config.Team2Zone.ToRectangleZone();
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
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown -= HandleOnItemWasThrown;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
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
	}

	private static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{

	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (!player.IsInDodgeball()) return;


		EliminatePlayer(player);
	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (!player.IsInDodgeball()) return;

		var pos = player.Position;
		Helper.RespawnPlayer(player, pos);
        player.Reset(ResetOptions);
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
		if (!player.IsInDodgeball()) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

		eventEntity.Destroy();
		player.ReceiveMessage("Cannot shapeshift during dodgeball".Error());
	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (!player.IsInDodgeball()) return;

		eventEntity.Destroy();
		player.ReceiveMessage("Cannot use consumeables during dodgeball".Error());
	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (!player.IsInDodgeball()) return;

		var prefabGuid = buffEntity.Read<PrefabGUID>();
		prefabGuid.LogPrefabName();
		if (prefabGuid == Prefabs.AB_Blood_BloodRite_Immaterial)
		{
			TeamCountersHit[player.MatchmakingTeam]++;
			if (TeamCountersHit[player.MatchmakingTeam] % DodgeballConfig.Config.BlocksToRevive == 0)
			{
				Plugin.PluginLog.LogInfo("Reviving teammate");
				TeamCountersHit[player.MatchmakingTeam] = 0;
				ReviveDeadTeammate(player);
			}
		}
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.IsInDodgeball()) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3()); //replace this with training tp
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (!player.IsInDodgeball()) return;

        //kill them
	}

	public override void HandleOnItemWasThrown(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
		else
		{
			var spell = damageDealtEvent.SpellSource.Read<PrefabGUID>();
			spell.LogPrefabName();
			if (spell == Prefabs.AB_Blood_Shadowbolt_Projectile)
			{
				float damagePercent = 1f / DodgeballConfig.Config.HitsToKill;
				Plugin.PluginLog.LogInfo(damagePercent);
				var damageDealtEventNew = new DealDamageEvent(damageDealtEvent.Target, damageDealtEvent.MainType, damageDealtEvent.MainFactor, damageDealtEvent.ResourceModifier, damageDealtEvent.MaterialModifiers, damageDealtEvent.SpellSource, 0, damagePercent, damageDealtEvent.Modifier, damageDealtEvent.DealDamageFlags);
				eventEntity.Write(damageDealtEventNew);
			}
			else
			{
				VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			}
		}
	}
	private bool IsOutOfBounds(Player player)
	{
		var enemyTeam = 1;
		if (player.MatchmakingTeam == 1)
		{
			enemyTeam = 2;
		}
		return FightZones[enemyTeam].Contains(player) && !IsGhost[player];
	}

	public void HandleOnGameFrameUpdate()
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
	}

	public static new Dictionary<string, bool> GetAllowedCommands()
	{
		return AllowedCommands;
	}

	private static void ReviveDeadTeammate(Player player)
	{
		if (TeamGhosts[player.MatchmakingTeam].Count > 0)
		{
			var ghost = TeamGhosts[player.MatchmakingTeam].Dequeue();
			RevivePlayer(ghost);
		}
	}

	private static void EliminatePlayer(Player player)
	{
		TeamGhosts[player.MatchmakingTeam].Enqueue(player);
		player.Reset(ResetOptions);
		Helper.MakeGhostlySpectator(player);
		IsGhost[player] = true;

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
				teamPlayer.ReceiveMessage($"{nameColorized} has been {resultColorized}!".White());
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
		player.Reset(ResetOptions);
        DodgeballHelper.SetPlayerAbilities(player);
        IsGhost[player] = false;

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
}

