using Bloodstone.API;
using ProjectM;
using PvpArena.Configs;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.Helpers;
using PvpArena.Models;

public static class ScheduleCommands
{
	[CommandFramework.Command("globaldiscord", description: "Give the link of the discord to everyone",
		usage: ".globaldiscord", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalDiscordCommand (Player sender)
	{
		string message = ("Join us".Emphasize() + " on Discord: ").White() +
		                 $"{PvpArenaConfig.Config.DiscordLink}".Colorify(ExtendedColor.LightServerColor).Emphasize();

		Helper.SendSystemMessageToAllClients(
			message
		);
	}

	[CommandFramework.Command("globalrankedhelp", description: "Send a message to everyone for ranked",
		usage: ".rankedhelp", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalRankedHelpCommand (Player sender)
	{
		string message = ("Rankeds".Emphasize() + " are out!" + " Join queue".Emphasize() + " with: ").White() +
		                 ".ranked join".Colorify(ExtendedColor.LightServerColor).Emphasize();
		Helper.SendSystemMessageToAllClients(
			message
		);
	}
}
