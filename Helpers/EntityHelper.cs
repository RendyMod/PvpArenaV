using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using PvpArena.Services;
using PvpArena.Models;
using PvpArena.Data;
using PvpArena.Configs;
using Il2CppSystem;
using Unity.Physics;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static Entity CreateEntityWithComponents<T1>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2, T3>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2, T3, T4>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity CreateEntityWithComponents<T1, T2, T3, T4, T5>()
	{
		return VWorld.Server.EntityManager.CreateEntity(
			new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T4>(), ComponentType.AccessMode.ReadWrite),
			new ComponentType(Il2CppType.Of<T5>(), ComponentType.AccessMode.ReadWrite)
		);
	}

	public static Entity GetPrefabEntityByPrefabGUID(PrefabGUID prefabGUID)
	{
		return Core.prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID];
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1>(bool includeDisabled = false)
	{
		EntityQueryOptions options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[] { new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static Entity GetHoveredEntity(Entity Character)
	{
		var input = Character.Read<EntityInput>();
		var position = input.AimPosition;
		if (input.HoveredEntity.Index > 0)
		{
			return input.HoveredEntity;
		}
		else
		{
			var entities = GetEntitiesByComponentTypes<PrefabGUID, PhysicsCollider>();
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				return entities[0];
			}
		}
		return Entity.Null;
	}

	public static Entity GetHoveredEntity<T>(Entity Character)
	{
		var input = Character.Read<EntityInput>();
		var position = input.AimPosition;
		if (input.HoveredEntity.Index > 0)
		{
			return input.HoveredEntity;
		}
		else
		{
			var entities = GetEntitiesByComponentTypes<PrefabGUID, PhysicsCollider, T>();
			if (entities.Length > 0)
			{
				SortEntitiesByDistance(entities, position);
				return entities[0];
			}
		}
		return Entity.Null;
	}

	public static List<Entity> GetEntitiesNearPosition(Player player, int amount = 5)
	{
		List<Entity> entityList = new List<Entity>();
		var position = player.Position;
		var entities = GetEntitiesByComponentTypes<PrefabGUID>();
		if (entities.Length > 0)
		{
			SortEntitiesByDistance(entities, position);
		}
		for (var i = 0; i < amount && i < entities.Length; i++)
		{
			entityList.Add(entities[i]);
		}
		return entityList;
	}

	public static List<Entity> GetHoveredEntities(Entity Character, int amount)
	{
		var input = Character.Read<EntityInput>();
		var position = input.AimPosition;
		List<Entity> entityList = new List<Entity>();
		
		var entities = GetEntitiesByComponentTypes<PrefabGUID>();
		if (entities.Length > 0)
		{
			SortEntitiesByDistance(entities, position);
		}
		for (var i = 0; i < amount && i < entities.Length; i++)
		{
			entityList.Add(entities[i]);
		}
		return entityList;
	}

	public static NativeArray<Entity> SortEntitiesByDistance(NativeArray<Entity> entities, float3 position)
	{
		// Create a temporary array to hold entities and their distances
		(Entity entity, float distance)[] tempArray = new (Entity, float)[entities.Length];

		// Populate the temporary array
		for (int i = 0; i < entities.Length; i++)
		{
			float distance = float.MaxValue;
			if (entities[i].Has<LocalToWorld>())
			{
				LocalToWorld ltw = entities[i].Read<LocalToWorld>();
				distance = math.distance(position, ltw.Position);
			}

			tempArray[i] = (entities[i], distance);
		}

		// Sort the temporary array based on distance
		System.Array.Sort(tempArray, (a, b) => a.distance.CompareTo(b.distance));

		// Extract the sorted entities back into the NativeArray
		for (int i = 0; i < entities.Length; i++)
		{
			entities[i] = tempArray[i].entity;
		}

		return entities;
	}

	public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1>()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetPrefabEntitiesByComponentTypes<T1, T2>()
	{
		EntityQueryOptions options = EntityQueryOptions.IncludePrefab;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeDisabled = false)
	{
		EntityQueryOptions options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2, T3>(bool includeDisabled = false)
	{
		EntityQueryOptions options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<T3>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};

		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}
}
