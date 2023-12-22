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
					INSERT INTO PlayerPoints (SteamID, TotalPoints, LastLoginDate, TotalPoints_EU, TotalPoints_NA, TotalPoints_CN, TotalPoints_BR, TotalPoints_TEST) 
					VALUES (@SteamID, @TotalPoints, @LastLoginDate, @TotalPoints_EU, @TotalPoints_NA, @TotalPoints_CN, @TotalPoints_BR, @TotalPoints_TEST) 
					ON DUPLICATE KEY UPDATE 
					TotalPoints = @TotalPoints,
					LastLoginDate = @LastLoginDate,
					TotalPoints_EU = @TotalPoints_EU,
					TotalPoints_NA = @TotalPoints_NA,
					TotalPoints_CN = @TotalPoints_CN,
					TotalPoints_BR = @TotalPoints_BR,
					TotalPoints_TEST = @TotalPoints_TEST;", _connection);
				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@TotalPoints", data.TotalPoints);
				command.Parameters.AddWithValue("@LastLoginDate", data.LastLoginDate);
				command.Parameters.AddWithValue("@TotalPoints_EU", data.TotalPoints_EU);
				command.Parameters.AddWithValue("@TotalPoints_NA", data.TotalPoints_NA);
				command.Parameters.AddWithValue("@TotalPoints_CN", data.TotalPoints_CN);
				command.Parameters.AddWithValue("@TotalPoints_BR", data.TotalPoints_BR);
				command.Parameters.AddWithValue("@TotalPoints_TEST", data.TotalPoints_TEST);
				await command.ExecuteNonQueryAsync();
			}
		}
		catch(Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player points data store: {e.ToString()}");
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
				var command = new MySqlCommand("SELECT SteamID, TotalPoints, LastLoginDate, TotalPoints_EU, TotalPoints_NA, TotalPoints_CN, TotalPoints_BR, TotalPoints_TEST FROM PlayerPoints;", _connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerPoints
						{
							SteamID = reader.GetUInt64("SteamID"),
							TotalPoints = reader.GetInt32("TotalPoints"),
							LastLoginDate = (reader.IsDBNull(reader.GetOrdinal("LastLoginDate"))? null : reader.GetDateTime("LastLoginDate")),
							TotalPoints_EU = reader.GetInt32("TotalPoints_EU"),
							TotalPoints_NA = reader.GetInt32("TotalPoints_NA"),
							TotalPoints_CN = reader.GetInt32("TotalPoints_CN"),
							TotalPoints_BR = reader.GetInt32("TotalPoints_BR"),
							TotalPoints_TEST = reader.GetInt32("TotalPoints_TEST")
						};
						dataList.Add(data);
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							// Ensure that PlayerService is updated to handle the new point properties
							player.PlayerPointsData = data;
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player points data load: {e.ToString()}");
		}

		return dataList;
	}
	public async Task<PlayerPoints> LoadPointsForPlayerAsync(Player player)
	{
		PlayerPoints playerPoints = null;

		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand("SELECT SteamID, TotalPoints, LastLoginDate, TotalPoints_EU, TotalPoints_NA, TotalPoints_CN, TotalPoints_BR, TotalPoints_TEST FROM PlayerPoints WHERE SteamID = @SteamID;", _connection);
				command.Parameters.AddWithValue("@SteamID", player.SteamID);

				using (var reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						playerPoints = new PlayerPoints
						{
							SteamID = reader.GetUInt64("SteamID"),
							TotalPoints = reader.GetInt32("TotalPoints"),
							LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : (DateTime?)reader.GetDateTime("LastLoginDate"),
							TotalPoints_EU = reader.GetInt32("TotalPoints_EU"),
							TotalPoints_NA = reader.GetInt32("TotalPoints_NA"),
							TotalPoints_CN = reader.GetInt32("TotalPoints_CN"),
							TotalPoints_BR = reader.GetInt32("TotalPoints_BR"),
							TotalPoints_TEST = reader.GetInt32("TotalPoints_TEST")
						};

						// Optionally, update the player object with points info
						var action = () =>
						{
							player.PlayerPointsData = playerPoints;
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player points info load: {e.Message}");
		}

		return playerPoints;
	}
}
