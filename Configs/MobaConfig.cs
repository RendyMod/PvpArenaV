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

public static class MobaConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "moba.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static MobaConfigData Config { get; private set; } = new MobaConfigData();

	static MobaConfig()
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
			Config = JsonSerializer.Deserialize<MobaConfigData>(jsonData, options) ?? new MobaConfigData();
		}
		catch (Exception ex)
		{
			Config = new MobaConfigData();
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

public class MobaConfigData
{
	public int SecondsBeforeMatchScalingStops { get; set; }
	public int PlayerStartingRespawnTime { get; set; } = 6; // Default value
	public int PlayerMaxRespawnTime { get; set; } = 30; // Default value
	public string DefaultBlood { get; set; } = "Warrior"; // Default value
	public CoordinateDto Team1PlayerRespawn { get; set; } = new CoordinateDto();
	public CoordinateDto Team2PlayerRespawn { get; set; } = new CoordinateDto();
	public RectangleZoneDto Team1EndZone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto Team2EndZone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto MapCenter { get; set; } = new RectangleZoneDto();
	public List<PrefabSpawn> StructureSpawns { get; set; } = new List<PrefabSpawn>();
	public List<UnitSpawn> UnitSpawns { get; set; } = new List<UnitSpawn>();
	public List<PatrolSpawn> PatrolSpawns { get; set; } = new List<PatrolSpawn>();
	public int CoinsLostPerDeath { get; set; } = 5;
	public int CoinsGainedPerPlayerKill { get; set; } = 10;
	public int CoinsGainedPerUnitKill { get; set; } = 2;

	// Additional properties...
}
