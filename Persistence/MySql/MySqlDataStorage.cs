using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MySqlConnector;
using PvpArena.Configs;
using PvpArena.Models;
using PvpArena.Persistence.Json;
using static PvpArena.Services.PlayerService;

namespace PvpArena.Persistence.MySql;

public abstract class MySqlDataStorage<T> : IDataStorage<T>
{
	protected MySqlConnection _connection;
	protected string connectionString;

	protected MySqlDataStorage()
	{
		var dbConfig = PvpArenaConfig.Config.Database;
		connectionString = $"Server={dbConfig.Server};" +
							  $"Port={dbConfig.Port};" +
							  $"Database={dbConfig.Name};" +
							  $"Uid={dbConfig.UserId};" +
							  $"Pwd={dbConfig.Password};";
	}

	public async Task SaveDataAsync(List<T> data)
	{
		var tasks = new List<Task>();
		foreach (var item in data)
		{
			tasks.Add(SaveItemAsync(item));
		}
		await Task.WhenAll(tasks);
	}

	protected abstract Task SaveItemAsync(T item);

	public abstract Task<List<T>> LoadDataAsync();
}
