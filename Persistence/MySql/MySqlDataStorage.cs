using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using static PvpArena.Configs.ConfigDtos;

namespace PvpArena.Persistence.MySql;

public abstract class MySqlDataStorage<T>
{
	protected MySqlConnection _connection;
	protected string connectionString;

	protected MySqlDataStorage(DatabaseConfig dbConfig)
	{
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
