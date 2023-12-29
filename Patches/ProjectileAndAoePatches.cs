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
using ProjectM.Network;
using ProjectM.CastleBuilding;

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
				if (!owner.Exists())
				{
					owner = entity.Read<EntityCreator>().Creator._Entity;
					if (!owner.Exists())
					{
						owner = entity;
					}
				}
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

[HarmonyPatch(typeof(HitCastColliderSystem_OnSpawn), nameof(HitCastColliderSystem_OnSpawn.OnUpdate))]
public static class HitCastColliderSystem_OnSpawnPatch
{
	public static void Prefix(HitCastColliderSystem_OnSpawn __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Exists())
			{
				var owner = entity.Read<EntityOwner>().Owner;
				if (!owner.Exists())
				{
					owner = entity.Read<EntityCreator>().Creator._Entity;
					if (!owner.Exists())
					{
						owner = entity;
					}
				}
				if (owner.Exists())
				{
					if (owner.Has<PlayerCharacter>())
					{
						var player = PlayerService.GetPlayerFromCharacter(owner);
						GameEvents.RaisePlayerHitColliderCastCreated(player, entity);
					}
					else
					{
						GameEvents.RaiseUnitHitColliderCastCreated(owner, entity);
					}
				}
			}
		}
		entities.Dispose();
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
					if (!owner.Exists())
					{
						owner = entity.Read<EntityCreator>().Creator._Entity;
						if (!owner.Exists())
						{
							owner = entity;
						}
					}
					if (owner.Exists())
					{
						if (owner.Has<PlayerCharacter>())
						{
							var player = PlayerService.GetPlayerFromCharacter(owner);
							GameEvents.RaisePlayerHitColliderCastUpdate(player, entity);
						}
						else
						{
							GameEvents.RaiseUnitHitColliderCastUpdate(owner, entity);
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


[HarmonyPatch(typeof(HandleGameplayEventsBase), nameof(HandleGameplayEventsBase.OnUpdate))]
public static class HandleGameplayEventsBasePatch
{
	public static void Prefix(HandleGameplayEventsBase __instance)
	{
		var entities = __instance.__RemoveOnHitTrigger_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.GetPrefabGUID() == Prefabs.Buff_General_BounceDelay)
			{
				var buffer = entity.ReadBuffer<HitTrigger>();
				foreach (var hitTrigger in buffer)
				{
					if (hitTrigger.Target.Has<CastleHeartConnection>())
					{
						entity.Remove<ScriptDestroy>(); //stops wolf bounce
						Helper.DestroyBuff(entity);
						break;
					}
				}
			}
			else if (entity.Has<ApplyBuffOnGameplayEvent>() && entity.Has<Projectile>())
			{
				var buffer = entity.ReadBuffer<HitTrigger>();
				foreach (var hitTrigger in buffer)
				{
					if (hitTrigger.Target.Has<CastleHeartConnection>())
					{
						if (entity.GetPrefabGUID() == Prefabs.AB_Frost_CrystalLance_Projectile_SpellMod_Pierce)
						{
							Helper.DestroyEntity(entity);
							break;
						}
						var buffer2 = entity.ReadBuffer<ApplyBuffOnGameplayEvent>();
						for (var i = 0; i < buffer2.Length; i++)
						{
							var buff = buffer2[i];
							buff.Buff0 = PrefabGUID.Empty;
							buff.Buff1 = PrefabGUID.Empty;
							buff.Buff2 = PrefabGUID.Empty;
							buff.Buff3 = PrefabGUID.Empty;
							buffer2[i] = buff;
						}
					}
				}
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

public class FromCharacterListener : EntityQueryListener
{
	public void OnNewMatchFound(Entity entity)
	{
		if (entity.Exists())
		{
			var user = entity.Read<FromCharacter>().User;
			if (user.Exists())
			{
				var player = PlayerService.GetPlayerFromUser(user);
				entity.LogComponentTypes();
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
