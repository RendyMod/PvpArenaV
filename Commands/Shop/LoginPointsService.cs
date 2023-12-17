using System;
using System.Collections.Generic;
using PvpArena.Configs;
using PvpArena.Models;

namespace PvpArena.Services;

public static class LoginPointsService
{
	public static void AwardPoints(Player player, int points)
	{
		player.PlayerPointsData.AddPointsToAllRegions(points);
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		player.ReceiveMessage($"You were awarded {points.ToString().Emphasize()} {"VPoints".Warning()} for being online. New total: {player.PlayerPointsData.TotalPoints.ToString().Warning()}".White());
	}

	public static void SetTimersForOnlinePlayers()
	{
		foreach (var Player in PlayerService.UserCache.Values)
		{
			if (Player.IsOnline)
			{
				Action action = () => LoginPointsService.AwardPoints(Player, PvpArenaConfig.Config.PointsPerIntervalOnline);
				Player.PlayerPointsData.OnlineTimer = ActionScheduler.RunActionEveryInterval(action, 60 * PvpArenaConfig.Config.IntervalDurationInMinutes);
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

