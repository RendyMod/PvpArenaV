using System.Text;
using ProjectM;
using ProjectM.Network;
using Bloodstone.API;
using Discord;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Models;
using PvpArena.Helpers;
using PvpArena.Services.Moderation;
using PvpArena.GameModes.Matchmaking1v1;

namespace PvpArena.Commands.Moderation.Imprison;

internal class PrisonCommands
{
	[Command("prison add", description: "Imprisons a player for a set duration", usage: ".imprison Ash",
		aliases: new string[] { "imprison", "prison-add" }, adminOnly: true)]
	public void ImprisonCommand (Player sender, Player imprisonedPlayer, int numberOfDays = -1,
		string reason = "No reason given.")
	{
		if (imprisonedPlayer.IsImprisoned())
		{
			sender.ReceiveMessage($"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} is already in prison."
				.Error());
		}
		else
		{
			if (imprisonedPlayer.IsIn1v1()) //do we need to do anything for other game modes here?
			{
				MatchmakingQueue.MatchManager.EndMatch(MatchmakingHelper.GetOpponentForPlayer(imprisonedPlayer),
					imprisonedPlayer, false);
			}

			ImprisonService.ImprisonPlayer(imprisonedPlayer.SteamID, numberOfDays);
			string durationMessage;
			if (numberOfDays == -1)
			{
				durationMessage = "indefinitely";
			}
			else
			{
				durationMessage = $"for {numberOfDays} day(s)";
			}

			Helper.SendSystemMessageToAllClients(
				$"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been imprisoned {durationMessage}."
					.Error());
			imprisonedPlayer.ReceiveMessage($"You have been imprisoned {durationMessage}.".Error());
			sender.ReceiveMessage(
				$"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been imprisoned {durationMessage}."
					.White());

			DiscordBot.SendEmbedAsync(DiscordBotConfig.Config.JailChannel,
				EmbedPrisonAnnouncement(imprisonedPlayer, true, numberOfDays, reason));
		}
	}

	[Command("prison remove", description: "Unimprisons a player for a set duration", usage: ".prison remove Ash",
		aliases: new string[] { "unimprison", "prison-remove" }, adminOnly: true)]
	public void ImprisonCommand (Player sender, Player imprisonedPlayer)
	{
		if (imprisonedPlayer.IsImprisoned())
		{
			ImprisonService.UnimprisonPlayer(imprisonedPlayer.SteamID);
			Helper.SendSystemMessageToAllClients(
				$"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been unimprisoned.".Success());
			imprisonedPlayer.ReceiveMessage($"You have been set free!".Success());
			sender.ReceiveMessage(
				$"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} has been unimprisoned.".White());

			DiscordBot.SendEmbedAsync(DiscordBotConfig.Config.JailChannel, EmbedPrisonAnnouncement(imprisonedPlayer, false));
		}
		else
		{
			sender.ReceiveMessage(
				$"{imprisonedPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} can't be freed because they are not in prison."
					.Error());
		}
	}

	public static Embed EmbedPrisonAnnouncement (Player _player, bool imprisoned = true, int numberOfDays = -1,
		string reason = "")
	{
		var embedBuilder = new EmbedBuilder	
		{
			Title = "\ud83d\udc6e\u2502Player " + (imprisoned ? "imprisoned!" : "released!"),
			Description = "**" + _player.Name + "**" +
			              (imprisoned ? " has been imprisoned." : " has been released from prison."),
			Color = imprisoned ? Color.Red : Color.Green,
		};

		// Adding fields with titles and text
		if (imprisoned)
		{
			embedBuilder.AddField("SteamID", _player.SteamID);
			embedBuilder.AddField("Duration", numberOfDays == -1 ? "Indefinitely." : numberOfDays + " days.");
			embedBuilder.AddField("Reason", reason == "" ? "No reason given." : reason);
		}

		return embedBuilder.Build();
	}
}
