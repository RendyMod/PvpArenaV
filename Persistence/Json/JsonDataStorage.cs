using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PvpArena.Persistence.Json;

public interface IDataStorage<T>
{
	Task<List<T>> LoadDataAsync();
	Task SaveDataAsync(List<T> data);
}

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
	private readonly string _dateFormat;

	public DateTimeConverterUsingDateTimeParse(string dateFormat)
	{
		_dateFormat = dateFormat;
	}

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		Debug.Assert(typeToConvert == typeof(DateTime));
		return DateTime.ParseExact(reader.GetString(), _dateFormat, CultureInfo.InvariantCulture);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(_dateFormat, CultureInfo.InvariantCulture));
	}
}

public class JsonDataStorage<T> : IDataStorage<T>
{
	private string _filePath;

	public JsonDataStorage(string filePath)
	{
		_filePath = filePath;
	}

	public async Task<List<T>> LoadDataAsync()
	{
		if (!File.Exists(_filePath))
			return new List<T>();

		var options = new JsonSerializerOptions
		{
			Converters = { new DateTimeConverterUsingDateTimeParse("yyyy-MM-ddTHH:mm:ss") }
		};

		using (var stream = File.OpenRead(_filePath))
		{
			return await JsonSerializer.DeserializeAsync<List<T>>(stream, options) ?? new List<T>();
		}
	}

	public async Task SaveDataAsync(List<T> data)
	{
		var options = new JsonSerializerOptions
		{
			Converters = { new DateTimeConverterUsingDateTimeParse("yyyy-MM-ddTHH:mm:ss") }
		};

		var jsonData = JsonSerializer.Serialize(data, options);
		await File.WriteAllTextAsync(_filePath, jsonData);
	}
}
