using ProjectM.Network;
using System.Linq;
using System;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Models;
using PvpArena.Configs;
using PvpArena.GameModes.Matchmaking1v1;
using PvpArena.GameModes.CaptureThePancake;

namespace PvpArena.Commands;

internal static class MatchmakingCommands
{
	[Command("ranked join", description: "Enters the matchmaking queue", aliases: new string[] { "join" }, adminOnly: false, includeInHelp: false, category: "Ranked")]
	public static void JoinMatchmakingCommand(Player sender, bool autoRequeue = false)
	{
		if (!PvpArenaConfig.Config.MatchmakingEnabled)
		{
			sender.ReceiveMessage("Matchmaking is currently disabled!".Error());
			return;
		}

		sender.MatchmakingData1v1.AutoRequeue = autoRequeue;
		MatchmakingQueue.Join(sender);
		sender.ReceiveMessage($"Joined the queue. Auto Re-queue: {autoRequeue.ToString().Emphasize()}".White());
	}

	[Command("ranked leave", description: "Leaves the matchmaking queue", aliases: new string[] { "leave" }, adminOnly: false, includeInHelp: false, category: "Ranked")]
	public static void LeaveMatchmakingCommand(Player sender)
	{
		if (!PvpArenaConfig.Config.MatchmakingEnabled)
		{
			sender.ReceiveMessage("Matchmaking is currently disabled!".Error());
			return;
		}

		MatchmakingQueue.Leave(sender);
		sender.MatchmakingData1v1.AutoRequeue = false;
		sender.ReceiveMessage("Left the queue!".Success());
	}

	[Command("lb ranked", description: "Displays the ranked leaderboard", adminOnly: false, includeInHelp: false, category: "Ranked")]
	public static void ShowLeaderboardCommand(Player sender, int pageNumber = 1)
	{
		const int playersPerPage = 10; // Number of players to display per page

		// Ensure the page number is positive
		if (pageNumber < 1)
		{
			sender.ReceiveMessage("Page number must be positive.".Error());
			return;
		}

		var orderedPlayers = PlayerService.UserCache.Values
								.Select(player => new {
									player.SteamID,
									player.MatchmakingData1v1.MMR,
									player.MatchmakingData1v1.Wins,
									player.MatchmakingData1v1.Losses,
									player.Name // Assuming Name gets the player's display name
								})
								.Where(player => player.Wins != 0 || player.Losses != 0)
								.OrderByDescending(player => player.MMR)
								.ToList();

		// Calculate start and end indices for the current page
		int startIndex = (pageNumber - 1) * playersPerPage;
		int endIndex = Math.Min(startIndex + playersPerPage, orderedPlayers.Count);

		if (orderedPlayers.Count == 0)
		{
			sender.ReceiveMessage("No players available on the leaderboard.".Error());
			return;
		}

		if (startIndex >= orderedPlayers.Count)
		{
			sender.ReceiveMessage("The page number is too high.".Error());
			return;
		}

		sender.ReceiveMessage("Ranked Leaderboard:".Colorify(ExtendedColor.LightServerColor) + $" Page {pageNumber.ToString().Emphasize()}".White());

		var currentPlayerSteam = sender.SteamID;
		for (int i = startIndex; i < endIndex; i++)
		{
			var player = orderedPlayers[i];

			string playerRankInfo = FormatPlayerRankInfo(player, i + 1, currentPlayerSteam);
			sender.ReceiveMessage(playerRankInfo);
		}

		// If current player was not shown in the displayed page, show them at the end
		int currentPlayerIndex = orderedPlayers.FindIndex(p => p.SteamID == currentPlayerSteam);
		if (currentPlayerIndex < startIndex || currentPlayerIndex >= endIndex)
		{
			if (currentPlayerIndex != -1) // If the player is in the list
			{
				sender.ReceiveMessage($"...........................................................".Colorify(ExtendedColor.LightServerColor));
				var currentPlayer = orderedPlayers[currentPlayerIndex];
				string playerRankInfo = FormatPlayerRankInfo(currentPlayer, currentPlayerIndex + 1, currentPlayerSteam, bold: true);
				sender.ReceiveMessage(playerRankInfo);
			}
		}
	}

	private static string FormatPlayerRankInfo(dynamic player, int rank, ulong currentPlayerSteam, bool bold = false)
	{
		string rankColor = "#FFFFFF"; // Default rank color
		string boldStart = bold ? "<b>" : "";
		string boldEnd = bold ? "</b>" : "";
		string highlightStart = player.SteamID == currentPlayerSteam ? "<color=#FFD700>" : ""; // Highlight color if it's the current player
		string highlightEnd = player.SteamID == currentPlayerSteam ? "</color>" : "";

		return $"{boldStart}"+$"{rank}".Colorify(ExtendedColor.LightServerColor) + $" - {highlightStart}<color={rankColor}>{player.Name}</color>{highlightEnd}{boldEnd}: ".White() + $"{player.MMR.ToString()}".Warning() + $" points - W: <color=#00FF00>{player.Wins}</color> / L: <color=#FF0000>{player.Losses}</color>".White();
	}

