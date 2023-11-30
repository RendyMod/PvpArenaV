using PvpArena.Data;
using System;
using System.Text.RegularExpressions;
using ProjectM.Network;
using System.Linq;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Models;
using PvpArena.Configs;
using PvpArena.Helpers;

namespace PvpArena.Commands;

internal static class LegendaryCommands
{
	[Command("legendary", description: "Gives you a custom legendary. Do .lw ? to see the mod options", usage: ".lw spear storm 123", aliases: new string[] { "lw", "leg", "lego", "l" }, adminOnly: false, includeInHelp: true, category: "Legendaries")]
	public static void LegendaryCommand(Player sender, string weaponName, string infusion = "", string mods = "", float power = 1)
	{
		//".chunguslegendary spear static 123"
		if (weaponName == "?")
		{
			sender.ReceiveMessage(("Legendary example:".Colorify(ExtendedColor.ServerColor) + " .lw spear storm 123").Emphasize());
			sender.ReceiveMessage(("List of mods:".Colorify(ExtendedColor.LightServerColor)).Emphasize());
			var i = 1;
			foreach (var description in LegendaryData.statModDescriptions)
			{
				// Convert the index to a hexadecimal string
				var hexValue = i.ToString("X");
				sender.ReceiveMessage($"{hexValue.Colorify(ExtendedColor.ServerColor)} - {description}".White());
				i++;
			}
		}
		else
		{
			if (!LegendaryData.weaponToPrefabDictionary.TryGetValue(weaponName.ToLower(), out var weaponPrefabGUID))
			{
				sender.ReceiveMessage("Invalid weapon name.".Error());
				return;
			}
			if (!LegendaryData.infusionToPrefabDictionary.TryGetValue(infusion.ToLower(), out var infusionPrefabGUID))
			{
				sender.ReceiveMessage("Invalid infusion name.".Error());
				return;
			}
			// Updated regex to match hexadecimal values
			if (!Regex.IsMatch(mods, @"^[0-9a-fA-F]{3}$") && mods != "?")
			{
				sender.ReceiveMessage("Invalid mods - should be three characters, i.e. 12A.".Error());
				return;
			}
			if (mods.GroupBy(c => c).Any(g => g.Count() > 1))
			{
				sender.ReceiveMessage("Invalid mods - no duplicate mods allowed!".Error());
				return;
			}

			var mod1 = Convert.ToInt32(mods[0].ToString(), 16) - 1;
			var mod2 = Convert.ToInt32(mods[1].ToString(), 16) - 1;
			var mod3 = Convert.ToInt32(mods[2].ToString(), 16) - 1;
			if (mod1 < 0 || mod1 > LegendaryData.statMods.Count)
			{
				sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
				return;
			}
			if (mod2 < 0 || mod2 > LegendaryData.statMods.Count)
			{
				sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
				return;
			}
			if (mod3 < 0 || mod3 > LegendaryData.statMods.Count)
			{
				sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
				return;
			}

			Helper.GenerateLegendaryViaEvent(sender, weaponName.ToLower(), infusion.ToLower(), mods, power);
			sender.ReceiveMessage("Legendary created!".Success());
		}
	}
}
