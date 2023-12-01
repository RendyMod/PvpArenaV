using HarmonyLib;
using ProjectM;
using Unity.Collections;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;

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
