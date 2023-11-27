using ProjectM.Network;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands;
internal static class PingCommands
{ 
	[Command("ping", description:"Shows your latency", usage: ".ping", aliases: new string[] { "p" }, adminOnly: false, includeInHelp: true, category: "Misc")]
	public static void PingCommand(Player sender, string mode = "")
	{
		var ping = (int)(sender.Character.Read<Latency>().Value * 1000);
		sender.ReceiveMessage($"Your latency is {ping.ToString().Emphasize()}ms.".White());
	}
}
