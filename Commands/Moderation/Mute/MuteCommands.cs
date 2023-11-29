using ProjectM;
using ProjectM.Network;
using Bloodstone.API;
using PvpArena.Services;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;
using PvpArena.Services.Moderation;
using PvpArena;

internal class MuteCommands
{
	public static int FOREVER = -1;
	[Command("mute", description: "Mutes a player", adminOnly: true)]
	public void MuteCommand(Player sender, Player foundPlayer, int numberOfDays = -1, string reason = "No reason given")
	{
		var player = foundPlayer;
		
		MuteService.MutePlayer(player, numberOfDays);
		player.Character.Remove<CharacterVoiceActivity>();

		/*Core.discordBot.SendMessageAsync(player.Name + " " + Moderation.BanMuteMessage(false, true, player.Name, player.SteamID.ToString(), numberOfDays, reason));*/
		sender.ReceiveMessage(("You ".Emphasize() + Moderation.BanMuteMessage(false, true, player.Name.Emphasize(), player.SteamID.ToString().Emphasize(), numberOfDays, reason.Emphasize()).White()));
		player.ReceiveMessage(Moderation.BanMuteMessage(true, true, player.Name, player.SteamID.ToString(), numberOfDays, reason.Emphasize()).White());
		
		var action = new ScheduledAction(Helper.KickPlayer, new object[] { player.SteamID });
		ActionScheduler.ScheduleAction(action, 10);
	}

	[Command("unmute", description: "Mutes a player", adminOnly: true)]
	public void UnmuteCommand(Player sender, Player player)
	{
		MuteService.UnmutePlayer(player.SteamID);
		player.Character.Add<CharacterVoiceActivity>();
		
		if (!player.MuteInfo.IsMuted())
		{
			sender.ReceiveMessage($"{player.Name} is not muted!".Error());
			return;
		}

		player.ReceiveMessage("You have been unmuted.".Success() + " Unmuting requires a temporary kick from the server, but you can rejoin!".White());
		sender.ReceiveMessage($"You unmuted {player.Name.Emphasize()}. (Unmuting requires a rejoin)".White());
		/*Core.discordBot.SendMessageAsync(sender.Name + $" unmuted {player.Name}.");*/
		var action = new ScheduledAction(Helper.KickPlayer, new object[] { player.SteamID });
		ActionScheduler.ScheduleAction(action, 10);
	}
}
