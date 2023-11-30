using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using ProjectM.Scripting;
using PvpArena.Data;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql;
public class MatchmakingArenasDataStorage
{
	private readonly string _connectionString;

	public MatchmakingArenasDataStorage(DatabaseConfig dbConfig)
	{
		_connectionString = $"Server={dbConfig.Server};" +
							$"Port={dbConfig.Port};" +
							$"Database={dbConfig.Name};" +
							$"Uid={dbConfig.UserId};" +
							$"Pwd={dbConfig.Password};";
	}

	public async Task<List<ArenaLocationDto>> LoadAllArenasAsync()
	{
		var arenas = new List<ArenaLocationDto>();

		var query = "SELECT Location1X, Location1Y, Location1Z, Location2X, Location2Y, Location2Z FROM MatchmakingArenas;";

		using (var connection = new MySqlConnection(_connectionString))
		{
			await connection.OpenAsync();

			using (var command = new MySqlCommand(query, connection))
			using (var reader = await command.ExecuteReaderAsync())
			{
				while (await reader.ReadAsync())
				{
					var location1 = new CoordinateDto
					{
						X = reader.GetFloat("Location1X"),
						Y = reader.GetFloat("Location1Y"),
						Z = reader.GetFloat("Location1Z"),
					};

					var location2 = new CoordinateDto
					{
						X = reader.GetFloat("Location2X"),
						Y = reader.GetFloat("Location2Y"),
						Z = reader.GetFloat("Location2Z"),
					};

					var arenaLocation = new ArenaLocationDto
					{
						Location1 = location1,
						Location2 = location2,
					};
					arenas.Add(arenaLocation);
				}
			}
		}

		return arenas;
	}
}
