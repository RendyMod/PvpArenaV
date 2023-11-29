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

public static class PrisonConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "prison_config.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static PrisonConfigData Config { get; private set; } = new PrisonConfigData();

	static PrisonConfig()
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
			Config = JsonSerializer.Deserialize<PrisonConfigData>(jsonData, options) ?? new PrisonConfigData();
		}
		catch (Exception ex)
		{
			Config = new PrisonConfigData();
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

public class PrisonConfigData
{
	public List<CoordinateDto> CellCoordinateList { get; set; } = new List<CoordinateDto>();
}
