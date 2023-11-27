using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PvpArena.Models;
public class RectangleZone
{
	public float Left { get; set; }
	public float Top { get; set; }
	public float Right { get; set; }
	public float Bottom { get; set; }
	
	public static RectangleZone FromEntity(Entity entity, int x, int y)
	{
		var tileBounds = entity.Read<TileBounds>();
		var position = entity.Read<LocalToWorld>().Position;

		float halfWidth = x / 2;
		float halfHeight = y / 2;

		float left = position.x - halfWidth;
		float right = position.x + halfWidth;
		float top = position.z + halfHeight;
		float bottom = position.z - halfHeight;


		return new RectangleZone(left, top, right, bottom);
	}

	public RectangleZone(float left, float top, float right, float bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public bool Contains(Player player)
	{
		var x = player.Position.x;
		var z = player.Position.z;
		return x >= Left && x <= Right && z >= Bottom && z <= Top;
	}

	public bool Contains(float3 position)
	{
		var x = position.x;
		var z = position.z;
		return x >= Left && x <= Right && z >= Bottom && z <= Top;
	}

	//assumes you are facing north and that are you are standing in the bottom-left square
	public static RectangleZone GetZoneByCurrentCoordinates(Player player, int tilesUp, int tilesRight)
	{
		// Assuming player.Position.x and player.Position.z give the X and Z coordinates
		var playerX = player.Position.x;
		var playerZ = player.Position.z;

		// Calculate bottom-left corner
		// Floor to the nearest multiple of 5 (tile size)
		float bottomLeftX = (float)Math.Floor(playerX / 5) * 5;
		float bottomLeftZ = (float)Math.Floor(playerZ / 5) * 5;

		// Calculate top-right corner
		float topRightX = bottomLeftX + tilesRight * 5; // Adjust by tile count * tile size
		float topRightZ = bottomLeftZ + tilesUp * 5; // Adjust by tile count * tile size

		// Create a new RectangleZone
		return new RectangleZone(bottomLeftX, topRightZ, topRightX, bottomLeftZ);
	}

	public override string ToString()
	{
		return $"\"Left\": {Left:F2},\n\"Top\": {Top:F2},\n\"Right\": {Right:F2},\n\"Bottom\": {Bottom:F2}";
	}
}

