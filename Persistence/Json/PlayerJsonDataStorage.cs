using System.Collections.Generic;
using System.Threading.Tasks;
using PvpArena.Services;
using static PvpArena.Services.PlayerService;

namespace PvpArena.Persistence.Json;


public class PlayerJsonDataStorage<T> : JsonDataStorage<T> where T : PlayerData, new()
{
	public PlayerJsonDataStorage(string filePath) : base(filePath)
	{
	}

	public new async Task SaveDataAsync(List<T> dataList)
	{
		await base.SaveDataAsync(dataList);
	}

	public async Task SaveDataAsync()
	{
		List<T> dataList = GetPlayerSubData<T>();
		await base.SaveDataAsync(dataList);
	}

	public new async Task<List<T>> LoadDataAsync()
	{
		// Load the data using the base class's LoadDataAsync method.
		var data = await base.LoadDataAsync();

		// Update the player cache with the loaded data.
		UpdatePlayerCache(data);

		return data;
	}
}
