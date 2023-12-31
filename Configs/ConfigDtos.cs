using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Terrain.MapMaker;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Mathematics;
using Unity.Physics.Authoring;

namespace PvpArena.Configs;

public class ConfigDtos
{
	public class PrefabSpawn
	{
		public PrefabGUID PrefabGUID { get; set; }
		public CoordinateDto Location { get; set; } = new CoordinateDto();
		public int Team { get; set; } = 3;
		public int RotationMode { get; set; } // Rotation mode (1-4)
		public int SpawnDelay { get; set; } = -1;
		public int RespawnTime { get; set; } = -1;
		public int Health { get; set; } = -1;
		public string Type { get; set; } = "";
		public List<PrefabGUID> InventoryItems { get; set; } = new List<PrefabGUID>();
		public string Description { get; set; } = "";
	}

	public class PrefabGUIDConverter : JsonConverter<PrefabGUID>
	{
		public override PrefabGUID Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string guidString = reader.GetString();
			return Helper.TryGetPrefabGUIDFromString(guidString, out var prefabGUID) ? prefabGUID : PrefabGUID.Empty;
		}

		public override void Write (Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
		{
			string guidString = value.LookupNameString();
			writer.WriteStringValue(guidString);
		}
	}

	public class TemplateUnitSpawn
	{
		public UnitSpawn UnitSpawn { get; set; }
		public int Quantity { get; set; }
	}

	public class MercenaryCamp
	{
		public List<UnitSpawn> UnitSpawns { get; set; } = new();
		public RectangleZoneDto Zone { get; set; } = new();
		public int SpawnDelay { get; set; } = 0;
		public int RespawnTime { get; set; } = 240;
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

		public float3 ToFloat3 ()
		{
			return new float3(X, Y, Z);
		}

		public static CoordinateDto FromFloat3 (float3 vec)
		{
			return new CoordinateDto { X = vec.x, Y = vec.y, Z = vec.z };
		}
	}

	public class CaptureThePancakeArenaDto
	{
		public CoordinateDto Team1PlayerRespawn { get; set; } = new CoordinateDto();
		public CoordinateDto Team2PlayerRespawn { get; set; } = new CoordinateDto();
		public RectangleZoneDto Team1EndZone { get; set; } = new RectangleZoneDto();
		public RectangleZoneDto Team2EndZone { get; set; } = new RectangleZoneDto();
		public RectangleZoneDto EntireMapZone { get; set; } = new RectangleZoneDto();
		public RectangleZoneDto MapCenter { get; set; } = new RectangleZoneDto();
		public List<PrefabSpawn> StructureSpawns { get; set; } = new List<PrefabSpawn>();
		public List<UnitSpawn> UnitSpawns { get; set; } = new List<UnitSpawn>();
	}

	public class RectangleZoneDto
	{
		public float Left { get; set; }
		public float Top { get; set; }
		public float Right { get; set; }
		public float Bottom { get; set; }

		public RectangleZone ToRectangleZone ()
		{
			return new RectangleZone(Left, Top, Right, Bottom);
		}

		public static RectangleZoneDto FromRectangleZone (RectangleZone zone)
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
		public float CenterY { get; set; } = 0;
		public float CenterZ { get; set; }
		public float Radius { get; set; }

		public CircleZone ToCircleZone ()
		{
			return new CircleZone(new float3(CenterX, CenterY, CenterZ), Radius);
		}

		public static CircleZoneDto FromCircleZone (CircleZone zone)
		{
			return new CircleZoneDto
			{
				CenterX = zone.Center.x,
				CenterY = zone.Center.y, //this isn't used for any calculations
				CenterZ = zone.Center.z,
				Radius = zone.Radius
			};
		}
	}

	public class BulletHellArenaDto
	{
		public CircleZoneDto FightZone { get; set; }
		public TemplateUnitSpawn TemplateUnitSpawn { get; set; }
	}

	public class CapturePointDto
	{
		public RectangleZoneDto Zone { get; set; }
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
		public int Slot { get; set; } = -1;
	}

	public class StructureDto
	{
		public PrefabGUID PrefabGUID { get; set; }
		public CoordinateDto Location { get; set; }
		public int DyeColor { get; set; }
		public int Rotation { get; set; }
	}

	public class DefaultJewel
	{
		public string SpellName { get; set; }
		public string Mods { get; set; } = "123";
	}

	public class PlayerDefaultJewel
	{
		public DefaultJewel JewelData { get; set; }
		public ulong SteamID { get; set; }
	}

	public class DatabaseConfig
	{
		public string Server { get; set; } = "";
		public int Port { get; set; } = 3306;
		public string Name { get; set; } = "";
		public string UserId { get; set; } = "";
		public string Password { get; set; } = "";
	}

	public class TraderDto
	{
		public UnitSpawn UnitSpawn { get; set; }
		public List<TraderItemDto> TraderItems { get; set; }
	}

	public class TraderItemDto
	{
		public PrefabGUID OutputItem { get; set; }
		public int OutputAmount { get; set; }
		public PrefabGUID InputItem { get; set; }
		public int InputAmount { get; set; }
		public int StockAmount { get; set; }
		public bool AutoRefill { get; set; } = true;
	}
}
