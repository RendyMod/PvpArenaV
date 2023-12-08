using HarmonyLib;
using ProjectM;
using Unity.Collections;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;
using System;
using PvpArena.GameModes;
using PvpArena.Services;
using ProjectM.Shared;
using static ProjectM.HitColliderCast;
using PvpArena.Listeners;
using Unity.Entities;
using Unity.Transforms;
using ProjectM.Gameplay.Scripting;
using PvpArena.Helpers;
using ProjectM.Scripting;
using PvpArena.Models;
using static UnityEngine.UI.GridLayoutGroup;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(ProjectileSystem_Spawn_Server), nameof(ProjectileSystem_Spawn_Server.OnUpdate))]
public static class ProjectileSystem_Spawn_ServerPatch
{
	public static void Prefix(ProjectileSystem_Spawn_Server __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var prefabGuid = entity.Read<PrefabGUID>();
				var owner = entity.Read<EntityOwner>().Owner;
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerProjectileCreated(player, entity);
					}
					else
					{
						GameEvents.RaiseUnitProjectileCreated(owner, entity);
					}
				}
			}
			catch (Exception e)
			{
				entities.Dispose();
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

[HarmonyPatch(typeof(HitCastColliderSystem_OnUpdate), nameof(HitCastColliderSystem_OnUpdate.OnUpdate))]
public static class HitCastColliderSystem_OnUpdatePatch
{
	public static void Prefix(HitCastColliderSystem_OnUpdate __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				if (entity.Exists())
				{
					var owner = entity.Read<EntityOwner>().Owner;
					var prefabGuid = entity.Read<PrefabGUID>();
					if (owner.Exists())
					{
						if (entity.Has<Projectile>())
						{
							if (owner.Has<PlayerCharacter>())
							{
								var player = PlayerService.GetPlayerFromCharacter(owner);
								GameEvents.RaisePlayerProjectileUpdate(player, entity);
							}
							else
							{
								GameEvents.RaiseUnitProjectileUpdate(owner, entity);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				entities.Dispose();
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		entities.Dispose();
	}
}

public class TargetAoeListener : EntityQueryListener
{
	public void OnNewMatchFound(Entity entity)
	{
		if (entity.Exists())
		{
			var owner = entity.Read<EntityOwner>().Owner;
			if (owner.Exists())
			{
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerAoeCreated(player, entity);
					}
					else
					{
						GameEvents.RaiseUnitAoeCreated(owner, entity);
					}
				}
			}
		}
	}

	public void OnNewMatchRemoved(Entity entity)
	{

	}

	public void OnUpdate(Entity entity)
	{

	}
}
