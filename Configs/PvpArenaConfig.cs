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
        public string CurrentRegion { get; set; } = "TEST";
        public bool MatchmakingEnabled { get; set; } = true;
		public bool UseCustomSpawnLocation { get; set; } = false;
		public CoordinateDto CustomSpawnLocation { get; set; } = new CoordinateDto { X = 0, Y = 0, Z = 0 };
		public string DiscordLink { get; set; } = "https://discord.gg";
		public string DefaultArenaBlood { get; set; } = "Warrior"; // Default value
		public bool PointSystemEnabled { get; set; } = false;
		public int PointsPerIntervalOnline { get; set; } = 1; // Default value
		public int IntervalDurationInMinutes { get; set; } = 15; // Default value
		public DatabaseConfig MainDatabase { get; set; } = new DatabaseConfig();
		public DatabaseConfig ServerDatabase { get; set; } = new DatabaseConfig();

		public PvpArenaConfigData()
		{
		}
	}
}
