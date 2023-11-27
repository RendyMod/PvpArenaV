using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Tiles;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace PvpArena.Data;
public static class Rotations
{

	private static quaternion RotationMode1 = new quaternion(0f, 0f, 0f, 1f);
	private static quaternion RotationMode2 = new quaternion(0f, 0.7071067f, 0f, 0.7071068f);
	private static quaternion RotationMode3 = new quaternion(0f, 1f, 0f, 0f);
	private static quaternion RotationMode4 = new quaternion(0f, -0.7071067f, 0f, 0.7071068f);

	public static Dictionary<int, TileRotation> TileRotationModes = new Dictionary<int, TileRotation>
	{
		{ 1, TileRotation.None },
		{ 2, TileRotation.Clockwise_90 },
		{ 3, TileRotation.Clockwise_180 },
		{ 4, TileRotation.Clockwise_270 },
	};

	public static Dictionary<int, quaternion> RotationModes = new Dictionary<int, quaternion>
	{
		{1, RotationMode1 },
		{2, RotationMode2 },
		{3, RotationMode3 },
		{4, RotationMode4 } 
	};

	public static int GetRotationModeFromQuaternion(Quaternion q)
	{
		q = NormalizeQuaternion(q); // Normalize the input quaternion

		foreach (var kvp in RotationModes)
		{
			Quaternion normalizedPredefined = NormalizeQuaternion(kvp.Value);

			float angle = Quaternion.Angle(q, normalizedPredefined);
			float angleNegated = Quaternion.Angle(q, NegateQuaternion(normalizedPredefined)); // Compare with negated quaternion

			// Check if either angle is within the threshold
			if (angle < 1f || angleNegated < 1f)
			{
				return kvp.Key;
			}
			else
			{
				//Debug.Log($"Angle difference for key {kvp.Key}: {angle} degrees, Negated: {angleNegated} degrees");
			}
		}

		Debug.Log($"Defaulting to 1 for: {q.ToString()}");
		return 1;
	}

	private static Quaternion NormalizeQuaternion(Quaternion q)
	{
		return Quaternion.Normalize(q);
	}

	private static Quaternion NegateQuaternion(Quaternion q)
	{
		return new Quaternion(-q.x, -q.y, -q.z, -q.w);
	}
}
