using PvpArena.Commands;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Network;
using PvpArena.Matchmaking;
using PvpArena.Services;
using ProjectM.UI;
using ProjectM.LightningStorm;
using ProjectM.Sequencer;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using ProjectM.Debugging;
using static ProjectM.Gameplay.Systems.DealDamageSystem;
using System.Runtime.InteropServices;
using ProjectM.CastleBuilding;
using ProjectM.Presentation;
using PvpArena.GameModes;
using ProjectM.Gameplay.Scripting;
using static ProjectM.HitColliderCast;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(ProjectileSystem_Spawn_Server), nameof(ProjectileSystem_Spawn_Server.OnUpdate))]
public static class ProjectileSystem_Spawn_ServerPatch
{
	public static void Prefix(ProjectileSystem_Spawn_Server __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var prefabGuid = entity.Read<PrefabGUID>();
			if (prefabGuid == Prefabs.AB_Subdue_Projectile || prefabGuid == Prefabs.AB_Sorceress_Projectile)
			{
				var buffer = entity.ReadBuffer<HitColliderCast>();
				for (var i = 0; i < buffer.Length; i++)
				{
					var hitCollider = buffer[i];
					hitCollider.IgnoreImmaterial = true;
					buffer[i] = hitCollider;
				}
			}
		}
	}
}

[HarmonyPatch(typeof(HitCastColliderSystem_OnDestroy), nameof(HitCastColliderSystem_OnDestroy.OnUpdate))]
public static class HitCastColliderSystem_OnDestroyPatch
{
	public static void Prefix(HitCastColliderSystem_OnDestroy __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var prefabGuid = entity.Read<PrefabGUID>();
			if (prefabGuid == Prefabs.AB_Sorceress_AoE_Throw)
			{
				var buffer = entity.ReadBuffer<HitColliderCast>();
				for (var i = 0; i < buffer.Length; i++)
				{
					var hitColliderCast = buffer[i];
					hitColliderCast.IgnoreImmaterial = true;
					buffer[i] = hitColliderCast;
				}
			}
		}
		entities.Dispose();
	}
}
