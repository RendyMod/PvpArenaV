using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bloodstone.API;
using Discord;
using Il2CppSystem.Linq.Expressions.Interpreter;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Hybrid;
using ProjectM.Network;
using ProjectM.Sequencer;
using ProjectM.Shared;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.HighDefinition;
using static PvpArena.Configs.ConfigDtos;
using static PvpArena.Factories.UnitFactory;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.Moba;

public static class MobaHelper
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

	public static void SpawnStructures(Player player1, Player player2)
	{
		foreach (var structureSpawn in MobaConfig.Config.StructureSpawns)
		{
			var spawnPos = structureSpawn.Location.ToFloat3();
			Action action = () =>
			{
				PrefabSpawnerService.SpawnWithCallback(structureSpawn.PrefabGUID, spawnPos, (e) =>
				{
					if (e.Has<Health>() && structureSpawn.Health > 0)
					{
						var health = e.Read<Health>();
						health.MaxHealth.Value = structureSpawn.Health;
						health.Value = structureSpawn.Health;
						health.MaxRecoveryHealth = structureSpawn.Health;
						e.Write(health);
					}
					if (structureSpawn.Team == 1)
					{
						e.Write(player1.Character.Read<Team>());
						e.Write(player1.Character.Read<TeamReference>());
					}
					else if (structureSpawn.Team == 2)
					{
						e.Write(player2.Character.Read<Team>());
						e.Write(player2.Character.Read<TeamReference>());
					}
				}, structureSpawn.RotationMode, -1, true, "moba");
			};
			
			Timer timer;
			if (structureSpawn.SpawnDelay > 0)
			{
				timer = ActionScheduler.RunActionOnceAfterDelay(action, structureSpawn.SpawnDelay);
				timers.Add(timer);
			}
			else
			{
				action();
			}
		}
	}

	private static void HandleGateOpeningAtMatchStart(Entity e)
	{
		e.Remove<Interactable>();
		var spawnDoor = e.Read<Door>();
		spawnDoor.OpenState = false;
		e.Write(spawnDoor);

		Action action = () =>
		{
			spawnDoor.OpenState = true;
			e.Write(spawnDoor);
		};
		var timer = ActionScheduler.RunActionOnceAfterDelay(action, 10);
		MobaGameMode.Timers.Add(timer);
	}

	private static void StartMatchCountdown()
	{
		for (int i = 5; i >= 0; i--)
		{
			int countdownNumber = i; // Introduce a new variable

			Action action = () =>
			{
				foreach (var team in MobaGameMode.Teams.Values)
				{
					foreach (var player in team)
					{
						if (countdownNumber > 0)
						{
							player.ReceiveMessage($"The match will start in: {countdownNumber.ToString().Emphasize()}".White());
						}
						else
						{
							player.ReceiveMessage($"The match has started. {"Go!".Emphasize()}".White());
						}
					}
				}

				if (countdownNumber == 0)
				{
					SpawnUnits(MobaGameMode.Teams[1][0], MobaGameMode.Teams[2][0]);
				}
			};

			Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5 - countdownNumber);
			MobaGameMode.Timers.Add(timer);
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
					if (spawnedUnit.Unit.Category == "moba")
					{
						Helper.DestroyEntity(entity);
					}
				}
				else
				{
					if (UnitFactory.HasCategory(entity, "moba"))
					{
						Helper.DestroyEntity(entity);
					}
/*					else if (entity.Has<CanFly>() && entity.Read<PrefabGUID>() == Prefabs.Resource_PlayerDeathContainer_Drop)
					{
						Helper.DestroyEntity(entity);
					}*/
				}
			}
		}
	}


	public static void GiveVerminSalvesIfNotPresent(Player player)
	{
		if (!Helper.PlayerHasItemInInventories(player, Prefabs.Item_Consumable_Salve_Vermin))
		{
			Helper.AddItemToInventory(player.Character, Prefabs.Item_Consumable_Salve_Vermin, 1, out Entity entity);
		}
		Helper.RemoveItemFromInventory(player, Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01);
		Helper.RemoveItemFromInventory(player, Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02);
	}

	public static void StartMatch(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		var team1Players = team1LeaderPlayer.GetClanMembers();
		var team2Players = team2LeaderPlayer.GetClanMembers();
		Core.mobaGameMode.Initialize(team1Players, team2Players);
		SpawnStructures(team1LeaderPlayer, team2LeaderPlayer);
		Action action;
		
		foreach (var team1Player in team1Players)
		{
			team1Player.CurrentState = Player.PlayerState.Moba;
			team1Player.MatchmakingTeam = 1;
			team1Player.Reset(BaseGameMode.ResetOptions);
			Helper.SetDefaultBlood(team1Player, MobaConfig.Config.DefaultBlood.ToLower());
			GiveVerminSalvesIfNotPresent(team1Player);
			action = () => team1Player.Teleport(MobaConfig.Config.Team1PlayerRespawn.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(action, .1);
			team1Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		foreach (var team2Player in team2Players)
		{
			team2Player.CurrentState = Player.PlayerState.Moba;
			team2Player.MatchmakingTeam = 2;
			team2Player.Reset(BaseGameMode.ResetOptions);
			Helper.SetDefaultBlood(team2Player, MobaConfig.Config.DefaultBlood.ToLower());
			GiveVerminSalvesIfNotPresent(team2Player);
			action = () => team2Player.Teleport(MobaConfig.Config.Team2PlayerRespawn.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(action, .1);
			team2Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		action = () => { StartMatchCountdown(); };

		Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5);
		MobaGameMode.Timers.Add(timer);
	}

	private static void SpawnUnits(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		foreach (var unitSettings in MobaConfig.Config.UnitSpawns)
		{
			Unit unitToSpawn;
			var unitType = unitSettings.Type.ToLower();
			if (unitType == "turret")
			{
				unitToSpawn = new BaseTurret(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "boss")
			{
				unitToSpawn = new Boss(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.DrawsAggro = true;
				unitToSpawn.IsRooted = true;
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
				unitToSpawn.IsRooted = true;
			}
			unitToSpawn.MaxHealth = unitSettings.Health;
			unitToSpawn.Category = "moba";
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
			UnitFactory.SpawnUnitWithCallback(unitToSpawn, unitSettings.Location.ToFloat3(), (e) => { MobaGameMode.TeamUnits[unitToSpawn.Team].Add(e); }, teamLeader);
		}

		var initialAction = () =>
		{
			// Spawn the first patrol immediately when this action is triggered
			SpawnPatrols(team1LeaderPlayer, team2LeaderPlayer);

			// Schedule the recurring action to start after 30 seconds from now
			var action = () => SpawnPatrols(team1LeaderPlayer, team2LeaderPlayer);
			var timer = ActionScheduler.RunActionEveryInterval(action, 30);
			MobaGameMode.Timers.Add(timer);
		};

		// Run the initial action after a 15-second delay
		var timer = ActionScheduler.RunActionOnceAfterDelay(initialAction, 15);
		MobaGameMode.Timers.Add(timer);
	}

	public static void SpawnPatrols(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		foreach (var patrol in MobaConfig.Config.PatrolSpawns)
		{
			foreach (var unitSettings in patrol.PatrolUnits)
			{
				Unit unitToSpawn;
				var unitType = unitSettings.Type.ToLower();
				unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.MaxHealth = unitSettings.Health;
				unitToSpawn.Category = "moba";
				unitToSpawn.RespawnTime = unitSettings.RespawnTime;
				unitToSpawn.SpawnDelay = 5;
				unitToSpawn.DynamicCollision = true;
				unitToSpawn.SoftSpawn = true;
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
				UnitFactory.SpawnUnitWithCallback(unitToSpawn, unitSettings.Location.ToFloat3(), (e) => {
					MobaGameMode.TeamPatrols[unitToSpawn.Team].Add(e);
					MobaGameMode.TeamUnits[unitToSpawn.Team].Add(e);
					e.Remove<DisableWhenNoPlayersInRange>();
					var blood = e.Read<BloodConsumeSource>();
					blood.BloodQuality = 0;
					e.Write(blood);
					Helper.BuffEntity(e, Helper.CustomBuff5, out var buffEntity, 10);
					Helper.ModifyBuff(buffEntity, BuffModificationTypes.DisableDynamicCollision, true);
				}, teamLeader);
			}
		}
	}

	public static void EndMatch(int winner = 0)
	{
		try
		{
			foreach (var timer in MobaGameMode.Timers)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			MobaGameMode.Timers.Clear();

			foreach (var team in MobaGameMode.Teams.Values)
			{
				foreach (var player in team)
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					player.Reset(BaseGameMode.ResetOptions);
					Helper.RespawnPlayer(player, player.Position);
				}
			}
			if (winner > 0 && MobaGameMode.Teams.Count > 0)
			{
				var action = () => {
					TeleportTeamsToCenter(MobaGameMode.Teams, winner, TeamSide.East);
					Core.mobaGameMode.Dispose();
					UnitFactory.DisposeTimers("moba");
					DisposeTimers();
					KillPreviousEntities();
				};
				ActionScheduler.RunActionOnceAfterDelay(action, .1);
			}
			else
			{
				Core.mobaGameMode.Dispose();
				UnitFactory.DisposeTimers("moba");
				DisposeTimers();
				KillPreviousEntities();
			}
		}
		catch (Exception e)
		{
			Core.mobaGameMode.Dispose();
			UnitFactory.DisposeTimers("moba");
			DisposeTimers();
			KillPreviousEntities();
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
		var mapCenter = MobaConfig.Config.MapCenter;
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
			if (team[i].IsOnline)
			{
				team[i].Teleport(new float3(x, team[i].Position.y, z));
			}
			else
			{
				team[i].Teleport(new float3(0, 0, 0));
			}
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
}
