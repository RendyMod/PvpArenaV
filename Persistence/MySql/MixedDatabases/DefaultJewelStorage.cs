using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql.AllDatabases;
public class DefaultJewelDataStorage
{
	private Dictionary<ulong, Dictionary<string, string>> _playerJewels;
	private Dictionary<string, string> _defaultJewels;

	private readonly string _defaultJewelsConnectionString;
	private readonly string _playerJewelsConnectionString;

	public DefaultJewelDataStorage(DatabaseConfig defaultJewelsConfig, DatabaseConfig playerJewelsConfig)
	{
		_defaultJewelsConnectionString = $"Server={defaultJewelsConfig.Server};" +
								 $"Port={defaultJewelsConfig.Port};" +
								 $"Database={defaultJewelsConfig.Name};" +
								 $"Uid={defaultJewelsConfig.UserId};" +
								 $"Pwd={defaultJewelsConfig.Password};";

		_playerJewelsConnectionString = $"Server={playerJewelsConfig.Server};" +
										$"Port={playerJewelsConfig.Port};" +
										$"Database={playerJewelsConfig.Name};" +
										$"Uid={playerJewelsConfig.UserId};" +
										$"Pwd={playerJewelsConfig.Password};";
	}

	protected async Task SaveItemAsync(PlayerDefaultJewel jewel)
	{
		var query = "INSERT INTO PlayerDefaultJewels (SpellName, Mods, SteamID) VALUES (@SpellName, @Mods, @SteamID);";

		using (var connection = new MySqlConnection(_playerJewelsConnectionString))
		{
			await connection.OpenAsync();

			using (var command = new MySqlCommand(query, connection))
			{
				command.Parameters.AddWithValue("@SpellName", jewel.JewelData.SpellName);
				command.Parameters.AddWithValue("@Mods", jewel.JewelData.Mods);
				command.Parameters.AddWithValue("@SteamID", jewel.SteamID);

				await command.ExecuteNonQueryAsync();
			}
		}
	}

