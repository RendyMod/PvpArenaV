using PvpArena.Data;
using ProjectM.Shared;
using Unity.Entities;
using ProjectM;
using Bloodstone.API;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ProjectM.Network;
using System.Linq;
using PvpArena.Services;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands;

internal static class ShopCommands
{
	[Command("points", description: "Displays your points", usage: ".points", adminOnly: false, includeInHelp: false, category: "Shop")]
	public static void PointsCommand(Player sender)
	{
		sender.ReceiveMessage($"Total points: {sender.PlayerPointsData.TotalPoints.ToString().Emphasize()}".White());
	}

	[Command("add-points", adminOnly: true)]
	public static void AddPointsCommand(Player sender, int points, Player foundPlayer = null)
	{
		Player player = foundPlayer ?? sender;
		
		player.PlayerPointsData.TotalPoints += points;
		sender.ReceiveMessage($"Added {points} points to {player.Name}");
	}
}
