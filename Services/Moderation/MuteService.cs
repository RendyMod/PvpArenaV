using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bloodstone.API;
using HarmonyLib;
using Il2CppInterop.Runtime;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;

namespace PvpArena.Services.Moderation;

public class MuteService
{
	public static void MutePlayer(Player player, int durationDays)
	{
		player.MuteInfo = new PlayerMuteInfo(player.SteamID, DateTime.UtcNow, durationDays);
		Core.muteDataRepository.SaveDataAsync(new List<PlayerMuteInfo> { player.MuteInfo });
	}

	public static void UnmutePlayer(ulong platformId)
	{
		var Player = PlayerService.GetPlayerFromSteamId(platformId);
		Player.MuteInfo = new PlayerMuteInfo();
		Player.MuteInfo.SteamID = platformId;
		Core.muteDataRepository.SaveDataAsync(new List<PlayerMuteInfo> { Player.MuteInfo });
	}
}

