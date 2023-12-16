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
public class PlayerBulletHellDataStorage : MySqlDataStorage<PlayerBulletHellData>
{
	public PlayerBulletHellDataStorage(DatabaseConfig dbConfig) : base(dbConfig) { }

	protected async override Task SaveItemAsync(PlayerBulletHellData data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
            INSERT INTO PlayerBulletHellData (SteamID, BestTime) 
            VALUES (@SteamID, @BestTime) 
            ON DUPLICATE KEY UPDATE 
            BestTime = @BestTime;", _connection);
				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@BestTime", data.BestTime);
				await command.ExecuteNonQueryAsync();
			}
		}
		catch
		{
			Plugin.PluginLog.LogInfo("Exception during player points data store");
		}
	}

	public async override Task<List<PlayerBulletHellData>> LoadDataAsync()
	{
		var dataList = new List<PlayerBulletHellData>();

		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand("SELECT SteamID, BestTime FROM PlayerBulletHellData;", _connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerBulletHellData
						{
							SteamID = reader.GetUInt64("SteamID"),
							BestTime = reader.GetString("BestTime")
						};
						dataList.Add(data);
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.PlayerBulletHellData = data;
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
