using PvpArena.Data;
using PvpArena.Services;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;

namespace PvpArena.Commands;

internal static class BloodCommands
{
	[Command("bp", usage: ".bp warrior", description: "Sets your blood", aliases: new string[] {"blood", "b", "blod", "bloodpotion"}, adminOnly: false, includeInHelp: true, category:"Blood Potions")]
	public static void SetBloodCommand(Player sender, BloodPrefabData bloodType, float quality = 100f)
	{
		Helper.SetPlayerBlood(sender, bloodType.PrefabGUID, quality);
	}
}
