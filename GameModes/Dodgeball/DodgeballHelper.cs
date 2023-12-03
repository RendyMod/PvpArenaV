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
using ProjectM.Sequencer;
using ProjectM.Shared;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.HighDefinition;
using static PvpArena.Factories.UnitFactory;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.Dodgeball;

public static class DodgeballHelper
{
	private static AbilityBar dodgeballAbilityBar = new AbilityBar
	{
		Weapon2 = Prefabs.AB_Vampire_Spear_Harpoon_Throw_AbilityGroup,
		Spell1 = Prefabs.AB_Blood_Shadowbolt_AbilityGroup,
		Spell2 = Prefabs.AB_Blood_BloodRite_AbilityGroup
	};
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

	public static void StartMatch(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		var team1Players = team1LeaderPlayer.GetClanMembers();
		var team2Players = team2LeaderPlayer.GetClanMembers();
		Core.dodgeballGameMode.Initialize(team1Players, team2Players);
		SpawnStructures();
		Action action;

		foreach (var team1Player in team1Players)
		{
			team1Player.CurrentState = Player.PlayerState.Dodgeball;
			team1Player.MatchmakingTeam = 1;
			team1Player.Reset(BaseGameMode.ResetOptions);
			SetPlayerAbilities(team1Player);
			Helper.SetPlayerBlood(team1Player, Prefabs.BloodType_Worker, 69);
			action = () => team1Player.Teleport(DodgeballConfig.Config.Team1StartPosition.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(action, .1);
			team1Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		foreach (var team2Player in team2Players)
		{
			team2Player.CurrentState = Player.PlayerState.Dodgeball;
			team2Player.MatchmakingTeam = 2;
			team2Player.Reset(BaseGameMode.ResetOptions);
			SetPlayerAbilities(team2Player);
			Helper.SetPlayerBlood(team2Player, Prefabs.BloodType_Worker, 69);
			action = () => team2Player.Teleport(DodgeballConfig.Config.Team2StartPosition.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(action, .1);
			team2Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		action = () => { StartMatchCountdown(); };

		Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5);
		DodgeballGameMode.Timers.Add(timer);

		action = () => KillPreviousEntities();
		timer = ActionScheduler.RunActionOnceAfterDelay(action, 10);
		DodgeballGameMode.Timers.Add(timer);
	}

	public static void SetPlayerAbilities(Player player)
	{
		Helper.BuffPlayer(player, Helper.CustomBuff4, out var buffEntity, Helper.NO_DURATION, true);
		dodgeballAbilityBar.ApplyChangesHard(buffEntity);
	}

	public static void EndMatch(int winner = 0)
	{
		try
		{
			foreach (var timer in DodgeballGameMode.Timers)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			DodgeballGameMode.Timers.Clear();
			foreach (var team in DodgeballGameMode.Teams.Values)
			{
				foreach (var player in team)
				{
					if (player.IsInDodgeball())
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
			}

			if (winner > 0)
			{
				TeleportTeamsToCenter(DodgeballGameMode.Teams, winner, TeamSide.South);
			}
			KillPreviousEntities();
			Core.dodgeballGameMode.Dispose();
			UnitFactory.DisposeTimers("dodgeball");
			DisposeTimers();
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogError(e.ToString());
		}
	}

	public static void SpawnStructures()
	{
		foreach (var structureSpawn in DodgeballConfig.Config.StructureSpawns)
		{
			var spawnPos = structureSpawn.Location.ToFloat3();
			Action action = () =>
			{
				PrefabSpawnerService.SpawnWithCallback(structureSpawn.PrefabGUID, spawnPos, (e) =>
				{
					
				}, structureSpawn.RotationMode, -1, true, "dodgeball");
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

	private static void StartMatchCountdown()
	{
		for (int i = 5; i >= 0; i--)
		{
			int countdownNumber = i; // Introduce a new variable

			Action action = () =>
			{
				foreach (var team in DodgeballGameMode.Teams.Values)
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
			};

			Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5 - countdownNumber);
			DodgeballGameMode.Timers.Add(timer);
		}
	}

	private static void MessageAllPlayers(string message)
	{
		foreach (var team in DodgeballGameMode.Teams.Values)
		{
			foreach (var player in team)
			{
				player.ReceiveMessage(message);
			}
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
		var mapCenter = DodgeballConfig.Config.MapCenter;
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

	public static void KillPreviousEntities()
	{
		var entities = Helper.GetEntitiesByComponentTypes<CanFly>(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (UnitFactory.TryGetSpawnedUnitFromEntity(entity, out SpawnedUnit spawnedUnit))
				{
					if (spawnedUnit.Unit.Category == "dodgeball")
					{
						Helper.DestroyEntity(entity);
					}
				}
				else
				{
					if (UnitFactory.HasCategory(entity, "dodgeball"))
					{
						Helper.DestroyEntity(entity);
					}
				}
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
