using System.Text;
using ProjectM;
using ProjectM.Network;
using Bloodstone.API;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Models;
using PvpArena.Helpers;
using PvpArena.Services.Moderation;

namespace PvpArena.Commands.Moderation.Imprison;
internal class PrisonCommands
{
	[Command("prison add", description: "Imprisons a player for a set duration", usage: ".imprison Ash", aliases: new string[] { "imprison", "prison-add" }, adminOnly: true)]
	public void ImprisonCommand(Player sender, Player imprisonedPlayer, int numberOfDays = -1, string reason = "No reason given")
	{
		if (imprisonedPlayer.IsImprisoned())
		{
			sender.ReceiveMessage($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} is already in prison.".Error());
		}
		else
		{
			ImprisonService.ImprisonPlayer(imprisonedPlayer.SteamID, numberOfDays);
			string durationMessage;
			if (numberOfDays == -1)
			{
				durationMessage = "indefinitely".Emphasize();
			}
			else
			{
				durationMessage = $"for {numberOfDays.ToString().Emphasize()} days";
			}
			imprisonedPlayer.ReceiveMessage($"You have been imprisoned {durationMessage}.".Error());
			sender.ReceiveMessage($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been imprisoned {durationMessage}.".White());
		}
	}

	[Command("prison remove", description: "Unimprisons a player for a set duration", usage: ".prison remove Ash", aliases: new string[] { "unimprison", "prison-remove" }, adminOnly: true)]
	public void ImprisonCommand(Player sender, Player imprisonedPlayer)
	{
		if (imprisonedPlayer.IsImprisoned())
		{
			ImprisonService.UnimprisonPlayer(imprisonedPlayer.SteamID);
			imprisonedPlayer.ReceiveMessage($"You have been set free!".Success());
			sender.ReceiveMessage($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been unimprisoned.".White());
		}
		else
		{
			sender.ReceiveMessage($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} can't be freed because they are not in prison.".Error());
		}
	}
		ImprisonService.UnimprisonPlayer(imprisonedPlayer.SteamID);
		Helper.SendSystemMessageToAllClients($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been unimprisoned.".Success());
		imprisonedPlayer.ReceiveMessage($"You have been set free!".Success());
		//sender.ReceiveMessage($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been unimprisoned.".White());
	}
}
