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
		player.Reset(ResetOptions.FreshMatch);
		player.CurrentState = Player.PlayerState.NoHealingLimit;
		NoHealingLimitGameMode.Players.Add(player);
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
		player.ReceiveMessage($"You have left {"No Healing Limit".Emphasize()} mode.".White());
	}
}
