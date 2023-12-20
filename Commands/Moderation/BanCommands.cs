using System.Text;
using ProjectM;
using ProjectM.Network;
using Bloodstone.API;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Models;
using PvpArena.Helpers;
using PvpArena.Services.Moderation;

internal class BanCommands
{
	public static int FOREVER = -1;
	
	[Command("ban", description: "Bans a player for the specified number of days", adminOnly: true)]
	public void BanCommand(Player sender, Player player, int numberOfDays = -1, string reason = "No reason given")
	{
		var User = player.User;
		var Character = player.Character;
		
		BanService.BanPlayer(player.SteamID, numberOfDays);

		/*Core.discordBot.SendMessageAsync(sender.Name + " " + Moderation.BanMuteMessage(false, false, player.Name, player.SteamID.ToString(), numberOfDays, reason));*/
		sender.ReceiveMessage(("You ".Emphasize() + Moderation.BanMuteMessage(false, false, player.Name.Emphasize(), player.SteamID.ToString().Emphasize(), numberOfDays, reason.Emphasize()).White()));
		player.ReceiveMessage(Moderation.BanMuteMessage(true, false, player.Name, player.SteamID.ToString(), numberOfDays, reason.Emphasize()).White());

		var action = () => Helper.KickPlayer(player.SteamID);
		ActionScheduler.RunActionOnceAfterFrames(action, 10);
	}

	[Command("unban", description: "Unbans a player", adminOnly: true)]
	public void UnbanCommand(Player sender, Player player)
	{
		if (!player.BanInfo.IsBanned())
		{
			sender.ReceiveMessage($"{player.Name} is not banned!".Error());
			return;
		}

		BanService.UnbanPlayer(player.SteamID);
		sender.ReceiveMessage($"You unbanned {player.Name.Emphasize()}.".White());
		/*Core.discordBot.SendMessageAsync(sender.Name + $" unbanned {player.Name}.");*/
	}
}
