using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ProjectM;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Mathematics;

namespace PvpArena.Configs;
public class ConfigDtos
{

	public class StructureSpawn
	{
		public PrefabGUID PrefabGUID { get; set; }
		public CoordinateDto Location { get; set; } = new CoordinateDto();
		public int Team { get; set; } = 3;
		public int RotationMode { get; set; }  // Rotation mode (1-4)
		public int SpawnDelay { get; set; } = -1;
		public int RespawnTime { get; set; } = -1;
		public int Health { get; set; } = -1;
		public string Type { get; set; } = "";
		public List<PrefabGUID> InventoryItems { get; set; } = new List<PrefabGUID>();
		public string Description { get; set; } = "";
	}

	public class PrefabGUIDConverter : JsonConverter<PrefabGUID>
	{
		public override PrefabGUID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string guidString = reader.GetString();
			return Helper.TryGetPrefabGUIDFromString(guidString, out var prefabGUID) ? prefabGUID : PrefabGUID.Empty;
		}

		public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
		{
			string guidString = value.LookupNameString();
			writer.WriteStringValue(guidString);
		}
	}


	public class UnitSpawn
	{
		public PrefabGUID PrefabGUID { get; set; }
		public CoordinateDto Location { get; set; } = new CoordinateDto();
		public int Level { get; set; } = -1;
		public int RespawnTime { get; set; } = -1;
		public int SpawnDelay { get; set; } = 0;
		public int Health { get; set; } = -1;
		public int Team { get; set; } = 3;
		public string Type { get; set; } = "";
		public string Description { get; set; } = "";
	}

	public class CoordinateDto
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public float3 ToFloat3()
		{
			return new float3(X, Y, Z);
		}

		public static CoordinateDto FromFloat3(float3 vec)
		{
			return new CoordinateDto { X = vec.x, Y = vec.y, Z = vec.z };
		}
	}

	public class RectangleZoneDto
	{
		public float Left { get; set; }
		public float Top { get; set; }
		public float Right { get; set; }
		public float Bottom { get; set; }

		public RectangleZone ToRectangleZone()
		{
			return new RectangleZone(Left, Top, Right, Bottom);
		}

		public static RectangleZoneDto FromRectangleZone(RectangleZone zone)
		{
			return new RectangleZoneDto
			{
				Left = zone.Left,
				Top = zone.Top,
				Right = zone.Right,
				Bottom = zone.Bottom,
			};
		}
	}

	public class CircleZoneDto
	{
		public float CenterX { get; set; }
		public float CenterZ { get; set; }
		public float Radius { get; set; }

		public CircleZone ToCircleZone()
		{
			return new CircleZone(new float3(CenterX, 0, CenterZ), Radius);
		}

		public static CircleZoneDto FromCircleZone(CircleZone zone)
		{
			return new CircleZoneDto
			{
				CenterX = zone.Center.x,
				CenterZ = zone.Center.z,
				Radius = zone.Radius
			};
		}
	}

	public class CapturePointDto
	{
		public RectangleZoneDto Zone {  get; set; }
		public PrefabGUID BuffToApplyOnCapture { get; set; }
		public string Description { get; set; }
	}

	public class ArenaLocationDto
	{
		public CoordinateDto Location1 { get; set; }
		public CoordinateDto Location2 { get; set; }
	}

	public class WeaponConfigDto
	{
		public string Infusion { get; set; } = "storm";
		public string Mods { get; set; } = "123";
	}

	public class LegendaryDto
	{
		public string WeaponName { get; set; } = "spear";
		public string Infusion { get; set; } = "storm";
		public string Mods { get; set; } = "123";
		public int Slot { get; set; } = 0;
	}

	public class StructureDto
	{
		public PrefabGUID PrefabGUID { get; set; }
		public CoordinateDto Location { get; set; }
		public int DyeColor { get; set; }
		public int Rotation { get; set; }
	}

	public class JewelConfigDto
	{
		public string Mods { get; set; } = "123";
	}

	public class DatabaseConfig
	{
		public bool UseDatabaseStorage { get; set; } = false;
		public string Server { get; set; } = "";
		public int Port { get; set; } = 3306;
		public string Name { get; set; } = "";
		public string UserId { get; set; } = "";
		public string Password { get; set; } = "";
	}
}
