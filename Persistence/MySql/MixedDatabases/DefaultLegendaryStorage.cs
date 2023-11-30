using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using ProjectM.Scripting;
using PvpArena.Data;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql.AllDatabases;
public class DefaultLegendaryWeaponDataStorage
{
	private readonly string mainDatabaseConnectionString;
	private readonly string serverDatabaseConnectionString;

	private Dictionary<ulong, List<LegendaryDto>> _playerLegendaryWeapons;
	private List<LegendaryDto> _defaultLegendaryWeapons;

	public DefaultLegendaryWeaponDataStorage(DatabaseConfig mainDatabaseConfig, DatabaseConfig serverDatabaseConfig)
	{
		mainDatabaseConnectionString = BuildConnectionString(mainDatabaseConfig);
		serverDatabaseConnectionString = BuildConnectionString(serverDatabaseConfig);
	}

	private static string BuildConnectionString(DatabaseConfig dbConfig)
	{
		return $"Server={dbConfig.Server};" +
			   $"Port={dbConfig.Port};" +
			   $"Database={dbConfig.Name};" +
			   $"Uid={dbConfig.UserId};" +
			   $"Pwd={dbConfig.Password};";
	}

	public async Task LoadAllLegendaryDataAsync()
	{
		// Load default legendaries for all players
		_defaultLegendaryWeapons = await LoadDefaultLegendariesAsync();

		// Load player-specific legendaries for each player
		// This assumes you have a way to get a list of all player SteamIDs
		_playerLegendaryWeapons = new Dictionary<ulong, List<LegendaryDto>>();
		var playerSteamIds = PlayerService.SteamIdCache.Keys; // Implement this method based on your system
		foreach (var steamId in playerSteamIds)
		{
			_playerLegendaryWeapons[steamId] = await LoadPlayerSpecificLegendariesAsync(steamId);
		}
	}

	public List<LegendaryDto> GetLegendaryWeaponsForPlayer(ulong steamId)
	{
		// Ensure the dictionaries are not null
		_playerLegendaryWeapons ??= new Dictionary<ulong, List<LegendaryDto>>();
		_defaultLegendaryWeapons ??= new List<LegendaryDto>();

		// Check if the player has specific legendaries
		if (_playerLegendaryWeapons.TryGetValue(steamId, out var playerLegendaries) && playerLegendaries.Any())
		{
			return playerLegendaries;
		}

		// If not, return the default legendaries
		return _defaultLegendaryWeapons;
	}

	private async Task<List<LegendaryDto>> LoadPlayerSpecificLegendariesAsync(ulong steamId)
	{
		var legendaries = new List<LegendaryDto>();

		var query = @"
        SELECT WeaponName, Infusion, Mods, Slot
        FROM DefaultLegendaries
        WHERE SteamID = @SteamID;";

		using (var connection = new MySqlConnection(serverDatabaseConnectionString))
		{
			await connection.OpenAsync();

			using (var command = new MySqlCommand(query, connection))
			{
				command.Parameters.AddWithValue("@SteamID", steamId);

				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var legendary = new LegendaryDto
						{
							WeaponName = reader.GetString("WeaponName"),
							Infusion = reader.IsDBNull(reader.GetOrdinal("Infusion")) ? null : reader.GetString("Infusion"),
							Mods = reader.IsDBNull(reader.GetOrdinal("Mods")) ? null : reader.GetString("Mods"),
							Slot = reader.GetInt32("Slot")
						};

						legendaries.Add(legendary);
					}
				}
			}
		}

		return legendaries;
	}

	// Load default legendaries
	private async Task<List<LegendaryDto>> LoadDefaultLegendariesAsync()
	{

		var legendaries = new List<LegendaryDto>();

		var query = @"
        SELECT WeaponName, Infusion, Mods
        FROM DefaultLegendaries;";

		try
		{
			using (var connection = new MySqlConnection(mainDatabaseConnectionString))
			{
				await connection.OpenAsync();

				using (var command = new MySqlCommand(query, connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var legendary = new LegendaryDto
							{
								WeaponName = reader.GetString("WeaponName"),
								Infusion = reader.IsDBNull(reader.GetOrdinal("Infusion")) ? null : reader.GetString("Infusion"),
								Mods = reader.IsDBNull(reader.GetOrdinal("Mods")) ? null : reader.GetString("Mods"),
								// Slot is not applicable for default legendaries
							};

							legendaries.Add(legendary);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			ActionScheduler.RunActionOnMainThread(() => { Plugin.PluginLog.LogInfo(ex.ToString()); });
		}



		return legendaries;
	}
}
