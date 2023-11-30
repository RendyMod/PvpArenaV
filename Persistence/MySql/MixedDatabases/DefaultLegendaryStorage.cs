using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using MySqlConnector;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
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

	public async Task ExportLegendariesToDatabase()
	{
		try
		{
			var players = PlayerService.UserCache.Values;
			ActionScheduler.RunActionOnMainThread(() =>
			{
				foreach (var player in players)
				{
					var playerLegendaries = new List<LegendaryDto>();

					for (var i = 0; i < 36; i++)
					{
						if (InventoryUtilities.TryGetItemAtSlot(VWorld.Server.EntityManager, player.Character, i, out InventoryBuffer item))
						{
							if (item.ItemEntity._Entity.Has<LegendaryItemSpellModSetComponent>())
							{
								var legendary = item.ItemEntity._Entity;
								var legendaryModData = legendary.Read<LegendaryItemSpellModSetComponent>();
								var weaponType = legendary.Read<PrefabGUID>();
								var infusionPrefab = legendaryModData.AbilityMods0.Mod0.Id;
								var mod1 = LegendaryData.statModGuidToIndex[legendaryModData.StatMods.Mod0.Id];
								var mod2 = LegendaryData.statModGuidToIndex[legendaryModData.StatMods.Mod1.Id];
								var mod3 = LegendaryData.statModGuidToIndex[legendaryModData.StatMods.Mod2.Id];
								var legendaryConfigDto = new LegendaryDto
								{
									Infusion = LegendaryData.prefabToInfusionDictionary[infusionPrefab],
									WeaponName = LegendaryData.prefabToWeaponDictionary[weaponType],
									Mods = $"{mod1}{mod2}{mod3}",
									Slot = i
								};

								playerLegendaries.Add(legendaryConfigDto);
							}
						}
					};
					if (playerLegendaries.Any())
					{
						InsertPlayerLegendariesToDatabase(player.SteamID, playerLegendaries);
					}
				}
			});
		}
		catch (Exception ex)
		{
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	private async Task InsertPlayerLegendariesToDatabase(ulong steamId, List<LegendaryDto> legendaries)
	{
		using (var connection = new MySqlConnection(serverDatabaseConnectionString))
		{
			await connection.OpenAsync();
			foreach (var legendary in legendaries)
			{
				var query = @"
                INSERT INTO PlayerLegendaryWeapons (SteamID, WeaponName, Infusion, Mods, Slot) 
                VALUES (@SteamID, @WeaponName, @Infusion, @Mods, @Slot);";

				using (var command = new MySqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@SteamID", steamId);
					command.Parameters.AddWithValue("@WeaponName", legendary.WeaponName);
					command.Parameters.AddWithValue("@Infusion", legendary.Infusion);
					command.Parameters.AddWithValue("@Mods", legendary.Mods);
					command.Parameters.AddWithValue("@Slot", legendary.Slot);

					await command.ExecuteNonQueryAsync();
				}
			}
		}
	}
}
