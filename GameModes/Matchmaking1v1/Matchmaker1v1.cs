using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM.Network;
using Unity.Mathematics;
using PvpArena.Data;
using ProjectM;
using Bloodstone.API;
using PvpArena.Services;
using PvpArena.Models;
using PvpArena.Helpers;
using PvpArena.Configs;
using PvpArena.Persistence.MySql.PlayerDatabase;
using System.Threading;

namespace PvpArena.GameModes.Matchmaking1v1;

public static class MatchmakingService
{
	private static List<Timer> Timers = new List<Timer>();
	public static void Initialize()
	{
		foreach (var matchmakingArenaLocation in Core.matchmaking1v1ArenaLocations)
		{
			ArenaManager.AddArena(new Arena()
			{
				Location1 = matchmakingArenaLocation.Location1.ToFloat3(),
				Location2 = matchmakingArenaLocation.Location2.ToFloat3(),
				IsOccupied = false
			});
		}

		var lookForMatchesAction = () => MatchmakingService.LookForMatches();
		var timer = ActionScheduler.RunActionEveryInterval(lookForMatchesAction, 15);
		Timers.Add(timer);
	}

	public static void Dispose()
	{
		ArenaManager.Dispose();
		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
	}

	public static void LookForMatches()
	{
		var match = MatchmakingQueue.Matchmaker.FindMatch();
		while (match != null)
		{
			MatchmakingQueue.MatchManager.StartMatch(match.Item1, match.Item2);
			match = MatchmakingQueue.Matchmaker.FindMatch();
		}
	}
}

public static class MatchmakingQueue
{
	private static List<Player> _queue = new List<Player>();
	public static Dictionary<Player, Player> ActiveMatches = new Dictionary<Player, Player>();

	public static void Join(Player player)
	{
		if (player.IsEligibleForMatchmaking() && !_queue.Contains(player))
		{
			_queue.Add(player);
		}
	}

	public static void Leave(Player player)
	{
		if (_queue.Contains(player))
		{
			_queue.Remove(player);
		}
	}

	public static List<Player> GetQueue()
	{
		return _queue;
	}

	public static class Matchmaker
	{
		private const int MMR_TOLERANCE = 150; // change as required
		private const int MAX_RECENT_MATCHES = 5; // number of recent matches to keep track of
		private static List<Tuple<Player, Player>> recentMatchedPairs = new List<Tuple<Player, Player>>();

		public static Tuple<Player, Player> FindMatch()
		{
			var players = GetQueue();

			// First try to find a match that hasn't occurred recently
			foreach (var player1 in players)
			{
				foreach (var player2 in players.Where(p => p != player1))
				{
					if (Math.Abs(player1.MatchmakingData1v1.MMR - player2.MatchmakingData1v1.MMR) <= MMR_TOLERANCE &&
						!WasRecentlyMatched(player1, player2))
					{
						var currentMatchedPair = new Tuple<Player, Player>(player1, player2);
						AddToRecentMatches(currentMatchedPair);
						return currentMatchedPair;
					}
				}
			}

			// If we reach here, it means no new match-ups were found, so now allow recent match-ups
			foreach (var player1 in players)
			{
				foreach (var player2 in players.Where(p => p != player1))
				{
					if (Math.Abs(player1.MatchmakingData1v1.MMR - player2.MatchmakingData1v1.MMR) <= MMR_TOLERANCE)
					{
						var currentMatchedPair = new Tuple<Player, Player>(player1, player2);
						AddToRecentMatches(currentMatchedPair);
						return currentMatchedPair;
					}
				}
			}

			return null;
		}

		private static bool WasRecentlyMatched(Player player1, Player player2)
		{
			return recentMatchedPairs.Any(pair =>
				pair.Item1 == player1 && pair.Item2 == player2 ||
				pair.Item2 == player1 && pair.Item1 == player2);
		}

		private static void AddToRecentMatches(Tuple<Player, Player> matchedPair)
		{
			recentMatchedPairs.Add(matchedPair);
			if (recentMatchedPairs.Count > MAX_RECENT_MATCHES)
			{
				recentMatchedPairs.RemoveAt(0); // remove the oldest match
			}
		}
	}

