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

public static class BulletHellConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "bullet_hell.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static BulletHellConfigData Config { get; private set; } = new BulletHellConfigData();

	static BulletHellConfig()
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
			Config = JsonSerializer.Deserialize<BulletHellConfigData>(jsonData, options) ?? new BulletHellConfigData();
		}
		catch (Exception ex)
		{
			Config = new BulletHellConfigData();
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

public class BulletHellConfigData
{
	public string DefaultBlood { get; set; } = "Warrior"; // Default value
	public CoordinateDto RespawnPoint { get; set; } = new CoordinateDto();
	public CoordinateDto StartPosition { get; set; } = new CoordinateDto();
	public List<UnitSpawn> UnitSpawns { get; set; } = new List<UnitSpawn>();
	public CircleZoneDto FightZone { get; set; } = new CircleZoneDto();

	// Additional properties...
}
