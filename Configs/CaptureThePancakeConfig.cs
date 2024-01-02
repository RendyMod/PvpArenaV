using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectM;
using ProjectM.Behaviours;
using PvpArena;
using PvpArena.Helpers;
using Unity.Mathematics;
using static PvpArena.Configs.ConfigDtos;

public static class CaptureThePancakeConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "capture_the_pancake.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static CaptureThePancakeConfigData Config { get; private set; } = new CaptureThePancakeConfigData();

	static CaptureThePancakeConfig()
	{
		Load();
	}

	public static void Load()
	{
		if (!File.Exists(FullPath))
		{
			Save(); // Create file with default values
			return;
		}

		try
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new PrefabGUIDConverter() }
			};
			var jsonData = File.ReadAllText(FullPath);
			Config = JsonSerializer.Deserialize<CaptureThePancakeConfigData>(jsonData, options) ?? new CaptureThePancakeConfigData();
		}
		catch (Exception ex)
		{
			Config = new CaptureThePancakeConfigData();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void Save()
	{
		try
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new PrefabGUIDConverter() }
			};
			var jsonData = JsonSerializer.Serialize(Config, options);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
		}
	}
}

public class CaptureThePancakeConfigData
{
	public float ShardSpeed { get; set; }
	public float HorseSpeed { get; set; }
	public float PvpTimerDuration { get; set; } = 15;
	public int SecondsBeforeMatchScalingStops { get; set; }
	public int PlayerStartingRespawnTime { get; set; } = 6; // Default value
	public int PlayerMaxRespawnTime { get; set; } = 30; // Default value
	public string DefaultBlood { get; set; } = "Warrior"; // Default value
    public List<CaptureThePancakeArenaDto> Arenas { get; set; } = new();

	// Additional properties...
}