	public class MatchManager
	{
		public static void StartMatch(Player p1, Player p2)
		{
			Helper.BuffEntity(p1.Character, Prefabs.Buff_Manticore_ImmaterialHomePos, out var buffEntity);
			Helper.BuffEntity(p2.Character, Prefabs.Buff_Manticore_ImmaterialHomePos, out buffEntity);

			p1.ReceiveMessage("Found a match!".Success() + " Prepare for teleportation..".White());
			p2.ReceiveMessage("Found a match!".Success() + " Prepare for teleportation..".White());

			ActiveMatches[p1] = p2;
			ActiveMatches[p2] = p1;

			// Remove players from the queue.
			Leave(p1);
			Leave(p2);

			var arena = ArenaManager.GetAvailableArena();
			ArenaManager.MarkArenaAsOccupied(arena, p1, p2);

			p1.MatchmakingData1v1.ReturnLocation = p1.Position;
			p2.MatchmakingData1v1.ReturnLocation = p2.Position;

			// Mark players as in-match.
			p1.CurrentState = Player.PlayerState.In1v1Matchmaking;
			p2.CurrentState = Player.PlayerState.In1v1Matchmaking;

			var action = () => BringPlayers(p1, p2, arena);
			ActionScheduler.RunActionOnceAfterDelay(action, 3);
		}

		public static void EndMatch(Player winner, Player loser, bool teleportLoser = true)
		{
			ActiveMatches.Remove(winner);
			ActiveMatches.Remove(loser);

			int newWinnerMmr =
				MmrCalculator.CalculateNewMmr(winner.MatchmakingData1v1.MMR, loser.MatchmakingData1v1.MMR, true);
			int newLoserMmr =
				MmrCalculator.CalculateNewMmr(loser.MatchmakingData1v1.MMR, winner.MatchmakingData1v1.MMR, false);

			winner.ReceiveMessage("You won!".Success() + " Gained ".White() +
								  $"{newWinnerMmr - winner.MatchmakingData1v1.MMR}".Success() +
								  " points. New score: ".White() + $"{newWinnerMmr}".Warning());
			loser.ReceiveMessage("You lost.".Error() + " Lost ".White() +
								 $"{newLoserMmr - loser.MatchmakingData1v1.MMR}".Error() +
								 " points. New score: ".White() + $"{newLoserMmr}".Warning());

			int pointsToAward = 1;
			winner.PlayerPointsData.AddPointsToAllRegions(pointsToAward);
			loser.PlayerPointsData.AddPointsToAllRegions(pointsToAward);
			Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { winner.PlayerPointsData, loser.PlayerPointsData });
			string pointsName = "VPoint";
			if (pointsToAward > 1)
			{
				pointsName += "s";
			}
			string winnerVPointsMessage = $"Received {pointsToAward.ToString().Emphasize()} {pointsName.Warning()} for participating in matchmaking. New total: {winner.PlayerPointsData.TotalPoints.ToString().Warning}".White();
			string loserVPointsMessage = $"Received {pointsToAward.ToString().Emphasize()} {pointsName.Warning()} for participating in matchmaking. New total: {loser.PlayerPointsData.TotalPoints.ToString().Warning}".White();
			winner.ReceiveMessage(winnerVPointsMessage);
			loser.ReceiveMessage(loserVPointsMessage);


			winner.MatchmakingData1v1.MMR = newWinnerMmr;
			loser.MatchmakingData1v1.MMR = newLoserMmr;

			winner.MatchmakingData1v1.Wins += 1;
			loser.MatchmakingData1v1.Losses += 1;

			List<PlayerMatchmaking1v1Data> dataToUpdate = new List<PlayerMatchmaking1v1Data>
			{
				winner.MatchmakingData1v1,
				loser.MatchmakingData1v1
			};
			Core.matchmaking1V1DataRepository.SaveDataAsync(dataToUpdate);

			var action = () => ReturnPlayers(winner, loser, teleportLoser);
			ActionScheduler.RunActionOnceAfterDelay(action, 3);
		}