	public async Task<List<PlayerDefaultJewel>> LoadDataAsync(ulong SteamID)
	{
		var jewels = new List<PlayerDefaultJewel>();

		var query = "SELECT SpellName, Mods FROM PlayerDefaultJewels WHERE SteamID = @steamId;";

		using (var connection = new MySqlConnection(_playerJewelsConnectionString))
		{
			await connection.OpenAsync();

			using (var command = new MySqlCommand(query, connection))
			{
				command.Parameters.AddWithValue("@SteamID", SteamID);

				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var jewel = new DefaultJewel
						{
							SpellName = reader.GetString("SpellName"),
							Mods = reader.GetString("Mods")
						};
						var playerJewel = new PlayerDefaultJewel
						{
							SteamID = reader.GetUInt64("SteamID"),
							JewelData = jewel
						};
						jewels.Add(playerJewel);
					}
				}
			}
		}

		return jewels;
	}

	public async Task LoadAllJewelDataAsync()
	{
		try
		{
			_playerJewels = await GetAllPlayerJewelsAsync();
			_defaultJewels = await GetAllDefaultJewelsAsync();
		}
		catch (Exception ex)
		{
			ActionScheduler.RunActionOnMainThread(() => Unity.Debug.Log($"An error occurred: {ex.Message}"));
			
			throw; // Re-throw the exception if you need to handle it further up the call stack.
		}
	}

	// Helper method to load all player-specific jewels
	private async Task<Dictionary<ulong, Dictionary<string, string>>> GetAllPlayerJewelsAsync()
	{
		var playerJewels = new Dictionary<ulong, Dictionary<string, string>>();

		var query = "SELECT SteamID, SpellName, Mods FROM PlayerDefaultJewels;";

		using (var connection = new MySqlConnection(_playerJewelsConnectionString))
		{
			await connection.OpenAsync();

			using (var command = new MySqlCommand(query, connection))
			using (var reader = await command.ExecuteReaderAsync())
			{
				while (await reader.ReadAsync())
				{
					ulong steamId = reader.GetUInt64("SteamID");
					string spellName = reader.GetString("SpellName");
					string mods = reader.GetString("Mods");

					if (!playerJewels.ContainsKey(steamId))
					{
						playerJewels[steamId] = new Dictionary<string, string>();
					}

					playerJewels[steamId][spellName] = mods;
				}
			}
		}

		return playerJewels;
	}

	// Helper method to load all default jewels
	private async Task<Dictionary<string, string>> GetAllDefaultJewelsAsync()
	{
		var defaultJewels = new Dictionary<string, string>();

		var query = "SELECT SpellName, Mods FROM DefaultJewels;";

		using (var connection = new MySqlConnection(_defaultJewelsConnectionString))
		{
			await connection.OpenAsync();

			using (var command = new MySqlCommand(query, connection))
			using (var reader = await command.ExecuteReaderAsync())
			{
				while (await reader.ReadAsync())
				{
					string spellName = reader.GetString("SpellName");
					string mods = reader.GetString("Mods");
					defaultJewels[spellName] = mods;
				}
			}
		}

		return defaultJewels;
	}

	// Method to get mods, utilizing preloaded dictionaries
	public string GetModsForSpell(string spellName, ulong steamId)
	{
		// Check if there are player-specific mods first
		if (_playerJewels.ContainsKey(steamId) && _playerJewels[steamId].ContainsKey(spellName))
		{
			return _playerJewels[steamId][spellName];
		}

		// Fall back to the default mods if player-specific ones aren't found
		return _defaultJewels.TryGetValue(spellName, out var mods) ? mods : "123"; // Default mods if none are found
	}

	public async Task ExportJewels()
	{
		ActionScheduler.RunActionOnMainThread(() => {

			var jewelData = new Dictionary<ulong, Dictionary<string, string>>();

			var spellModEntities = Helper.GetEntitiesByComponentTypes<SpellModSetComponent>(true);
			foreach (var spellModEntity in spellModEntities)
			{
				/*ability.LogComponentTypes();*/
				var spellModSet = spellModEntity.Read<SpellModSetComponent>();
				if (spellModEntity.Has<AbilitySpellModItem>())
				{
					if (spellModEntity.Has<EntityOwner>())
					{
						var player = PlayerService.GetPlayerFromCharacter(spellModEntity.Read<EntityOwner>());
						var abilityGroupState = spellModEntity.Read<AbilityGroupState>();

						var abilityGroupPrefab = abilityGroupState.GroupId;
						if (PvpArena.Data.JewelData.prefabToAbilityNameDictionary.ContainsKey(abilityGroupPrefab))
						{
							var spellName = PvpArena.Data.JewelData.prefabToAbilityNameDictionary[abilityGroupPrefab];

							var equippedMods = new List<PrefabGUID> { spellModSet.SpellMods.Mod0.Id, spellModSet.SpellMods.Mod1.Id, spellModSet.SpellMods.Mod2.Id };
							var modIndices = new StringBuilder();

							foreach (var mod in equippedMods)
							{
								if (PvpArena.Data.JewelData.SpellMods.ContainsKey(spellName))
								{
									int index = PvpArena.Data.JewelData.SpellMods[spellName].FindIndex(pair => pair.Key == mod);
									if (index != -1)
									{
										modIndices.Append(index + 1); // Offset by 1
									}
								}
							}

							var modsString = modIndices.ToString();
							if (modsString.Length == 3)
							{
								if (jewelData.ContainsKey(player.SteamID))
								{
									jewelData[player.SteamID][spellName] = modsString;
								}
								else
								{
									jewelData[player.SteamID] = new Dictionary<string, string> { { spellName, modsString } };
								}
							}
						}
					}
				}
			}

			foreach (var kvp in jewelData)
			{
				SavePlayerJewelsAsync(kvp.Key, kvp.Value);
			}
		});
	}

	protected async Task SavePlayerJewelsAsync(ulong steamId, Dictionary<string, string> jewels)
	{
		using (var connection = new MySqlConnection(_playerJewelsConnectionString))
		{
			await connection.OpenAsync();
			using (var transaction = connection.BeginTransaction())
			{
				var query = @"
            INSERT INTO PlayerDefaultJewels (SteamID, SpellName, Mods) 
            VALUES (@SteamID, @SpellName, @Mods) 
            ON DUPLICATE KEY UPDATE Mods = @Mods;";

				foreach (var jewel in jewels)
				{
					using (var command = new MySqlCommand(query, connection, transaction))
					{
						command.Parameters.AddWithValue("@SteamID", steamId);
						command.Parameters.AddWithValue("@SpellName", jewel.Key);
						command.Parameters.AddWithValue("@Mods", jewel.Value);
						await command.ExecuteNonQueryAsync();
					}
				}

				await transaction.CommitAsync();
			}
		}
	}
}
