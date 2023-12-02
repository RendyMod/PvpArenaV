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
	[Command("points", description: "Displays your points", usage: ".points", adminOnly: false, includeInHelp: false, aliases: new string[] { "vpoints" }, category: "Shop")]
	public static void PointsCommand(Player sender)
	{
		sender.ReceiveMessage($"You have {sender.PlayerPointsData.TotalPoints.ToString().Emphasize()} {"VPoint(s)".Warning()}".White());
	}

	[Command("add-points", adminOnly: true, aliases: new string[] { "give-points", "addpoints", "add points", "givepoints", "give points" })]
	public static void AddPointsCommand(Player sender, int points, Player foundPlayer = null)
	{
		Player player = foundPlayer ?? sender;
		
		player.PlayerPointsData.TotalPoints += points;
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		sender.ReceiveMessage($"Added {points.ToString().Emphasize()} {"VPoint(s)".Warning()} to {player.Name.Colorify(ExtendedColor.ClanNameColor)}".Success());
	}
}
