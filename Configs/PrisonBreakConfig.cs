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

public static class PrisonBreakConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "prison_break.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static PrisonBreakConfigData Config { get; private set; } = new PrisonBreakConfigData();

	static PrisonBreakConfig()
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
			Config = JsonSerializer.Deserialize<PrisonBreakConfigData>(jsonData, options) ?? new PrisonBreakConfigData();
		}
		catch (Exception ex)
		{
			Config = new PrisonBreakConfigData();
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

public class PrisonBreakConfigData
{
	public string DefaultBlood { get; set; } = "Frailed"; // Default value
	public RectangleZoneDto PrisonZone { get; set; } = new RectangleZoneDto();
	public RectangleZoneDto MapCenter { get; set; } = new RectangleZoneDto();

	// Additional properties...
}
