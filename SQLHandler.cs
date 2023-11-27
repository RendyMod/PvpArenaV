using System;
using MySqlConnector;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.Models;

namespace PvpArena;

public class SQLHandler
{
	public static MySqlConnection _connection;
	
	public void InitializeAsync()
	{
		try
		{
			_connection = new MySqlConnection("Server=54.37.131.236;Port=3307;Database=varena;Uid=root;Pwd=19Mwesto36;");
			_connection.Open();
		}
		catch (Exception ex)
		{
			Unity.Debug.LogError(ex.ToString());
		}
	}
	
	[CommandFramework.Command("testsql", description: "Used for debugging", adminOnly: true)]
	public void TestSQLCommand(Player sender, string message)
	{
		var command = new MySqlCommand("INSERT INTO test (command) VALUES ('" + message + "');", _connection);
		var reader = command.ExecuteReader();
		while (reader.Read())
			Plugin.PluginLog.LogInfo(reader.GetString(0));
	}

	public void InsertOrUpdateMatchmakingData(PlayerMatchmaking1v1Data data)
	{
		try
		{
			var command = new MySqlCommand(@"
            INSERT INTO PlayerMatchmaking1v1Data (SteamID, Wins, Losses, MMR)
            VALUES (@SteamID, @Wins, @Losses, @MMR)
            ON DUPLICATE KEY UPDATE
            Wins = VALUES(Wins),
            Losses = VALUES(Losses),
            MMR = VALUES(MMR);", _connection);

			command.Parameters.AddWithValue("@SteamID", data.SteamID);
			command.Parameters.AddWithValue("@Wins", data.Wins);
			command.Parameters.AddWithValue("@Losses", data.Losses);
			command.Parameters.AddWithValue("@MMR", data.MMR);

			command.ExecuteNonQueryAsync();
		}
		catch (Exception ex)
		{
			Unity.Debug.LogError(ex.ToString());
		}
	}

	public PlayerMatchmaking1v1Data GetMatchmakingData(ulong steamId)
	{
		PlayerMatchmaking1v1Data data = null;

		try
		{
			var command = new MySqlCommand("SELECT SteamID, Wins, Losses, MMR FROM PlayerMatchmaking1v1Data WHERE SteamID = @SteamID;", _connection);
			command.Parameters.AddWithValue("@SteamID", steamId);

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					data = new PlayerMatchmaking1v1Data
					{
						SteamID = reader.GetUInt64("SteamID"),
						Wins = reader.GetInt32("Wins"),
						Losses = reader.GetInt32("Losses"),
						MMR = reader.GetInt32("MMR")
					};
				}
			}
		}
		catch (Exception ex)
		{
			Unity.Debug.LogError(ex.ToString());
		}

		return data;
	}
}
