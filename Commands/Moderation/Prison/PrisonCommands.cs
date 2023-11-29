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
	[Command("prison add", description: "Imprisons a player for a set duration", usage: ".imprison Ash", aliases: new string[] { "imprison" }, adminOnly: true)]
	public void ImprisonCommand(Player sender, Player imprisonedPlayer, int numberOfDays = -1, string reason = "No reason given")
	{
		ImprisonService.ImprisonPlayer(imprisonedPlayer.SteamID, numberOfDays);
		string durationMessage;
		if (numberOfDays == -1)
		{
			durationMessage = "indefinitely";
		}
		else
		{
			durationMessage = $"for {numberOfDays.ToString()} days";
		}
		imprisonedPlayer.ReceiveMessage($"You have been {"imprisoned".Emphasize()} {durationMessage}.".White());
		sender.ReceiveMessage($"{imprisonedPlayer.Name} has been imprisoned {durationMessage}.");
	}

	[Command("prison remove", description: "Unimprisons a player for a set duration", usage: ".prison remove Ash", adminOnly: true)]
	public void ImprisonCommand(Player sender, Player imprisonedPlayer)
	{
		ImprisonService.UnimprisonPlayer(imprisonedPlayer.SteamID);
		imprisonedPlayer.ReceiveMessage($"You have been set free!".White());
		sender.ReceiveMessage($"{imprisonedPlayer.Name} has been unimprisoned.");
	}
}
