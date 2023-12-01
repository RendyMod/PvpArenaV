using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bloodstone.API;
using Discord;
using Il2CppSystem.Linq.Expressions.Interpreter;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Shared;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.HighDefinition;
using static PvpArena.Factories.UnitFactory;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.Domination;

public static class DominationHelper
{
	public static List<Timer> timers = new List<Timer>();

	public static void DisposeTimers()
	{
		foreach (var timer in timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		timers.Clear();
	}

	public static void GiveHealingPotionsIfNotPresent(Player player)
	{
		if (!Helper.PlayerHasItemInInventories(player, Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02))
		{
			Helper.AddItemToInventory(player.Character, Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02, 1, out Entity entity);
		}

		Helper.RemoveItemFromInventory(player, Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01);
		Helper.RemoveItemFromInventory(player, Prefabs.Item_Consumable_Salve_Vermin);
	}

	private static void SpawnUnits(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		foreach (var unitSettings in DominationConfig.Config.UnitSpawns)
		{
			Unit unitToSpawn;
			var unitType = unitSettings.Type.ToLower();
			if (unitType == "turret")
			{
				unitToSpawn = new Turret(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "boss")
			{
				unitToSpawn = new Boss(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "angram") 
			{
				unitToSpawn = new AngramBoss(unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "horse")
			{
				unitToSpawn = new ObjectiveHorse(unitSettings.Team);
			}
			else if (unitType == "lightningrod")
			{
				unitToSpawn = new LightningBoss(unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "healingorb")
			{
				unitToSpawn = new HealingOrb();

			}
			else
			{
				unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			unitToSpawn.MaxHealth = unitSettings.Health;
			unitToSpawn.Category = "domination";
			unitToSpawn.RespawnTime = unitSettings.RespawnTime;
			unitToSpawn.SpawnDelay = unitSettings.SpawnDelay;
			Player teamLeader;
			if (unitToSpawn.Team == 1)
			{
				teamLeader = team1LeaderPlayer;
			}
			else if (unitToSpawn.Team == 2)
			{
				teamLeader = team2LeaderPlayer;
			}
			else
			{
				teamLeader = null;
			}
			
			UnitFactory.SpawnUnit(unitToSpawn, unitSettings.Location.ToFloat3(), teamLeader);
		}
	}

	public static void KillPreviousEntities()
	{
		var entities = Helper.GetEntitiesByComponentTypes<CanFly>(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (UnitFactory.TryGetSpawnedUnitFromEntity(entity, out SpawnedUnit spawnedUnit))
				{
					if (spawnedUnit.Unit.Category == "domination")
					{
						Helper.DestroyEntity(entity);
					}
				}
			}
		}
	}

	public static void StartMatch(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		var team1Players = team1LeaderPlayer.GetClanMembers();
		var team2Players = team2LeaderPlayer.GetClanMembers();
		SpawnUnits(team1LeaderPlayer, team2LeaderPlayer);
		Core.dominationGameMode.Initialize(team1Players, team2Players);

		foreach (var team1Player in team1Players)
		{
			team1Player.CurrentState = Player.PlayerState.Domination;
			team1Player.MatchmakingTeam = 1;
			team1Player.Reset(BaseGameMode.ResetOptions);
			Helper.SetDefaultBlood(team1Player, DominationConfig.Config.DefaultBlood.ToLower());
			GiveHealingPotionsIfNotPresent(team1Player);
			team1Player.Teleport(DominationConfig.Config.Team1PlayerRespawn.ToFloat3());
			Helper.ApplyMatchInitializationBuff(team1Player);
			Helper.BuffPlayer(team1Player, Prefabs.AB_Consumable_SpellBrew_T02_Buff, out var buffEntity, Helper.NO_DURATION);
			Helper.BuffPlayer(team1Player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff, out buffEntity, Helper.NO_DURATION);
		}

		foreach (var team2Player in team2Players)
		{
			team2Player.CurrentState = Player.PlayerState.Domination;
			team2Player.MatchmakingTeam = 2;
			team2Player.Reset(BaseGameMode.ResetOptions);
			Helper.SetDefaultBlood(team2Player, DominationConfig.Config.DefaultBlood.ToLower());
			GiveHealingPotionsIfNotPresent(team2Player);
			team2Player.Teleport(DominationConfig.Config.Team2PlayerRespawn.ToFloat3());
			Helper.ApplyMatchInitializationBuff(team2Player);
			Helper.BuffPlayer(team2Player, Prefabs.AB_Consumable_SpellBrew_T02_Buff, out var buffEntity, Helper.NO_DURATION);
			Helper.BuffPlayer(team2Player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff, out buffEntity, Helper.NO_DURATION);
		}
	}

	public static void EndMatch(int winner = 0)
	{
		try
		{
			foreach (var timer in DominationGameMode.QueuedRespawns)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			DominationGameMode.QueuedRespawns.Clear();

			foreach (var player in PlayerService.UserCache.Values)
			{
				if (player.IsInDomination())
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					player.Reset(BaseGameMode.ResetOptions);
					if (!player.IsAlive)
					{
						Helper.RespawnPlayer(player, player.Position);
					}
				}
			}
			if (winner > 0)
			{
				TeleportTeamsToCenter(DominationGameMode.Teams, winner, TeamSide.North);
			}

			Core.dominationGameMode.Dispose();
			KillPreviousEntities();
			UnitFactory.DisposeTimers("domination");
			DisposeTimers();
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogError(e.ToString());
		}
	}

	public enum TeamSide
	{
		North,
		East,
		South,
		West
	}

	public static void TeleportTeamsToCenter(
	Dictionary<int, List<Player>> Teams,
	int winningTeam,
	TeamSide teamOneSide)
	{
		var mapCenter = DominationConfig.Config.MapCenter;
		float playerSpacing = 2f;  // Adjust this as needed for the distance between players.

		// Determine the center coordinates
		var centerX = (mapCenter.Left + mapCenter.Right) / 2;
		var centerZ = (mapCenter.Top + mapCenter.Bottom) / 2;

		// Calculate the offset to center the team based on the number of players and spacing
		float teamOneOffset = (Teams[1].Count - 1) * playerSpacing / 2;
		float teamTwoOffset = (Teams[2].Count - 1) * playerSpacing / 2;

		// Determine the starting positions based on the side they are starting from
		float teamOneStartX = centerX;
		float teamOneStartZ = centerZ;
		float teamTwoStartX = centerX;
		float teamTwoStartZ = centerZ;

		if (teamOneSide == TeamSide.North || teamOneSide == TeamSide.South)
		{
			teamOneStartZ = teamOneSide == TeamSide.South ? centerZ - playerSpacing : centerZ + playerSpacing;
			teamTwoStartZ = teamOneSide == TeamSide.South ? centerZ + playerSpacing : centerZ - playerSpacing;
		}
		else
		{
			teamOneStartX = teamOneSide == TeamSide.East ? centerX + playerSpacing : centerX - playerSpacing;
			teamTwoStartX = teamOneSide == TeamSide.East ? centerX - playerSpacing : centerX + playerSpacing;
		}
		ApplyBuffsToTeams(Teams, winningTeam);

		// Position the teams
		PositionTeam(Teams[1], teamOneStartX, teamOneStartZ, playerSpacing, teamOneSide);
		PositionTeam(Teams[2], teamTwoStartX, teamTwoStartZ, playerSpacing, teamOneSide);
	}

	private static void PositionTeam(List<Player> team, float startCoordX, float startCoordZ, float spacing, TeamSide side)
	{
		bool isHorizontal = side == TeamSide.North || side == TeamSide.South;
		for (int i = 0; i < team.Count; i++)
		{
			float x = isHorizontal ? startCoordX + i * spacing - (team.Count - 1) * spacing / 2 : startCoordX;
			float z = isHorizontal ? startCoordZ : startCoordZ + i * spacing - (team.Count - 1) * spacing / 2;
			team[i].Teleport(new float3(x, team[i].Position.y, z));
		}
	}

	private static void ApplyBuffsToTeams(Dictionary<int, List<Player>> Teams, int winningTeam)
	{
		List<Player> winners = Teams[winningTeam];
		List<Player> losers = Teams[winningTeam == 1 ? 2 : 1];

		foreach (var winner in winners)
		{
			Helper.ApplyWinnerMatchEndBuff(winner);
		}
		foreach (var loser in losers)
		{
			Helper.ApplyLoserMatchEndBuff(loser);
		}
	}


	public static void MessageEveryone(Dictionary<int, List<Player>> teams, string message)
	{
		foreach (var team in teams.Values)
		{
			foreach (var player in team)
			{
				player.ReceiveMessage(message);
			}
		}
	}
}
