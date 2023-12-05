using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using PvpArena;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Listeners;
using PvpArena.Services;
using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace PvpArena
{
	public class PrefabSpawnerService
	{
		private static Entity empty_entity = new Entity();
		private static UnitSpawnerUpdateSystem usus = VWorld.Server.GetExistingSystem<UnitSpawnerUpdateSystem>();

		internal const int DEFAULT_MINRANGE = 0;
		internal const int DEFAULT_MAXRANGE = 0;

		//components:


		private static Action<Entity> WrapCallback(Action<Entity> originalAction, int rotationMode, bool tagForDestruction = false, string category = "")
		{
			return entity =>
			{
				var prefabUnit = Helper.GetPrefabEntityByPrefabGUID(entity.Read<PrefabGUID>());
				prefabUnit.Add<DestroyOnSpawn>();
				prefabUnit.Remove<DestroyWhenInventoryIsEmpty>();
				if (!tagForDestruction)
				{
					entity.Remove<CanFly>();
				}
				if (category != "") //we store the category on the unit's useless stats in case we need to find them after a server restart
				{
					entity.Add<ResistanceData>();
					entity.Write(new ResistanceData
					{
						GarlicResistance_IncreasedExposureFactorPerRating = UnitFactory.StringToFloatHash(category)
					});
				}

				if (Rotations.RotationModes.ContainsKey(rotationMode)) 
				{
					var rotation = entity.Read<Rotation>();
					rotation.Value = Rotations.RotationModes[rotationMode];
					entity.Write(rotation);
				}
				

				// Call the original action
				originalAction(entity);
			};
		}
		public static void Spawn(PrefabGUID unit, int count, float3 position, float minRange = 0, float maxRange = 0, float duration = -1)
		{
			usus.SpawnUnit(empty_entity, unit, position, count, minRange, maxRange, duration);
		}

		public static void SpawnWithCallback(PrefabGUID unit, float3 position, Action<Entity> postActions, int rotation = 0, float duration = -1, bool tagEntityForCallback = true, string category = "")
		{
			var buildingKey = $"{position.xz}_{unit.GuidHash}";
			var durationKey = NextKey();
			var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(unit);
			if (tagEntityForCallback)
			{
				prefabEntity.Add<CanFly>();
			}
			
			prefabEntity.Remove<DestroyOnSpawn>();
			// Spawn the entity1
			usus.SpawnUnit(empty_entity, unit, position, 1, DEFAULT_MINRANGE, DEFAULT_MAXRANGE, durationKey);

			// Associate the unique key with the post-action callback
			var wrappedCallback = WrapCallback(postActions, rotation, tagEntityForCallback, category);

			if (prefabEntity.Has<CastleHeartConnection>())
			{
				if (!BuildingPostActions.ContainsKey(buildingKey))
				{
					BuildingPostActions.Add(buildingKey, wrappedCallback);
				}
			}
			else
			{
				if (!UnitPostActions.ContainsKey(durationKey))
				{
					UnitPostActions.Add(durationKey, (duration, wrappedCallback));
				}
			}
		}
		static internal long NextKey()
		{
			System.Random r = new();
			long key;
			int breaker = 5;
			do
			{
				key = r.NextInt64(10000) * 3;
				breaker--;
				if (breaker < 0)
				{
					throw new Exception($"Failed to generate a unique key for UnitSpawnerService");
				}
			} while (UnitPostActions.ContainsKey(key));
			return key;
		}

		static internal Dictionary<string, Action<Entity>> BuildingPostActions = new Dictionary<string, Action<Entity>>();
		static internal Dictionary<long, (float actualDuration, Action<Entity>)> UnitPostActions = new Dictionary<long, (float actualDuration, Action<Entity>)>();

/*		[HarmonyPatch(typeof(SetTeamOnSpawnSystem), nameof(SetTeamOnSpawnSystem.OnUpdate))]
		public static class BuildingSpawnPatch
		{
			private static Queue<Entity> entityQueue = new Queue<Entity>();

			public static void Prefix(SetTeamOnSpawnSystem __instance)
			{

				// Enqueue new entities for processing in the next frame
				var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
				{
					foreach (var entity in entities)
					{
						if (entity.Has<CastleHeartConnection>())
						{
							if (entity.Exists() && entity.Has<LocalToWorld>())
							{
								var localToWorld = entity.Read<LocalToWorld>();
								var key = localToWorld.Position.xz;
								if (BuildingPostActions.ContainsKey(key))
								{
									BuildingPostActions[key](entity);
									BuildingPostActions.Remove(key);
								}
							}
						}
					}
				}
				entities.Dispose();
			}
		}*/

		[HarmonyPatch(typeof(UnitSpawnerReactSystem), nameof(UnitSpawnerReactSystem.OnUpdate))]
		public static class UnitSpawnerReactSystem_Patch
		{
			public static void Prefix(UnitSpawnerReactSystem __instance)
			{
				var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
				foreach (var entity in entities)
				{
					if (!entity.Has<LifeTime>()) continue;
					
					var lifetimeComp = entity.Read<LifeTime>();
					var durationKey = (long)Mathf.Round(lifetimeComp.Duration);
					if (UnitPostActions.TryGetValue(durationKey, out var unitData))
					{
						var (actualDuration, actions) = unitData;
						UnitPostActions.Remove(durationKey);

						var endAction = actualDuration < 0 ? LifeTimeEndAction.None : LifeTimeEndAction.Destroy;
                        actualDuration = 0;
						var newLifeTime = new LifeTime()
						{
							Duration = actualDuration,
							EndAction = endAction
						};

						entity.Write(newLifeTime);
						actions(entity);
					}
				}
				entities.Dispose();
			}
		}

		public class ManuallySpawnedPrefabListener : EntityQueryListener
		{
			public void OnNewMatchFound(Entity entity)
			{
				if (entity.Exists() && entity.Has<LocalToWorld>() && !entity.Has<PlayerCharacter>() && !entity.Has<UnitSpawnData>())
				{
					var localToWorld = entity.Read<LocalToWorld>();
					var key = $"{localToWorld.Position.xz}_{entity.Read<PrefabGUID>().GuidHash}";
					if (BuildingPostActions.ContainsKey(key))
					{
						BuildingPostActions[key](entity);
						BuildingPostActions.Remove(key);
					}
				}
			}

			public void OnNewMatchRemoved(Entity entity)
			{
				
			}
		}

	}
}


