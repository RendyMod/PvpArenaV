using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bloodstone.API;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.CastleBuilding;
using ProjectM.Shared;
using ProjectM.Tiles;
using PvpArena;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Mathematics;
using Unity.Transforms;
using static PvpArena.Configs.ConfigDtos;

public static class SavedStructures
{
	private const string ConfigDirectoryName = "Bepinex/config/PvpArena";
	private const string ConfigFileName = "structures.json";
	private static readonly string FullPath = Path.Combine(ConfigDirectoryName, ConfigFileName);
	public static List<StructureDto> Structures;

	static SavedStructures()
	{
		Load();
	}

	public static void Load()
	{
		if (!File.Exists(FullPath))
		{
			ExportStructures(); // Create file with default values
			return;
		}

		try
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new PrefabGUIDConverter() }
			};
			var jsonData = File.ReadAllText(FullPath);
			Structures = JsonSerializer.Deserialize<List<StructureDto>>(jsonData, options) ?? new List<StructureDto>();
		}
		catch (Exception ex)
		{
			Structures = new List<StructureDto>();
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}

	public static void ExportStructures(Player player = null, int range = -1)
	{
		try
		{
			if (Structures == null)
			{
				Structures = new List<StructureDto>();
			}
			Structures.Clear();

			var castleEntities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection, TileModel, LocalToWorld>(true);
			foreach (var castleEntity in castleEntities)
			{
				if (castleEntity.Has<CastleRailing>())
				{
					continue;
				}
				var localToWorld = castleEntity.Read<LocalToWorld>();
				if (player != null && range != -1)
				{
					if (math.distance(localToWorld.Position, player.Position) > range)
					{
						continue;
					}
				}
				
				var dyeColor = -1;
				if (castleEntity.Has<DyeableCastleObject>())
				{
					var dyeable = castleEntity.Read<DyeableCastleObject>();
					dyeColor = dyeable.ActiveColorIndex;
				}
				Structures.Add(new StructureDto
				{
					PrefabGUID = castleEntity.Read<PrefabGUID>(),
					Location = new CoordinateDto { X = localToWorld.Position.x, Y = localToWorld.Position.y, Z = localToWorld.Position.z },
					DyeColor = dyeColor,
					Rotation = Rotations.GetRotationModeFromQuaternion(localToWorld.Rotation)
				});
			}

			Unity.Debug.Log($"done: {Structures.Count}");
			// Write the JSON string to a file

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new PrefabGUIDConverter() }
			};
			var jsonData = JsonSerializer.Serialize(Structures, options);
			File.WriteAllText(FullPath, jsonData);
		}
		catch (Exception ex)
		{
			Plugin.PluginLog.LogInfo(ex.ToString());
		}
	}
}
