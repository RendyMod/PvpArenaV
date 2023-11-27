using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Shared;
using PvpArena;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Services;
using Unity.Mathematics;
using static PvpArena.Configs.ConfigDtos;

public static class PlayerJewels
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "player_jewels.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
	public static Dictionary<ulong, Dictionary<string, string>> JewelData;

	static PlayerJewels()
	{
		Load();
	}

	public static void Load()
	{
		if (!File.Exists(FullPath))
		{
			ExportJewels(); // Create file with default values
			return;
		}

		try
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
			};
			var jsonData = File.ReadAllText(FullPath);
			JewelData = JsonSerializer.Deserialize<Dictionary<ulong, Dictionary<string, string>>>(jsonData, options) ?? new Dictionary<ulong, Dictionary<string, string>>();
		}
		catch (Exception ex)
		{
			JewelData = new Dictionary<ulong, Dictionary<string, string>>();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void ExportJewels()
	{
		try
		{
			if (JewelData == null)
			{
				JewelData = new Dictionary<ulong, Dictionary<string, string>>();
			}
			
			var spellModEntities = Helper.GetEntitiesByComponentTypes<SpellModSetComponent>(true);
			foreach (var spellModEntity in spellModEntities)
			{
				/*ability.LogComponentTypes();*/
				var spellModSet = spellModEntity.Read<SpellModSetComponent>();
				if (spellModEntity.Has<AbilitySpellModItem>())
				{
					if (spellModEntity.Has<EntityOwner>())
					{
						var player = PlayerService.GetPlayerFromCharacter(spellModEntity.Read<EntityOwner>());
						var abilityGroupState = spellModEntity.Read<AbilityGroupState>();

						var abilityGroupPrefab = abilityGroupState.GroupId;
						if (PvpArena.Data.JewelData.prefabToAbilityNameDictionary.ContainsKey(abilityGroupPrefab))
						{
							var spellName = PvpArena.Data.JewelData.prefabToAbilityNameDictionary[abilityGroupPrefab];

							var equippedMods = new List<PrefabGUID> { spellModSet.SpellMods.Mod0.Id, spellModSet.SpellMods.Mod1.Id, spellModSet.SpellMods.Mod2.Id };
							var modIndices = new StringBuilder();

							foreach (var mod in equippedMods)
							{
								if (PvpArena.Data.JewelData.SpellMods.ContainsKey(spellName))
								{
									int index = PvpArena.Data.JewelData.SpellMods[spellName].FindIndex(pair => pair.Key == mod);
									if (index != -1)
									{
										modIndices.Append(index + 1); // Offset by 1
									}
								}
							}

							var modsString = modIndices.ToString();
							if (modsString.Length == 3)
							{
								if (JewelData.ContainsKey(player.SteamID))
								{
									JewelData[player.SteamID][spellName] = modsString;
								}
								else
								{
									JewelData[player.SteamID] = new Dictionary<string, string> { { spellName, modsString } };
								}
							}
						}
					}
				}
			}

			// Write the JSON string to a file
			
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
			};
			var jsonData = JsonSerializer.Serialize(JewelData, options);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}
}
