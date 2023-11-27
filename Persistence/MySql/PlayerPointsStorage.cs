using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PvpArena.Models;
using PvpArena.Services;

namespace PvpArena.Persistence.MySql;
public class PlayerPointsStorage : MySqlDataStorage<PlayerPoints>
{
	public PlayerPointsStorage() : base() { }

	protected async override Task SaveItemAsync(PlayerPoints data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
            INSERT INTO PlayerPoints (SteamID, TotalPoints) 
            VALUES (@SteamID, @TotalPoints) 
            ON DUPLICATE KEY UPDATE 
            TotalPoints = @TotalPoints;", _connection);
				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@TotalPoints", data.TotalPoints);
				await command.ExecuteNonQueryAsync();
			}
		}
		catch 
		{
			Plugin.PluginLog.LogInfo("Exception during player points data store");
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
				var command = new MySqlCommand("SELECT SteamID, TotalPoints FROM PlayerPoints;", _connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerPoints
						{
							SteamID = reader.GetUInt64("SteamID"),
							TotalPoints = reader.GetInt32("TotalPoints")
						};
						var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
						player.PlayerPointsData = data;
						dataList.Add(data);
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