		private static void ReturnPlayers(Player winner, Player loser, bool teleportLoser = true)
		{
			winner.Reset(Helper.ResetOptions.FreshMatch);
			loser.Reset(Helper.ResetOptions.FreshMatch);

			// Teleport players back to their original location.
			winner.Teleport(winner.MatchmakingData1v1.ReturnLocation);
			if (teleportLoser)
			{
				loser.Teleport(loser.MatchmakingData1v1.ReturnLocation);
			}

			var arena = winner.MatchmakingData1v1.CurrentArena;
			ArenaManager.FreeArena(arena);

			// Reset match status.
			winner.CurrentState = Player.PlayerState.Normal;
			loser.CurrentState = Player.PlayerState.Normal;
			string message = $"Re-entering the {"Ranked".Colorify(ExtendedColor.ServerColor)} queue.".White();

			if (winner.MatchmakingData1v1.AutoRequeue)
			{
				winner.ReceiveMessage(message);
				Join(winner);
			}

			if (loser.MatchmakingData1v1.AutoRequeue)
			{
				loser.ReceiveMessage(message);
				Join(loser);
			}
		}

		private static void BringPlayers(Player p1, Player p2, Arena arena)
		{
			if (p1.User.Read<Team>().Equals(p2.User.Read<Team>()))
			{
				if (Core.clanSystem.Outranks(p1.User, p2.User))
				{
					p2.ReceiveMessage($"You and your opponent are in the same clan, removing you from clan.."
						.Warning());
					p2.RemoveFromClan();
				}
				else
				{
					p1.ReceiveMessage($"You and your opponent are in the same clan, removing you from clan.."
						.Warning());
					p1.RemoveFromClan();
				}
			}

			List<Player> players = new List<Player>
			{
				p1, p2
			};


			foreach (var player in players)
			{
				player.Reset(Helper.ResetOptions.FreshMatch);
				Helper.BuffPlayer(player, Prefabs.AB_InvisibilityAndImmaterial_Buff, out var buffEntity, 1);
				Helper.SetDefaultBlood(player, PvpArenaConfig.Config.DefaultArenaBlood);
			}

			var action = () => BringPlayersPart2(p1, p2, arena);
			ActionScheduler.RunActionOnceAfterFrames(action, 2);
		}

		private static void BringPlayersPart2(Player p1, Player p2, Arena arena)
		{
			// Teleport players to match location.
			Helper.ApplyMatchInitializationBuff(p1);
			Helper.ApplyMatchInitializationBuff(p2);
			p1.Teleport(arena.Location1);
			p2.Teleport(arena.Location2);
		}
	}
}

public static class MatchmakingHelper
{
	public static Player GetOpponentForPlayer(Player player)
	{
		if (player != null && MatchmakingQueue.ActiveMatches.TryGetValue(player, out var opponent))
		{
			return opponent;
		}

		return null;
	}
}

public class Arena
{
	public float3 Location1 { get; set; }
	public float3 Location2 { get; set; }
	public bool IsOccupied { get; set; }
	public Player Player1 { get; set; }
	public Player Player2 { get; set; }
}

public static class ArenaManager
{
	private static List<Arena> _arenas = new List<Arena>();

	public static void AddArena(Arena arena)
	{
		_arenas.Add(arena);
	}

	public static void RemoveArena(Arena arena)
	{
		_arenas.Remove(arena);
	}

	public static Arena GetAvailableArena()
	{
		return _arenas.FirstOrDefault(arena => !arena.IsOccupied);
	}

	public static void MarkArenaAsOccupied(Arena arena, Player player1, Player player2)
	{
		arena.Player1 = player1;
		arena.Player2 = player2;
		arena.IsOccupied = true;
		player1.MatchmakingData1v1.CurrentArena = arena;
		player2.MatchmakingData1v1.CurrentArena = arena;
	}

	public static void FreeArena(Arena arena)
	{
		arena.Player1.MatchmakingData1v1.CurrentArena = default;
		arena.Player2.MatchmakingData1v1.CurrentArena = default;
		arena.Player1 = default;
		arena.Player2 = default;
		arena.IsOccupied = false;
	}

	public static void Dispose()
	{
		_arenas.Clear();
	}
}
