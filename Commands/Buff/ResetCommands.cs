using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands;

internal static class ResetCommands
{
	[Command(name:"r", description:"Reset cooldown and hp for the player.", usage:".r (or use Blood Mend)", aliases: new string[] { "reset", "res", "r", "К", "к" }, adminOnly: false, includeInHelp: true, category: "Reset CDs & Heal")]
	public static void ResetCommand(Player sender)
	{		
		Helper.Reset(sender);
		// sender.ReceiveMessage($"Player \"{name.Emphasize()}\" reset.".Success());
	}
}
