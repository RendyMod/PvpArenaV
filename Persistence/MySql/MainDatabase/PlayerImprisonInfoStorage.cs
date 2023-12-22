using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using ProjectM.Scripting;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql.MainDatabase;
public class PlayerImprisonInfoStorage : MySqlDataStorage<PlayerImprisonInfo>
{
	public PlayerImprisonInfoStorage(DatabaseConfig dbConfig) : base(dbConfig) { }

	protected override async Task SaveItemAsync(PlayerImprisonInfo data)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                INSERT INTO PlayerImprisonInfo (SteamID, ImprisonedDate, ImprisonDurationDays, Reason, PrisonCellNumber) 
                VALUES (@SteamID, @ImprisonedDate, @ImprisonDurationDays, @Reason, @PrisonCellNumber) 
                ON DUPLICATE KEY UPDATE 
                ImprisonedDate = @ImprisonedDate, ImprisonDurationDays = @ImprisonDurationDays, Reason = @Reason, PrisonCellNumber = @PrisonCellNumber;", _connection);

				command.Parameters.AddWithValue("@SteamID", data.SteamID);
				command.Parameters.AddWithValue("@ImprisonedDate", data.ImprisonedDate);
				command.Parameters.AddWithValue("@ImprisonDurationDays", data.ImprisonDurationDays);
				command.Parameters.AddWithValue("@Reason", data.Reason ?? (object)DBNull.Value); // Handling potential null value for Reason
				command.Parameters.AddWithValue("@PrisonCellNumber", data.PrisonCellNumber); // Handling potential null value for Reason

				await command.ExecuteNonQueryAsync();
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception on player ban save: {e.Message}");
		}
	}

	protected async Task UnimprisonPlayerAsync(Player player)
	{
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand(@"
                DELETE FROM PlayerImprisonInfo 
                WHERE SteamID = @SteamID;", _connection);

				command.Parameters.AddWithValue("@SteamID", player.SteamID);

				await command.ExecuteNonQueryAsync();
			}
			player.ImprisonInfo = new PlayerImprisonInfo();
			player.CurrentState = Player.PlayerState.Normal;
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception on player unimprison: {e.Message}");
		}
	}

	public override async Task<List<PlayerImprisonInfo>> LoadDataAsync()
	{
		var dataList = new List<PlayerImprisonInfo>();
		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();

				var command = new MySqlCommand("SELECT SteamID, ImprisonedDate, ImprisonDurationDays, Reason, PrisonCellNumber FROM PlayerImprisonInfo;", _connection);

				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						var data = new PlayerImprisonInfo
						{
							SteamID = reader.GetUInt64("SteamID"),
							ImprisonedDate = reader.GetDateTime("ImprisonedDate"),
							ImprisonDurationDays = reader.GetInt32("ImprisonDurationDays"),
							Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString("Reason"),
							PrisonCellNumber = reader.GetInt32("PrisonCellNumber")
						};
						var action = () =>
						{
							var player = PlayerService.GetPlayerFromSteamId(data.SteamID);
							player.ImprisonInfo = data;
							if (player.ImprisonInfo.IsImprisoned())
							{
								player.CurrentState = Player.PlayerState.Imprisoned;
							}
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
			Plugin.PluginLog.LogInfo("Exception on player imprison load");
			return dataList;
		}
	}

	public async Task<PlayerImprisonInfo> LoadDataForPlayerAsync(Player player)
	{
		PlayerImprisonInfo playerImprisonInfo = null;

		try
		{
			using (var _connection = new MySqlConnection(connectionString))
			{
				await _connection.OpenAsync();
				var command = new MySqlCommand("SELECT SteamID, ImprisonedDate, ImprisonDurationDays, Reason, PrisonCellNumber FROM PlayerImprisonInfo WHERE SteamID = @SteamID;", _connection);
				command.Parameters.AddWithValue("@SteamID", player.SteamID);

				using (var reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						playerImprisonInfo = new PlayerImprisonInfo
						{
							SteamID = reader.GetUInt64("SteamID"),
							ImprisonedDate = reader.GetDateTime("ImprisonedDate"),
							ImprisonDurationDays = reader.GetInt32("ImprisonDurationDays"),
							Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString("Reason"),
							PrisonCellNumber = reader.GetInt32("PrisonCellNumber")
						};

						// Optionally, update the player object
						var action = () =>
						{
							player.ImprisonInfo = playerImprisonInfo;
							if (player.ImprisonInfo.IsImprisoned())
							{
								player.CurrentState = Player.PlayerState.Imprisoned;
							}
						};
						ActionScheduler.RunActionOnMainThread(action);
					}
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception during player imprison info load: {e.Message}");
		}

		return playerImprisonInfo;
	}

}
