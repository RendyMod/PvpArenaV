using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PvpArena.Models;
using PvpArena.Services;

namespace PvpArena.Persistence.MySql;
public class PlayerMatchmaking1v1DataStorage : MySqlDataStorage<PlayerMatchmaking1v1Data>
{
	public PlayerMatchmaking1v1DataStorage() : base() { }
	protected override async Task SaveItemAsync(PlayerMatchmaking1v1Data data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                INSERT INTO PlayerMatchmaking1v1Data (SteamID, Wins, Losses, MMR) 
                VALUES (@SteamID, @Wins, @Losses, @MMR) 
                ON DUPLICATE KEY UPDATE 
                Wins = @Wins, Losses = @Losses, MMR = @MMR;", _connection);

				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@Wins", data.Wins);
				command.Parameters.AddWithValue("@Losses", data.Losses);
				command.Parameters.AddWithValue("@MMR", data.MMR);

				await command.ExecuteNonQueryAsync();
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player matchmaking data save: {e.Message}");
			// Consider adding more detailed logging here
		}
	}

	public override async Task<List<PlayerMatchmaking1v1Data>> LoadDataAsync()
	{
		var dataList = new List<PlayerMatchmaking1v1Data>();

		try
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				var command = new MySqlCommand("SELECT SteamID, Wins, Losses, MMR FROM PlayerMatchmaking1v1Data;", connection);

				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerMatchmaking1v1Data
						{
							SteamID = reader.GetUInt64("SteamID"),
							Wins = reader.GetInt32("Wins"),
							Losses = reader.GetInt32("Losses"),
							MMR = reader.GetInt32("MMR")
						};
						dataList.Add(data);
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.MatchmakingData1v1 = data;
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch
		{
			Plugin.PluginLog.LogInfo("Exception during matchmaking 1v1 data load");
		}

		return dataList;
	}
}
