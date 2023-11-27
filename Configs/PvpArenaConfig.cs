using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Unity.Mathematics;
using System.IO;
using System.Text.Json;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Configs;

public static class PvpArenaConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "settings.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static PvpArenaConfigData Config { get; private set; } = new PvpArenaConfigData();

	static PvpArenaConfig()
	{
		Load();
	}

	public static void Load()
	{
		if (!File.Exists(FullPath))
		{
			Save();  // Create file with default values
			return;
		}

		try
		{
			var jsonData = File.ReadAllText(FullPath);
			Config = JsonSerializer.Deserialize<PvpArenaConfigData>(jsonData) ?? new PvpArenaConfigData();
		}
		catch (Exception ex)
		{
			Config = new PvpArenaConfigData();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void Save()
	{
		try
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			var jsonData = JsonSerializer.Serialize(Config, options);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}



	public class PvpArenaConfigData
	{
		public bool MatchmakingEnabled { get; set; } = true;
		public bool UseCustomSpawnLocation { get; set; } = false;
		public CoordinateDto CustomSpawnLocation { get; set; } = new CoordinateDto { X = 0, Y = 0, Z = 0 };

		public float MaxJewelPower { get; set; } = 1;
		public float MaxLegendaryPower { get; set; } = 1;

		public string DiscordLink { get; set; } = "https://discord.gg";
		public Dictionary<string, WeaponConfigDto> DefaultLegendaries { get; set; } = new Dictionary<string, WeaponConfigDto>();
		public Dictionary<string, JewelConfigDto> DefaultJewels { get; set; } = new Dictionary<string, JewelConfigDto>();
		public string DefaultArenaBlood { get; set; } = "Warrior"; // Default value
		public bool PointSystemEnabled { get; set; } = false;
		public int PointsPerIntervalOnline { get; set; } = 1; // Default value
		public int IntervalDurationInMinutes { get; set; } = 15; // Default value
		public DatabaseConfig Database { get; set; } = new DatabaseConfig();

		public PvpArenaConfigData()
		{
			InitializeDefaultLegendaries();
			InitializeDefaultJewels();
		}


		public List<ArenaLocationDto> MatchmakingArenaLocations { get; set; } = new List<ArenaLocationDto>
		{
			new ArenaLocationDto
			{
				Location1 = new CoordinateDto { X = -1147.0f, Y = 10.31f, Z = -669 },
				Location2 = new CoordinateDto { X = -1147.0f, Y = 10.31f, Z = -669 }
			}
		};

		private void InitializeDefaultLegendaries()
		{
			if (DefaultLegendaries.Count == 0)
			{
				var defaultWeapon = new WeaponConfigDto();
				DefaultLegendaries["Slashers"] = defaultWeapon;
				DefaultLegendaries["Spear"] = defaultWeapon;
				DefaultLegendaries["Axes"] = defaultWeapon;
				DefaultLegendaries["GreatSword"] = defaultWeapon;
				DefaultLegendaries["Crossbow"] = defaultWeapon;
				DefaultLegendaries["Pistols"] = defaultWeapon;
				DefaultLegendaries["Reaper"] = defaultWeapon;
				DefaultLegendaries["Sword"] = defaultWeapon;
				DefaultLegendaries["Mace"] = defaultWeapon;
			}
		}
		private void InitializeDefaultJewels()
		{
			if (DefaultJewels.Count == 0)
			{
				var defaultJewel = new JewelConfigDto();
				DefaultJewels["bloodfountain"] = defaultJewel;
				DefaultJewels["bloodrage"] = defaultJewel;
				DefaultJewels["bloodrite"] = defaultJewel;
				DefaultJewels["sanguinecoil"] = defaultJewel;
				DefaultJewels["shadowbolt"] = defaultJewel;
				DefaultJewels["aftershock"] = defaultJewel;
				DefaultJewels["chaosbarrier"] = defaultJewel;
				DefaultJewels["powersurge"] = defaultJewel;
				DefaultJewels["void"] = defaultJewel;
				DefaultJewels["chaosvolley"] = defaultJewel;
				DefaultJewels["crystallance"] = defaultJewel;
				DefaultJewels["frostbat"] = defaultJewel;
				DefaultJewels["iceblock"] = defaultJewel;
				DefaultJewels["icenova"] = defaultJewel;
				DefaultJewels["frostbarrier"] = defaultJewel;
				DefaultJewels["misttrance"] = defaultJewel;
				DefaultJewels["mosquito"] = defaultJewel;
				DefaultJewels["phantomaegis"] = defaultJewel;
				DefaultJewels["spectralwolf"] = defaultJewel;
				DefaultJewels["wraithspear"] = defaultJewel;
				DefaultJewels["balllightning"] = defaultJewel;
				DefaultJewels["cyclone"] = defaultJewel;
				DefaultJewels["discharge"] = defaultJewel;
				DefaultJewels["lightningcurtain"] = defaultJewel;
				DefaultJewels["polarityshift"] = defaultJewel;
				DefaultJewels["boneexplosion"] = defaultJewel;
				DefaultJewels["corruptedskull"] = defaultJewel;
				DefaultJewels["deathknight"] = defaultJewel;
				DefaultJewels["soulburn"] = defaultJewel;
				DefaultJewels["wardofthedamned"] = defaultJewel;
				DefaultJewels["veilofblood"] = defaultJewel;
				DefaultJewels["veilofbones"] = defaultJewel;
				DefaultJewels["veilofchaos"] = defaultJewel;
				DefaultJewels["veiloffrost"] = defaultJewel;
				DefaultJewels["veilofillusion"] = defaultJewel;
				DefaultJewels["veilofstorm"] = defaultJewel;
			}
		}
	}
}
