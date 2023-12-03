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

public static class DodgeballConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "dodgeball.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static DodgeballConfigData Config { get; private set; } = new DodgeballConfigData();

	static DodgeballConfig()
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
			Config = JsonSerializer.Deserialize<DodgeballConfigData>(jsonData, options) ?? new DodgeballConfigData();
		}
		catch (Exception ex)
		{
			Config = new DodgeballConfigData();
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

public class DodgeballConfigData
{
	public int HitsToKill { get; set; } = 3;
	public int BlocksToRevive { get; set; } = 2;
	public CoordinateDto Team1StartPosition { get; set; } = new CoordinateDto();
	public CoordinateDto Team2StartPosition { get; set; } = new CoordinateDto();
	public RectangleZoneDto Team1Zone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto Team2Zone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto MapCenter { get; set; } = new RectangleZoneDto();
	public List<StructureSpawn> StructureSpawns { get; set; } = new List<StructureSpawn>();

	// Additional properties...
}
