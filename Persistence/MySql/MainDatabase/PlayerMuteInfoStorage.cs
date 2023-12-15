using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql.MainDatabase;
public class PlayerMuteInfoStorage : MySqlDataStorage<PlayerMuteInfo>
{
	public PlayerMuteInfoStorage(DatabaseConfig dbConfig) : base(dbConfig) { }

	protected override async Task SaveItemAsync(PlayerMuteInfo data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				// SQL command to insert or update data
				var command = new MySqlCommand(@"
            INSERT INTO PlayerMuteInfo (SteamID, MutedDate, MuteDurationDays) 
            VALUES (@SteamID, @MutedDate, @MuteDurationDays) 
            ON DUPLICATE KEY UPDATE 
            MutedDate = @MutedDate, MuteDurationDays = @MuteDurationDays;", _connection);

				// Add parameters to avoid SQL injection
				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@MutedDate", data.MutedDate);
				command.Parameters.AddWithValue("@MuteDurationDays", data.MuteDurationDays);

				// Execute the command
				await command.ExecuteNonQueryAsync();
			}
		}
		catch
		{
			Plugin.PluginLog.LogInfo("Exception during player mute data save");
		}
	}

	protected async Task UnmutePlayerAsync(Player player)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                DELETE FROM PlayerMuteInfo 
                WHERE SteamID = @SteamID;", _connection);

				command.Parameters.AddWithValue("@SteamID", player.SteamID);

				await command.ExecuteNonQueryAsync();
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception on player unmute: {e.Message}");
		}
	}

	public override async Task<List<PlayerMuteInfo>> LoadDataAsync()
	{
		var dataList = new List<PlayerMuteInfo>();

		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();

				var command = new MySqlCommand("SELECT SteamID, MutedDate, MuteDurationDays FROM PlayerMuteInfo;", _connection);
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerMuteInfo
						{
							SteamID = reader.GetUInt64("SteamID"),
							MutedDate = reader.GetDateTime("MutedDate"),
							MuteDurationDays = reader.GetInt32("MuteDurationDays")
						};
						dataList.Add(data);
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.MuteInfo = data;
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch
		{
			Plugin.PluginLog.LogInfo("Exception during player mute data load");
		}


		return dataList;
	}
}
