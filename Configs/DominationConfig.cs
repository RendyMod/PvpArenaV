using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectM;
using ProjectM.Behaviours;
using PvpArena;
using PvpArena.Configs;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Mathematics;
using static PvpArena.Configs.ConfigDtos;

public static class DominationConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "domination.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static DominationConfigData Config { get; private set; } = new DominationConfigData();

	static DominationConfig()
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
			Config = JsonSerializer.Deserialize<DominationConfigData>(jsonData, options) ?? new DominationConfigData();
		}
		catch (Exception ex)
		{
			Config = new DominationConfigData();
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

public class DominationConfigData
{
	public int PlayerRespawnTime { get; set; } = 6; // Default value
	public string DefaultBlood { get; set; } = "Warrior"; // Default value
	public CoordinateDto Team1PlayerRespawn { get; set; } = new CoordinateDto();
	public CoordinateDto Team2PlayerRespawn { get; set; } = new CoordinateDto();
	public List<CapturePointDto> CapturePoints { get; set; } = new List<CapturePointDto>();
	public RectangleZoneDto MapCenter { get; set; } = new RectangleZoneDto();
	public List<PrefabSpawn> StructureSpawns { get; set; } = new List<PrefabSpawn>();
	public List<UnitSpawn> UnitSpawns { get; set; } = new List<UnitSpawn>();

	// Additional properties...
}
