using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Bloodstone.API;
using Discord;
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
using Color = UnityEngine.Color;

namespace PvpArena.GameModes.OD;

public static class ODManager
{
	private static Dictionary<int, ODGameMode> ODGameModes = new Dictionary<int, ODGameMode>();

	public static void Dispose()
	{
		foreach (var kvp in ODGameModes)
		{
			EndMatch(kvp.Key);
		}

		ODGameModes.Clear();
	}

	public static void StartMatch(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		var team1 = team1LeaderPlayer.GetClanMembers();
		var team2 = team2LeaderPlayer.GetClanMembers();
		foreach (var player in team1)
		{
			player.CurrentState = Player.PlayerState.OD;
			player.MatchmakingTeam = 1;
			player.Reset(Helper.ResetOptions.FreshMatch);
			Helper.SetDefaultBlood(player, "warrior");

			Helper.BuffPlayer(player, Prefabs.AB_Consumable_Eat_TrippyShroom_Buff, out var buffEntity, Helper.NO_DURATION);
			Helper.ApplyMatchInitializationBuff(player, 15);
		}

		foreach (var player in team2)
		{
			player.CurrentState = Player.PlayerState.OD;
			player.MatchmakingTeam = 2;
			player.Reset(Helper.ResetOptions.FreshMatch);
			Helper.SetDefaultBlood(player, "warrior");
			Helper.BuffPlayer(player, Prefabs.AB_Consumable_Eat_TrippyShroom_Buff, out var buffEntity, Helper.NO_DURATION);
			Helper.ApplyMatchInitializationBuff(player, 15);
		}

		var ODGameMode = new ODGameMode();
		ODGameMode.Initialize(team1, team2);
		ODGameModes[ODGameMode.MatchNumber] = ODGameMode;
	}

	public static int FindMatchNumberByPlayer(Player player)
	{
		foreach (var gameMode in ODGameModes.Values)
		{
			if (gameMode.Teams[1].Contains(player) || gameMode.Teams[2].Contains(player))
			{
				return gameMode.MatchNumber;
			}
		}
		return -1;
	}

	public static void EndMatch(int matchNumber, int winnerTeam = 1)
	{
		try
		{
			foreach (var team in ODGameModes[matchNumber].Teams.Values)
			{
				foreach (var player in team)
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					player.Reset(Helper.ResetOptions.FreshMatch);
				}
			}
			if (winnerTeam > 0 && ODGameModes[matchNumber].Teams.Count > 0)
			{
				var action = () => {
					ODGameModes[matchNumber].Dispose();
				};
				ActionScheduler.RunActionOnceAfterDelay(action, .1);
			}
			else
			{
				ODGameModes[matchNumber].Dispose();
			}
		}
		catch (Exception e)
		{
			ODGameModes[matchNumber].Dispose();
			Plugin.PluginLog.LogError(e.ToString());
		}
	}
}
