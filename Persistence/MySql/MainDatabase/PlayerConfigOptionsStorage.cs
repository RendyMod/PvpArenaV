using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Xml;
using MySqlConnector;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql.MainDatabase;
public class PlayerConfigOptionsStorage : MySqlDataStorage<PlayerConfigOptions>
{
	public PlayerConfigOptionsStorage(DatabaseConfig dbConfig) : base(dbConfig) { }

	protected override async Task SaveItemAsync(PlayerConfigOptions data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                INSERT INTO PlayerConfigOptions (SteamID, SubscribeToKillFeed) 
                VALUES (@SteamID, @SubscribeToKillFeed) 
                ON DUPLICATE KEY UPDATE 
                SubscribeToKillFeed = @SubscribeToKillFeed;", _connection);

				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@SubscribeToKillFeed", data.SubscribeToKillFeed);

				await command.ExecuteNonQueryAsync();
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player config save: {e.ToString()}");
		}
	}

	public override async Task<List<PlayerConfigOptions>> LoadDataAsync()
	{
		var dataList = new List<PlayerConfigOptions>();

		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();

				var command = new MySqlCommand("SELECT SteamID, SubscribeToKillFeed FROM PlayerConfigOptions;", _connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerConfigOptions
						{
							SteamID = reader.GetUInt64("SteamID"),
							SubscribeToKillFeed = reader.GetBoolean("SubscribeToKillFeed")
						};
						dataList.Add(data);
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.ConfigOptions = data;
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player config load: {e.Message}");
		}

		return dataList;
	}
}
