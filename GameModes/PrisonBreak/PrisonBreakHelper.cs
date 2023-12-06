using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ProjectM;
using PvpArena.Data;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using Unity.Mathematics;
using static PvpArena.Helpers.Helper;

namespace PvpArena.GameModes.PrisonBreak;

public static class PrisonBreakHelper
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

	private static void StartMatchCountdown()
	{
		for (int i = 5; i >= 0; i--)
		{
			int countdownNumber = i;

			Action action = () =>
			{
				foreach (var player in PrisonBreakGameMode.PlayersAlive.Keys)
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
			};

			Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5 - countdownNumber);
			CaptureThePancakeGameMode.Timers.Add(timer);
		}
	}

	private static List<Entity> GetAllGates()
	{
		var allGates = GetEntitiesByComponentTypes<Door>(true);
		var prisonZone = PrisonBreakConfig.Config.PrisonZone.ToRectangleZone();
		List<Entity> prisonGates = new List<Entity>();
		foreach (var gate in allGates)
		{
			if (prisonZone.Contains(gate))
			{
				prisonGates.Add(gate);
			}
		}
		return prisonGates;
	}

	public static void StartMatch()
	{
		Core.prisonBreakGameMode.Initialize();
		Action action;
		var gates = GetAllGates();
		foreach (var gate in gates)
		{
			var door = gate.Read<Door>();
			door.OpenState = false;
			gate.Write(door);
		}

		var index = 0;
		foreach (var player in PrisonBreakGameMode.PlayersAlive.Keys)
		{
			if (player.IsInDefaultMode())
			{
				player.RemoveFromClan();
				player.CurrentState = Player.PlayerState.PrisonBreak;
				player.Reset(PrisonBreakGameMode.ResetOptions);
				SetDefaultBlood(player, PrisonBreakConfig.Config.DefaultBlood);
				player.Teleport(PrisonConfig.Config.CellCoordinateList[index].ToFloat3());
				BuffPlayer(player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff, out var buffEntity, NO_DURATION);
				BuffPlayer(player, Prefabs.AB_Consumable_SpellBrew_T02_Buff, out buffEntity, NO_DURATION);
				if (BuffPlayer(player, Prefabs.Buff_General_Phasing, out buffEntity, 10))
				{
					ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair);
				}

				index++;
				player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
			}
		}

		action = () => { StartMatchCountdown(); };

		Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5);
		PrisonBreakGameMode.Timers.Add(timer);
		action = () =>
		{
			foreach (var gate in gates)
			{
				var door = gate.Read<Door>();
				door.OpenState = true;
				gate.Write(door);
			}
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 10);
	}

	public static void EndMatch(Player winner = null)
	{
		try
		{
			if (winner != null)
			{
				Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>
				{
					{ 1, new List<Player> { winner } },
					{ 2, PrisonBreakGameMode.PlayersAlive.Keys.Where(player => player != winner).ToList() }
				};

				foreach (var player in PrisonBreakGameMode.PlayersAlive.Keys)
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					RespawnPlayer(player, player.Position);
					player.Reset(ResetOptions.FreshMatch);
				}

				var action = () =>
				{
					TeleportTeamsToCenter(Teams, 1, TeamSide.East);
					Core.prisonBreakGameMode.Dispose();
					DisposeTimers();
				};
				ActionScheduler.RunActionOnceAfterDelay(action, .1);
				Core.prisonBreakGameMode.Dispose();
			}
			else
			{

				foreach (var player in PrisonBreakGameMode.PlayersAlive.Keys)
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					RespawnPlayer(player, player.Position);
					player.Reset(ResetOptions.FreshMatch);
				}
				Core.prisonBreakGameMode.Dispose();
				DisposeTimers();
			}
		}
		catch (Exception e)
		{
			Core.prisonBreakGameMode.Dispose();
			DisposeTimers();
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
		var mapCenter = PrisonBreakConfig.Config.MapCenter;
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
			ApplyWinnerMatchEndBuff(winner);
		}
		foreach (var loser in losers)
		{
			ApplyLoserMatchEndBuff(loser);
		}
	}
}
