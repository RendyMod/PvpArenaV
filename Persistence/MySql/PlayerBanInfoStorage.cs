using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using ProjectM.Scripting;
using PvpArena.Models;
using PvpArena.Services;

namespace PvpArena.Persistence.MySql;
public class PlayerBanInfoStorage : MySqlDataStorage<PlayerBanInfo>
{
	public PlayerBanInfoStorage() : base() { }

	protected override async Task SaveItemAsync(PlayerBanInfo data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                INSERT INTO PlayerBanInfo (SteamID, BannedDate, BanDurationDays, Reason) 
                VALUES (@SteamID, @BannedDate, @BanDurationDays, @Reason) 
                ON DUPLICATE KEY UPDATE 
                BannedDate = @BannedDate, BanDurationDays = @BanDurationDays, Reason = @Reason;", _connection);

				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@BannedDate", data.BannedDate);
				command.Parameters.AddWithValue("@BanDurationDays", data.BanDurationDays);
				command.Parameters.AddWithValue("@Reason", data.Reason ?? (object)DBNull.Value); // Handling potential null value for Reason

				await command.ExecuteNonQueryAsync();
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception on player ban save: {e.Message}");
		}
	}

	protected async Task UnbanPlayerAsync(Player player)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                DELETE FROM PlayerBanInfo 
                WHERE SteamID = @SteamID;", _connection);

				command.Parameters.AddWithValue("@SteamID", player.SteamID);

				await command.ExecuteNonQueryAsync();
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception on player unban: {e.Message}");
		}
	}

	public override async Task<List<PlayerBanInfo>> LoadDataAsync()
	{
		var dataList = new List<PlayerBanInfo>();
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				
				var command = new MySqlCommand("SELECT SteamID, BannedDate, BanDurationDays, Reason FROM PlayerBanInfo;", _connection);

				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerBanInfo
						{
							SteamID = reader.GetUInt64("SteamID"),
							BannedDate = reader.GetDateTime("BannedDate"),
							BanDurationDays = reader.GetInt32("BanDurationDays"),
							Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString("Reason")
						};
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.BanInfo = data;
							dataList.Add(data);
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
			return dataList;
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo("Exception on player ban load");
			return dataList;
		}
	}
}
