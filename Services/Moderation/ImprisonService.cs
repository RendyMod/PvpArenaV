using System;
using System.Collections.Generic;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Mathematics;
using UnityEngine.Rendering.HighDefinition;

namespace PvpArena.Services.Moderation;

public static class ImprisonService
{
	public static void ImprisonPlayer(ulong steamId, int ImprisonDurationDays, string reason = "No reason given")
	{
		var player = PlayerService.GetPlayerFromSteamId(steamId);
		player.ImprisonInfo = new PlayerImprisonInfo
		{
			SteamID = player.SteamID,
			ImprisonedDate = DateTime.UtcNow,
			ImprisonDurationDays = ImprisonDurationDays,
			Reason = reason,
			PrisonCellNumber = GetNextCell()
		};
		player.CurrentState = Player.PlayerState.Imprisoned;
		player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
		Core.imprisonDataRepository.SaveDataAsync(new List<PlayerImprisonInfo> { player.ImprisonInfo });
	}

	public static void UnimprisonPlayer(ulong steamId)
	{
		var player = PlayerService.GetPlayerFromSteamId(steamId);
		player.ImprisonInfo = new PlayerImprisonInfo();
		player.ImprisonInfo.SteamID = player.SteamID;
		player.CurrentState = Player.PlayerState.Normal;
		Core.imprisonDataRepository.SaveDataAsync(new List<PlayerImprisonInfo> { player.ImprisonInfo });
	}

	public static int GetNextCell()
	{
		int totalPlayers = 0;
		foreach (var player in PlayerService.UserCache.Values)
		{
			if (player.IsImprisoned())
			{
				totalPlayers++;
			}
		}
		var totalCells = PrisonConfig.Config.CellCoordinateList.Count;
		var nextCellNumber = (totalPlayers % totalCells); //don't add 1 because current player is already marked as imprisoned
		return nextCellNumber;
	}

	public static float3 GetPlayerCellCoordinates(Player player)
	{
		return PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3();
	}
}

