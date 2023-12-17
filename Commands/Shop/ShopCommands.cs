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
		sender.ReceiveMessage(($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} {"VPoint(s)".Warning()} are now {player.PlayerPointsData.TotalPoints.ToString().Emphasize()}.").White());
		
		player.ReceiveMessage($"An admin added {points.ToString().Emphasize()} {"VPoint(s)".Warning()} to you.".Success());
		player.ReceiveMessage(($"Your {"VPoint(s)".Warning()} are now {player.PlayerPointsData.TotalPoints.ToString().Emphasize()}.").White());
	}

	[Command("remove-points", adminOnly: true, aliases: new string[] { "removepoints", "remove points" })]
	public static void RemovePointsCommand(Player sender, int points, Player foundPlayer = null)
	{
		Player player = foundPlayer ?? sender;

		player.PlayerPointsData.TotalPoints = Math.Max(player.PlayerPointsData.TotalPoints - points, 0);
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		sender.ReceiveMessage($"Removed {points.ToString().Emphasize()} {"VPoint(s)".Warning()} to {player.Name.Colorify(ExtendedColor.ClanNameColor)} ".Success());
		sender.ReceiveMessage(($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} {"VPoint(s)".Warning()} are now {player.PlayerPointsData.TotalPoints.ToString().Emphasize()}.").White());
		
		player.ReceiveMessage($"An admin removed {points.ToString().Emphasize()} {"VPoint(s)".Warning()} to you.".Error());
		player.ReceiveMessage(($"Your {"VPoint(s)".Warning()} are now {player.PlayerPointsData.TotalPoints.ToString().Emphasize()}.").White());
	}

	[Command("set-points", adminOnly: true, aliases: new string[] { "setpoints", "set points"})]
	public static void SetPointsCommand(Player sender, int points, Player foundPlayer = null)
	{
		Player player = foundPlayer ?? sender;

		player.PlayerPointsData.TotalPoints = Math.Max(points, 0);
		Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
		sender.ReceiveMessage($"Set {player.Name.Colorify(ExtendedColor.ClanNameColor)}'s {"VPoint(s)".Warning()} to {points.ToString().Emphasize()}".Success());
		sender.ReceiveMessage(($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} {"VPoint(s)".Warning()} are now {player.PlayerPointsData.TotalPoints.ToString().Emphasize()}.").White());
		
		player.ReceiveMessage($"An admin set {points.ToString().Emphasize()} {"VPoint(s)".Warning()} to you.".Success());
		player.ReceiveMessage(($"Your {"VPoint(s)".Warning()} are now {player.PlayerPointsData.TotalPoints.ToString().Emphasize()}.").White());
	}

	[Command("get-points", adminOnly: true, aliases: new string[] { "getpoints", "get points" })]
	public static void GetPointsCommand(Player sender, Player player)
	{
		sender.ReceiveMessage($"{player.Name.Colorify(ExtendedColor.ClanNameColor)} has {player.PlayerPointsData.TotalPoints.ToString().Emphasize()} {"VPoint(s)".Warning()}".White());
	}
}
