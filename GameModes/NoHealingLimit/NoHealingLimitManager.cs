using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ProjectM;
using ProjectM.Behaviours;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.GameModes.Domination;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Mathematics;
using UnityEngine;
using static PvpArena.Factories.UnitFactory;
using static PvpArena.Helpers.Helper;

namespace PvpArena.GameModes.Troll;

public static class NoHealingLimitManager
{
	public static void AddPlayer(Player player)
	{
		if (NoHealingLimitGameMode.Players.Count == 0)
		{
			Core.noHealingLimitGameMode.Initialize();
		}
		player.Reset(NoHealingLimitGameMode.ResetOptions);
		player.CurrentState = Player.PlayerState.NoHealingLimit;
		NoHealingLimitGameMode.Players.Add(player);
		Helper.BuffPlayer(player, Helper.CustomBuff4, out var buffEntity, Helper.NO_DURATION);
		player.ReceiveMessage($"You have entered {"No Healing Limit".Emphasize()} mode.".White());
	}

	public static void RemovePlayer(Player player)
	{
		player.CurrentState = Player.PlayerState.Normal;
		NoHealingLimitGameMode.Players.Remove(player);
		if (NoHealingLimitGameMode.Players.Count == 0)
		{
			Core.noHealingLimitGameMode.Dispose();
		}
		player.Reset(ResetOptions.FreshMatch);
		player.ReceiveMessage($"You have left {"No Healing Limit".Emphasize()} mode.".White());
	}
}
