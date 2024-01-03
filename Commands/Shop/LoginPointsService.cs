using System;
using System.Collections.Generic;
using System.Threading;
using PvpArena.Configs;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.Models;
using UnityEngine;

namespace PvpArena.Services;

public static class LoginPointsService
{
	private static Timer timer;
	public static void Initialize()
	{
		var action = () => AwardPointsToAllOnlinePlayers(PvpArenaConfig.Config.PointsPerIntervalOnline);
		timer = ActionScheduler.RunActionEveryInterval(action, PvpArenaConfig.Config.IntervalDurationInMinutes * 60);
	}
	public static void Dispose()
	{
		if (timer != null)
		{
			timer.Dispose();
			timer = null;
		}
	}

	
	public static void AwardPoints(Player player, int points)
	{
		player.PlayerPointsData.AddPointsToAllRegions(points);
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		player.ReceiveMessage($"You were awarded {points.ToString().Emphasize()} {"VPoints".Warning()} for being online.".White());
		player.ReceiveMessage($"New total: {player.PlayerPointsData.GetPointsFromCurrentRegion().ToString().Warning()}".White());
	}

	public static void TryGrantDailyLoginPoints(Player player, int points)
	{
		DateTime currentTime = DateTime.UtcNow; // Current timestamp
		DateTime currentDate = currentTime.Date; // Current date with time part set to 00:00:00

		// Check if LastLoginTimestamp is null or if the current date is after the date part of the last login timestamp
		if (player.PlayerPointsData.LastLoginDate == null || currentDate > player.PlayerPointsData.LastLoginDate.Value.Date)
		{
			// Logic for granting daily login points
			player.PlayerPointsData.AddPointsToAllRegions(points);
			Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
			player.ReceiveMessage($"You were awarded {points.ToString().Emphasize()} {"VPoints".Warning()} for your daily login.".White());
			player.ReceiveMessage($"New total: {player.PlayerPointsData.GetPointsFromCurrentRegion().ToString().Warning()}".White());
		}

		// Always update the LastLoginTimestamp, whether or not points are awarded
		player.PlayerPointsData.LastLoginDate = currentTime;
	}

	public static void AwardPointsToAllOnlinePlayers(int points)
	{
		foreach (var player in PlayerService.OnlinePlayers)
		{
			AwardPoints(player, points);
		}
	}
	
	[CommandFramework.Command("time", description: "Logs the time of the server", adminOnly: true)]
	public static void LogTimeCommand(Player sender)
	{
		sender.ReceiveMessage($"Time: {(DateTime.UtcNow.ToString()).White()}".Emphasize());
	}

	/*
	public static void SetTimersForOnlinePlayers()
	{
		foreach (var player in PlayerService.OnlinePlayers.Keys)
		{
			if (player.IsOnline)
			{
				Action action = () => LoginPointsService.AwardPoints(player, PvpArenaConfig.Config.PointsPerIntervalOnline);
				player.PlayerPointsData.OnlineTimer = ActionScheduler.RunActionEveryInterval(action, 60 * PvpArenaConfig.Config.IntervalDurationInMinutes);
			}
		}
	}

	public static void DisposeTimersForOnlinePlayers()
	{
		PlayerService.LoadAllPlayers();
		foreach (var Player in PlayerService.OnlinePlayers.Keys)
		{
			if (Player.PlayerPointsData.OnlineTimer != null)
			{
				Player.PlayerPointsData.OnlineTimer.Dispose();
				Player.PlayerPointsData.OnlineTimer = null;
			}
		}
	}
	*/
}

