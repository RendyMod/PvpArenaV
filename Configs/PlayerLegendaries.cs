using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bloodstone.API;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Shared;
using PvpArena;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Services;
using Unity.Mathematics;
using static PvpArena.Configs.ConfigDtos;

public static class PlayerLegendaries
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "player_legendaries.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
	public static Dictionary<ulong, List<LegendaryDto>> LegendaryWeaponsData;

	static PlayerLegendaries()
	{
		Load();
	}

	public static void Load()
	{
		if (!File.Exists(FullPath))
		{
			ExportLegendaries(); // Create file with default values
			return;
		}

		try
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
			};
			var jsonData = File.ReadAllText(FullPath);
			LegendaryWeaponsData = JsonSerializer.Deserialize<Dictionary<ulong, List<LegendaryDto>>>(jsonData, options) ?? new Dictionary<ulong, List<LegendaryDto>>();
		}
		catch (Exception ex)
		{
			LegendaryWeaponsData = new Dictionary<ulong, List<LegendaryDto>>();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void ExportLegendaries()
	{
		try
		{
			if (LegendaryWeaponsData == null)
			{
				LegendaryWeaponsData = new Dictionary<ulong, List<LegendaryDto>>();
			}
			LegendaryWeaponsData.Clear();
			var players = PlayerService.UserCache.Values;
			foreach (var player in players)
			{
				for (var i = 0; i < 36; i++)
				{
					if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, i, out InventoryBuffer item))
					{
						if (item.ItemEntity._Entity.Has<LegendaryItemSpellModSetComponent>())
						{
							var legendary = item.ItemEntity._Entity;
							var legendaryModData = legendary.Read<LegendaryItemSpellModSetComponent>();
							var weaponType = legendary.Read<PrefabGUID>();
							var infusionPrefab = legendaryModData.AbilityMods0.Mod0.Id;
							var mod1 = LegendaryData.statModGuidToIndex[legendaryModData.StatMods.Mod0.Id];
							var mod2 = LegendaryData.statModGuidToIndex[legendaryModData.StatMods.Mod1.Id];
							var mod3 = LegendaryData.statModGuidToIndex[legendaryModData.StatMods.Mod2.Id];
							var legendaryConfigDto = new LegendaryDto
							{
								Infusion = LegendaryData.prefabToInfusionDictionary[infusionPrefab],
								WeaponName = LegendaryData.prefabToWeaponDictionary[weaponType],
								Mods = $"{mod1}{mod2}{mod3}",
								Slot = i
							};
						
							if (LegendaryWeaponsData.ContainsKey(player.SteamID))
							{
								LegendaryWeaponsData[player.SteamID].Add(legendaryConfigDto);
							}
							else
							{
								LegendaryWeaponsData[player.SteamID] = new List<LegendaryDto> { legendaryConfigDto };
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
			var jsonData = JsonSerializer.Serialize(LegendaryWeaponsData, options);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}
}