	private static string FormatPlayerBulletHellInfo(dynamic player, int rank, ulong currentPlayerSteam, bool bold = false)
	{
		string rankColor = "#FFFFFF"; // Default rank color
		string boldStart = bold ? "<b>" : "";
		string boldEnd = bold ? "</b>" : "";
		string highlightStart = player.SteamID == currentPlayerSteam ? "<color=#FFD700>" : ""; // Highlight color if it's the current player
		string highlightEnd = player.SteamID == currentPlayerSteam ? "</color>" : "";

		//string message = $"{boldStart}{rank}. {highlightStart}<color={rankColor}>{player.Name}</color>{highlightEnd}{boldEnd} - {player.BestTime}".White();
		return $"{boldStart}"+$"{rank}".Colorify(ExtendedColor.LightServerColor) + $" - {highlightStart}<color={rankColor}>{player.Name}</color>{highlightEnd}{boldEnd}: ".White() + $"{player.BestTime}".Warning();
	}

	[Command("lb bullet", description: "Displays the ranked leaderboard", adminOnly: false, includeInHelp: false, category: "Ranked")]
	public static void ShowBulletLeaderboardCommand(Player sender, int pageNumber = 1)
	{
		const int playersPerPage = 10; // Number of players to display per page

		// Ensure the page number is positive
		if (pageNumber < 1)
		{
			sender.ReceiveMessage("Page number must be positive.".Error());
			return;
		}

		var orderedPlayers = PlayerService.UserCache.Values
						.Select(player => new {
							player.SteamID,
							BestTime = double.Parse(player.PlayerBulletHellData.BestTime),
							player.Name // Assuming Name gets the player's display name
						})
						.Where(player => player.BestTime != 0)
						.OrderByDescending(player => player.BestTime)
						.ToList();


		// Calculate start and end indices for the current page
		int startIndex = (pageNumber - 1) * playersPerPage;
		int endIndex = Math.Min(startIndex + playersPerPage, orderedPlayers.Count);

		if (orderedPlayers.Count == 0)
		{
			sender.ReceiveMessage("No players available on the leaderboard.".Error());
			return;
		}

		if (startIndex >= orderedPlayers.Count)
		{
			sender.ReceiveMessage("The page number is too high.".Error());
			return;
		}

		sender.ReceiveMessage("Bullet Hell Leaderboard:".Colorify(ExtendedColor.LightServerColor) + $" Page {pageNumber.ToString().Emphasize()}".White());

		var currentPlayerSteam = sender.SteamID;
		for (int i = startIndex; i < endIndex; i++)
		{
			var player = orderedPlayers[i];

			string playerRankInfo = FormatPlayerBulletHellInfo(player, i + 1, currentPlayerSteam);
			sender.ReceiveMessage(playerRankInfo);
		}

		// If current player was not shown in the displayed page, show them at the end
		int currentPlayerIndex = orderedPlayers.FindIndex(p => p.SteamID == currentPlayerSteam);
		if (currentPlayerIndex < startIndex || currentPlayerIndex >= endIndex)
		{
			if (currentPlayerIndex != -1) // If the player is in the list
			{
				sender.ReceiveMessage($"...........................................................".Colorify(ExtendedColor.LightServerColor));
				var currentPlayer = orderedPlayers[currentPlayerIndex];
				string playerRankInfo = FormatPlayerBulletHellInfo(currentPlayer, currentPlayerIndex + 1, currentPlayerSteam, bold: true);
				sender.ReceiveMessage(playerRankInfo);
			}
		}
	}

	[Command("lb pancake", description: "Displays the current pancake match's stats", adminOnly: false)]
	public static void PancakeLeaderboardCommand(Player sender, int arenaNumber = -1)
	{
		if (arenaNumber == -1)
		{
			if (sender.IsInCaptureThePancake())
			{
				for (var i = 0; i < CaptureThePancakeManager.captureThePancakeGameModes.Count; i++)
				{
					if (CaptureThePancakeManager.captureThePancakeGameModes[i].Players.Contains(sender))
					{
						arenaNumber = i;
						break;
					}
				}
			}
			else
			{
				arenaNumber = 0;
			}
		}
		else
		{
			arenaNumber--;
		}
		
		if (arenaNumber < 0 || arenaNumber >= CaptureThePancakeManager.captureThePancakeGameModes.Count)
		{
			sender.ReceiveMessage("Invalid arena number specified".Error());
			return;
		}
		if (CaptureThePancakeManager.captureThePancakeGameModes[arenaNumber].MatchActive)
		{
			sender.ReceiveMessage($"Current Pancake Stats For Arena #{arenaNumber+1}".White());
			CaptureThePancakeManager.captureThePancakeGameModes[arenaNumber].ReportStatsToPlayer(sender);
		}
		else
		{
			sender.ReceiveMessage($"There is no active pancake match for Arena #{arenaNumber+1}".Error());
		}
	}

}
