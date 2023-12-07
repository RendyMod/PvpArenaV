using System;
using System.IO;
using System.Text.Json;
using PvpArena;

public static class DiscordBotConfig
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "discord_bot.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);

	public static DiscordBotConfigData Config { get; private set; } = new DiscordBotConfigData();

	static DiscordBotConfig()
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
			};
			var jsonData = File.ReadAllText(FullPath);
			Config = JsonSerializer.Deserialize<DiscordBotConfigData>(jsonData, options) ?? new DiscordBotConfigData();
		}
		catch (Exception ex)
		{
			Config = new DiscordBotConfigData();
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
			};
			var jsonData = JsonSerializer.Serialize(Config, options);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
		}
	}
}

public class DiscordBotConfigData
{
	public string Token { get; set; } = "";
	public ulong JailChannel { get; set; } = 0;
	public ulong RecordChannel { get; set; } = 0;
	public ulong GlobalChannel { get; set; } = 0;
}
