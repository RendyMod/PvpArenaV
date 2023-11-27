using System.Collections.Generic;
using PvpArena;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class PlayerPrefCommands
{
	[Command("togglekillfeed", description:"Toggles killfeed notifications on and off", usage:".tkf", aliases: new string[] { "tkf" }, adminOnly: false, includeInHelp: false, category: "Preferences")]
	public static void ToggleKillFeed(Player sender)
	{
		sender.ConfigOptions.SubscribeToKillFeed = !sender.ConfigOptions.SubscribeToKillFeed;
		sender.ReceiveMessage($"Turned killfeed: {(sender.ConfigOptions.SubscribeToKillFeed ? "ON".Success() : "OFF".Error())}.".White());
		Core.playerConfigOptionsRepository.SaveDataAsync(new List<PlayerConfigOptions> { sender.ConfigOptions });
	}
}
