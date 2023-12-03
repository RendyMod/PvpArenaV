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

	[CommandFramework.Command("globalranked", description: "Send a message to everyone for ranked",
		usage: ".globalranked", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalRankedHelpCommand (Player sender)
	{
		string message1 = ("Rankeds".Emphasize() + " are now available!").White();
		
		string message2 = ("Join it".Emphasize() + " with: " +
		                   ".ranked join".Colorify(ExtendedColor.LightServerColor).Emphasize() + ". Leaderboard: " +
		                   ".lb ranked".Colorify(ExtendedColor.LightServerColor).Emphasize()).White();
		Helper.SendSystemMessageToAllClients(
			message1
		);
		Helper.SendSystemMessageToAllClients(
			message2
		);
	}

	[CommandFramework.Command("globalbullet", description: "Send a message to everyone for bullet",
		usage: ".globalbullet", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalBulletHelpCommand (Player sender)
	{
		string message = ("Bullet Hell".Emphasize() + " mini " + "game" +" is out!" ).White();
		string message2 = ("Join it".Emphasize() + " with: " +
		                  ".start-bullet".Colorify(ExtendedColor.LightServerColor).Emphasize() + ". Leaderboard: " +
		                  ".lb bullet".Colorify(ExtendedColor.LightServerColor).Emphasize()).White();

		Helper.SendSystemMessageToAllClients(
			message
		);
		Helper.SendSystemMessageToAllClients(
			message2
		);
	}
}
