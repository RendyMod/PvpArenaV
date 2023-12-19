using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql.MainDatabase;
public class PlayerPointsStorage : MySqlDataStorage<PlayerPoints>
{
	public PlayerPointsStorage(DatabaseConfig dbConfig) : base(dbConfig) { }

	protected async override Task SaveItemAsync(PlayerPoints data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
            INSERT INTO PlayerPoints (SteamID, TotalPoints, LastLoginDate) 
            VALUES (@SteamID, @TotalPoints, @LastLoginDate) 
            ON DUPLICATE KEY UPDATE 
            TotalPoints = @TotalPoints;", _connection);
				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@TotalPoints", data.TotalPoints);
				command.Parameters.AddWithValue("@LastLoginDate", data.LastLoginDate);
				await command.ExecuteNonQueryAsync();
			}
		}
		catch(Exception e)
		{
			Plugin.PluginLog.LogInfo("Exception during player points data store");
			Plugin.PluginLog.LogError(e);
		}
	}

	public async override Task<List<PlayerPoints>> LoadDataAsync()
	{
		var dataList = new List<PlayerPoints>();

		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand("SELECT SteamID, TotalPoints, LastLoginDate FROM PlayerPoints;", _connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerPoints
						{
							SteamID = reader.GetUInt64("SteamID"),
							TotalPoints = reader.GetInt32("TotalPoints"),
							LastLoginDate = reader.GetDateTime("LastLoginDate")
						};
						dataList.Add(data);
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.PlayerPointsData = data;
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch
		{
			Plugin.PluginLog.LogInfo("Exception during player points data load");
		}

		return dataList;
	}
}
