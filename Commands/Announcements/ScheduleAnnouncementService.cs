using System.Threading;
using Bloodstone.API;
using ProjectM;
using PvpArena.Configs;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;

public static class ScheduleAnnouncementService
{
	private static Timer m_discordTimer;
	private static Timer m_shopTimer;
	private static Timer m_bulletTimer;
	public static void Initialize()
	{
		m_discordTimer = ActionScheduler.RunActionOnceAfterDelay(StartDiscordAnnouncement, 120 * 60);
		m_shopTimer = ActionScheduler.RunActionOnceAfterDelay(StartShopAnnouncement, 60 * 60);
		m_bulletTimer = ActionScheduler.RunActionOnceAfterDelay(StartBulletAnnouncement, 180 * 60);
	}

	private static void StartDiscordAnnouncement ()
	{
		m_discordTimer = ActionScheduler.RunActionEveryInterval(SendDiscordAnnouncementMessage, 180 * 60);
	}
	
	private static void StartShopAnnouncement ()
	{
		m_shopTimer = ActionScheduler.RunActionEveryInterval(SendShopAnnouncementMessage, 180 * 60);
	}
	
	private static void StartBulletAnnouncement ()
	{
		m_bulletTimer = ActionScheduler.RunActionEveryInterval(SendBulletAnnouncementMessage, 180 * 60);
	}
	
	public static void Dispose()
	{
		m_discordTimer?.Dispose();
		m_discordTimer = null;
		
		m_shopTimer?.Dispose();
		m_shopTimer = null;
		
		m_bulletTimer?.Dispose();
		m_bulletTimer = null;
	}
	
	[CommandFramework.Command("globaldiscord", description: "Give the link of the discord to everyone",
		usage: ".globaldiscord", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalDiscordCommand (Player sender)
	{
		SendDiscordAnnouncementMessage();
	}

	public static void SendDiscordAnnouncementMessage ()
	{
		string message = ("Join us".Emphasize() + " on "+"Discord".Emphasize()+": ").White() +
		                 $"{PvpArenaConfig.Config.DiscordLink}".Colorify(ExtendedColor.LightServerColor).Emphasize();

		Helper.SendSystemMessageToAllClients(
			message
		);
	}

	[CommandFramework.Command("globalranked", description: "Send a message to everyone for ranked",
		usage: ".globalranked", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalRankedHelpCommand (Player sender)
	{
		SendRankedAnnouncementMessage();
	}
	
	public static void SendRankedAnnouncementMessage ()
	{
		string message1 = ("Ranked Mode".Emphasize() + " is now available!").White();
		
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
		SendBulletAnnouncementMessage();
	}

	public static void SendBulletAnnouncementMessage ()
	{
		string message = ("Bullet Hell Mode".Emphasize() + " is available!" ).White();
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
	
	[CommandFramework.Command("globalshop", description: "Send a message to everyone for shop",
		usage: ".globalshop", adminOnly: true, includeInHelp: false, category: "Misc")]
	public static void GlobalShopCommand (Player sender)
	{
		SendShopAnnouncementMessage();
	}

	public static void SendShopAnnouncementMessage ()
	{
		string message = ("Shop".Emphasize() + " is available!" + " Spend your "+"VPoints".Warning()+" there!").White();
		string message2 = ("Check it out with: " +
		                   ".tp shop".Colorify(ExtendedColor.LightServerColor).Emphasize() + " or " + ".tp 5".Colorify(ExtendedColor.LightServerColor).Emphasize()).White();
		string message3 = ("Check out your balance with: " +
		                   ".points".Colorify(ExtendedColor.LightServerColor)).White();

		Helper.SendSystemMessageToAllClients(
			message
		);
		Helper.SendSystemMessageToAllClients(
			message2
		);
		Helper.SendSystemMessageToAllClients(
			message3
		);
	}
}
