using System;
using System.Collections.Generic;
using PvpArena.Models;

namespace PvpArena.Services.Moderation;

public static class BanService
{
	public static void BanPlayer(ulong steamId, int banDurationDays, string reason = "No reason given")
	{
		var Player = PlayerService.GetPlayerFromSteamId(steamId);
		Player.BanInfo = new PlayerBanInfo
		{
			SteamID = Player.SteamID,
			BannedDate = DateTime.UtcNow,
			BanDurationDays = banDurationDays,
			Reason = reason
		};

		Core.banDataRepository.SaveDataAsync(new List<PlayerBanInfo> { Player.BanInfo });
	}

	public static void UnbanPlayer(ulong steamId)
	{
		var player = PlayerService.GetPlayerFromSteamId(steamId);
		player.BanInfo = new PlayerBanInfo();
		player.BanInfo.SteamID = player.SteamID;
		Core.banDataRepository.SaveDataAsync(new List<PlayerBanInfo> { player.BanInfo });
	}
}

