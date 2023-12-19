using System;
using System.Collections.Generic;
using System.Threading;
using PvpArena.Configs;
using PvpArena.Models;

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
		player.PlayerPointsData.TotalPoints += points;
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		player.ReceiveMessage($"You were awarded {points.ToString().Emphasize()} {"VPoints".Warning()} for being online.".White());
		player.ReceiveMessage($"New total: {player.PlayerPointsData.TotalPoints.ToString().Warning()}".White());
	}
	
	public static void DailyLoginPoints(Player player, int points)
	{
		player.PlayerPointsData.TotalPoints += points;
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		player.ReceiveMessage($"You were awarded {points.ToString().Emphasize()} {"VPoints".Warning()} for your daily login.".White());
		player.ReceiveMessage($"New total: {player.PlayerPointsData.TotalPoints.ToString().Warning()}".White());
	}

	public static void AwardPointsToAllOnlinePlayers(int points)
	{
		foreach (var player in PlayerService.OnlinePlayers.Keys)
		{
			AwardPoints(player, points);
		}
	}

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
}

