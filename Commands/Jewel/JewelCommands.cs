using PvpArena.Data;
using ProjectM;
using Bloodstone.API;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ProjectM.Network;
using System.Linq;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Configs;
using PvpArena.Helpers;

namespace PvpArena.Commands;

internal static class JewelCommands
{
	[Command("jewel", description:"Creates a jewel with the mods of your choice. Do .j phantomaegis ? to see the options", usage: ".j phantomaegis 123", aliases: new string[] { "j", "je", "jew", "jewe" }, adminOnly: false, includeInHelp: true, category: "Jewels")]
	public static void JewelCommand(Player sender, string input1, string input2 = "", string input3 = "", string input4 = "", string input5 = "", string input6 = "", string input7 = "")
	{
		string spellName, mods;
		float power;
		ParseInputs(input1, input2, input3, input4, input5, input6, input7, out spellName, out mods, out power);
		if (input1 == "?") 
		{
			sender.ReceiveMessage(("Jewel example:".Colorify(ExtendedColor.ServerColor) + " .j bloodrite 123").Emphasize());
			sender.ReceiveMessage(("To display the list of mods use " + ".j spellName ?".Colorify(ExtendedColor.LightServerColor)).White());
			return;
		}

		if (!Helper.TryGetJewelPrefabDataFromString(spellName.ToLower(), out PrefabData item))
		{
			sender.ReceiveMessage("Couldn't find the ability name.".Error());
			return;
		}
		var properName = item.GetName();
		var condensedName = item.GetName().Replace(" ", "").ToLower();
		if (!Regex.IsMatch(mods, @"^[0-9a-fA-F]{3}$") && mods != "?")
		{
			sender.ReceiveMessage("Mods should be three numbers or A, B, i.e. 123.".Error());
			return;
		}
		if (mods.GroupBy(c => c).Any(g => g.Count() > 1))
		{
			sender.ReceiveMessage("No duplicate mods allowed.".Error());
			return;
		}
		
		SchoolData jewelSchoolData = JewelData.abilityToSchoolDictionary[condensedName];;

		if (mods == "?")
		{
			sender.ReceiveMessage($"Mods for {properName.Colorify(jewelSchoolData.lightColor)}".Colorify(jewelSchoolData.color));
			int i = 1;
			foreach (var kvp in JewelData.SpellMods[condensedName])
			{
				var hexValue = i.ToString("X");
				sender.ReceiveMessage($"{hexValue.Colorify(jewelSchoolData.color)} - {kvp.Value}".White());
				i++;
			}
		}
		else
		{
			var mod1 = Convert.ToInt32(mods[0].ToString(), 16) - 1;
			var mod2 = Convert.ToInt32(mods[1].ToString(), 16) - 1;
			var mod3 = Convert.ToInt32(mods[2].ToString(), 16) - 1;
			if (mod1 < 0 || mod1 >= JewelData.SpellMods[condensedName].Count)
			{
				sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
				return;
			}
			if (mod2 < 0 || mod2 >= JewelData.SpellMods[condensedName].Count)
			{
				sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
				return;
			}
			if (mod3 < 0 || mod3 >= JewelData.SpellMods[condensedName].Count)
			{
				sender.ReceiveMessage("You specified a mod that doesn't exist.".Error());
				return;
			}
			Helper.GenerateJewelViaEvent(sender, condensedName, mods, power);
			sender.ReceiveMessage("Generated jewel!".Success());
		}
	}

	private static void ParseInputs(string input1, string input2, string input3, string input4, string input5, string input6, string input7, out string spellName, out string mods, out float power)
	{
		// Initialize variables
		spellName = "";
		mods = "";
		power = 1;

		// Consolidate and preprocess inputs
		List<string> inputs = new List<string> { input1, input2, input3, input4, input5, input6, input7 };
		inputs = inputs.Select(input => input.Replace(",", "")).ToList(); // Remove commas
		inputs.RemoveAll(string.IsNullOrEmpty);

		// Identify spellName, mods, and power
		bool modsIdentified = false;
		foreach (var input in inputs)
		{
			if (input == "?" || input.EndsWith("?"))
			{
				mods = "?";
				modsIdentified = true;
				spellName += input.TrimEnd('?'); // Add to spellName but remove trailing '?'
				break; // No need to look for numbers if mods is "?"
			}
			else if (int.TryParse(input, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int num))
			{
				mods += num.ToString("X"); // Convert decimal to hexadecimal
				if (mods.Length >= 3)
				{
					mods = mods.Substring(0, 3); // Ensure mods is exactly 3 characters
					modsIdentified = true;
				}
			}
			else if (!modsIdentified)
			{
				spellName += input;
			}
			else if (float.TryParse(input, out float numPower))
			{
				power = numPower;
				break; // Assuming only one power parameter is expected
			}
		}

		// Remove all spaces from spellName
		spellName = spellName.Replace(" ", "");
	}

}
