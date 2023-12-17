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
using static PvpArena.Helpers.Helper;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.Dodgeball;

public static class DodgeballHelper
{
	private static AbilityBar dodgeballAbilityBar = new AbilityBar
	{
		Auto = PrefabGUID.Empty,
		Weapon1 = PrefabGUID.Empty,
		Weapon2 = Prefabs.AB_Vampire_Spear_Harpoon_Throw_AbilityGroup,
		Dash = PrefabGUID.Empty,
		Spell1 = Prefabs.AB_Blood_Shadowbolt_AbilityGroup,
		Spell2 = Prefabs.AB_Blood_BloodRite_AbilityGroup,
		Ult = PrefabGUID.Empty
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

		foreach (var team1Player in team1Players)
		{
			team1Player.CurrentState = Player.PlayerState.Dodgeball;
			team1Player.MatchmakingTeam = 1;
			team1Player.Reset(ResetOptions.FreshMatch);
			SetPlayerAbilities(team1Player);
			Helper.SetPlayerBlood(team1Player, Prefabs.BloodType_Worker, 69);
			var teleportPlayerAction = () => team1Player.Teleport(DodgeballConfig.Config.Team1StartPosition.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(teleportPlayerAction, .1);
			team1Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		foreach (var team2Player in team2Players)
		{
			team2Player.CurrentState = Player.PlayerState.Dodgeball;
			team2Player.MatchmakingTeam = 2;
			team2Player.Reset(ResetOptions.FreshMatch);
			SetPlayerAbilities(team2Player);
			Helper.SetPlayerBlood(team2Player, Prefabs.BloodType_Worker, 69);
			var teleportPlayerAction = () => team2Player.Teleport(DodgeballConfig.Config.Team2StartPosition.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(teleportPlayerAction, .1);
			team2Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		var startMatchCountdownAction = () => { StartMatchCountdown(); };

		Timer timer = ActionScheduler.RunActionOnceAfterDelay(startMatchCountdownAction, 5);
		DodgeballGameMode.Timers.Add(timer);

		var killPreviousEntitiesAction = () => KillPreviousEntities(false);
		timer = ActionScheduler.RunActionOnceAfterDelay(killPreviousEntitiesAction, 10);
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
				TeleportTeamsToCenter(DodgeballGameMode.Teams, winner, TeamSide.West);
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
		foreach (var structureSpawn in DodgeballConfig.Config.PrefabSpawns)
		{
			var spawnPos = structureSpawn.Location.ToFloat3();
			Action action = () =>
			{
				PrefabSpawnerService.SpawnWithCallback(structureSpawn.PrefabGUID, spawnPos, (e) =>
				{
					if (e.Read<PrefabGUID>() == Prefabs.EH_Monster_EnergyBeam_Active)
					{
                        e.Add<Immortal>();
                        e.Write(new Immortal
                        {
                            IsImmortal = true
                        });
						var lifetime = e.Read<LifeTime>();
						lifetime.Duration = 0;
						lifetime.EndAction = LifeTimeEndAction.None;
						e.Write(lifetime);
					}
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

	public static void KillPreviousEntities(bool all = true)
	{
		var entities = Helper.GetEntitiesByComponentTypes<CanFly>(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (entity.Read<PrefabGUID>() == Prefabs.EH_Monster_EnergyBeam_Active && !all)
				{
					continue;
				}
				if (UnitFactory.TryGetSpawnedUnitFromEntity(entity, out SpawnedUnit spawnedUnit))
				{
					if (spawnedUnit.Unit.GameMode == "dodgeball")
					{
						Helper.KillOrDestroyEntity(entity);
					}
				}
				else
				{
					if (UnitFactory.HasGameMode(entity, "dodgeball"))
					{
						Helper.KillOrDestroyEntity(entity);
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
