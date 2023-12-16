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
	public int GolemShield { get; set; } = 3750;
	public int PlayerStartingRespawnTime { get; set; } = 6;
	public int PlayerMaxRespawnTime { get; set; } = 30;
	public string DefaultBloodType { get; set; } = "Warrior"; 
	public int DefaultBloodQuality { get; set; } = 0; 
	public CoordinateDto Team1Heart { get; set; } = new CoordinateDto();
	public CoordinateDto Team2Heart { get; set; } = new CoordinateDto();
	public CoordinateDto Team1MerchantPosition { get; set; } = new CoordinateDto();
	public CoordinateDto Team2MerchantPosition { get; set; } = new CoordinateDto();
	public CoordinateDto Team1PlayerRespawn { get; set; } = new CoordinateDto();
	public CoordinateDto Team2PlayerRespawn { get; set; } = new CoordinateDto();
	public CoordinateDto Team1MercenaryStartPosition { get; set; } = new CoordinateDto();
	public CoordinateDto Team2MercenaryStartPosition { get; set; } = new CoordinateDto();
	public RectangleZoneDto Team1HealZone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto Team2HealZone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto MapCenter { get; set; } = new RectangleZoneDto();
	public List<TraderDto> Traders { get; set; } = new();
	public List<PrefabSpawn> StructureSpawns { get; set; } = new List<PrefabSpawn>();
	public List<UnitSpawn> UnitSpawns { get; set; } = new List<UnitSpawn>();
	public List<MercenaryCamp> MercenaryCamps { get; set; } = new();
	public List<TemplateUnitSpawn> PatrolSpawns { get; set; } = new List<TemplateUnitSpawn>();
	public int CoinsLostPerDeath { get; set; } = 5;
	public int CoinsGainedPerPlayerKill { get; set; } = 10;
	public int CoinsGainedPerUnitKill { get; set; } = 2;
	public int CoinsGainedPerTowerKill { get; set; } = 10;
	public int CoinsGainedPerFortKill { get; set; } = 15;
	public int CoinsGainedPerKeepKill { get; set; } = 15;
	public int CoinsGainedPassivelyPerMinute { get; set; } = 5;
	// Additional properties...
}
