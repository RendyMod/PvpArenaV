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
using PvpArena.Factories;
using static PvpArena.Factories.UnitFactory;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using UnityEngine;
using UnityEngine.Rendering;
using PvpArena.GameModes;
using Il2CppSystem.Security.Cryptography;

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public const int RANDOM_POWER = -1;

	public static NativeHashSet<PrefabGUID> prefabGUIDs;

	public static System.Random random = new System.Random();

	public static void RevivePlayer(Player player)
	{
		var sbs = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();
		var bufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
		var buffer = bufferSystem.CreateCommandBuffer();

		Nullable_Unboxed<float3> spawnLoc = new();
		spawnLoc.value = player.Position;
		spawnLoc.has_value = true;
		var health = player.Character.Read<Health>();
		if (Helper.HasBuff(player, Prefabs.Buff_General_Vampire_Wounded_Buff))
		{
			Helper.RemoveBuff(player, Prefabs.Buff_General_Vampire_Wounded_Buff);

			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			player.Character.Write(health);
		}

		if (health.IsDead)
		{
			sbs.RespawnCharacter(buffer, player.User,
				customSpawnLocation: spawnLoc,
				previousCharacter: player.Character);
		}
	}

	public static void KickPlayer(ulong PlatformId)
	{
		var kickEventEntity = CreateEntityWithComponents<KickEvent>();
		kickEventEntity.Write(new KickEvent
		{
			PlatformId = PlatformId
		});
	}

	public static float Clamp(float value, float min, float max)
	{
		return System.Math.Max(min, System.Math.Min(value, max));
	}

	public static void Unlock(Player player, bool unlockContent = false)
	{
		var fromCharacter = player.ToFromCharacter();
		Core.debugEventsSystem.UnlockAllResearch(fromCharacter);
		Core.debugEventsSystem.UnlockAllVBloods(fromCharacter);
		Core.debugEventsSystem.CompleteAllAchievements(fromCharacter);
		UnlockAllWaypoints(player);
        
		if (unlockContent)
			UnlockAllContent(fromCharacter);
	}

	public static void UnlockAllContent(FromCharacter fromCharacter)
	{
		SetUserContentDebugEvent setUserContentDebugEvent = new SetUserContentDebugEvent
		{
			Value = UserContentFlags.EarlyAccess | UserContentFlags.DLC_DraculasRelics_EA |
					UserContentFlags.GiveAway_Razer01 | UserContentFlags.DLC_FoundersPack_EA |
					UserContentFlags.Halloween2022 | UserContentFlags.DLC_Gloomrot
		};
		Core.debugEventsSystem.SetUserContentDebugEvent(fromCharacter.User.Read<User>().Index, ref setUserContentDebugEvent,
			ref fromCharacter);
	}

	public static void UnlockAllWaypoints(Player player)
	{
		var buffer = VWorld.Server.EntityManager.AddBuffer<UnlockedWaypointElement>(player.User);
		var waypointComponentType =
			new ComponentType(Il2CppType.Of<ChunkWaypoint>(), ComponentType.AccessMode.ReadWrite);
		var query = VWorld.Server.EntityManager.CreateEntityQuery(waypointComponentType);
		var waypoints = query.ToEntityArray(Allocator.Temp);
		foreach (var waypoint in waypoints)
		{
			var unlockedWaypoint = new UnlockedWaypointElement();
			unlockedWaypoint.Waypoint = waypoint.Read<NetworkId>();
			buffer.Add(unlockedWaypoint);
		}

		waypoints.Dispose();
	}

	public static void RenamePlayer(FromCharacter fromCharacter, string newName)
	{
		var networkId = fromCharacter.User.Read<NetworkId>();
		var renameEvent = new RenameUserDebugEvent
		{
			NewName = newName,
			Target = networkId
		};
		Core.debugEventsSystem.RenameUser(fromCharacter, renameEvent);
	}

	public static void ResetAllServants(Team playerTeam)
	{
		var servantCoffinComponentType =
			new ComponentType(Il2CppType.Of<ServantCoffinstation>(), ComponentType.AccessMode.ReadWrite);
		var query = VWorld.Server.EntityManager.CreateEntityQuery(servantCoffinComponentType);
		var servantCoffins = query.ToEntityArray(Allocator.Temp);

		foreach (var servantCoffin in servantCoffins)
		{
			try
			{
				var coffinTeam = servantCoffin.Read<Team>();
				if (coffinTeam.Value == playerTeam.Value)
				{
					var servantCoffinStation = servantCoffin.Read<ServantCoffinstation>();
					var servant = servantCoffinStation.ConnectedServant._Entity;
					var servantEquipment = servant.Read<ServantEquipment>();
					servantEquipment.Reset();
					servant.Write(servantEquipment);
					StatChangeUtility.KillEntity(VWorld.Server.EntityManager, servant, Entity.Null, 0, true);
				}
			}
			catch (System.Exception e)
			{

			}
		}
	}

	public static bool UserHasAbilitiesUnlocked(Entity User)
	{
		var buffer = User.ReadBuffer<AttachedBuffer>();
		foreach (var attached in buffer)
		{
			if (attached.PrefabGuid == Prefabs.ProgressionCollection)
			{
				var progressionEntity = attached.Entity;
				var unlockedAbilityBuffer = progressionEntity.ReadBuffer<UnlockedAbilityElement>();
				if (unlockedAbilityBuffer.Length > 20)
				{
					return true;
				}
			}
		}

		return false;
	}


	public static void DestroyEntity(Entity entity)
	{
		if (entity.Has<CanFly>())
		{
			entity.Remove<CanFly>(); //this prevents a unit from being respawned when killed this way
		}
		StatChangeUtility.KillOrDestroyEntity(VWorld.Server.EntityManager, entity, entity, entity, 0, true);
	}

	public static void RespawnPlayer(Player player, float3 pos)
	{
		if (!player.IsAlive)
		{
			var buffer = Core.entityCommandBufferSystem.CreateCommandBuffer();

			Nullable_Unboxed<float3> spawnLoc = new();
			spawnLoc = new();
			spawnLoc.value = pos;
			spawnLoc.has_value = true;

			Core.serverBootstrapSystem.RespawnCharacter(buffer, player.User,
			customSpawnLocation: spawnLoc,
				previousCharacter: player.Character, spawnLocationIndex: 0);
		}
	}

	//TODO: investigate doing this by making copying the Teleport player model but make a from character using a non-player character
	public static void Teleport(this Entity unit, float3 targetPosition)
	{
		Player anyPlayer = null;
		foreach (var player in PlayerService.OnlinePlayers.Keys)
		{
			anyPlayer = player;
			break;
		}
        if (anyPlayer == null)
        {
            foreach (var player in PlayerService.UserCache.Values)
            {
                anyPlayer = player;
                break;
            }
        }
		var eventEntity = Helper.CreateEntityWithComponents<TeleportDebugEvent, FromCharacter>();
		eventEntity.Write(anyPlayer.ToFromCharacter());
		eventEntity.Write(new TeleportDebugEvent
		{
			Location = TeleportDebugEvent.TeleportLocation.WorldPosition,
			MousePosition = unit.Read<LocalToWorld>().Position,
			Target = TeleportDebugEvent.TeleportTarget.ClosestUnitToCursor,
			LocationPosition = targetPosition
		});
	}

	public static void Teleport(this Player player, float3 targetPosition)
	{
        if (player.User.Exists())
        {
            var entity = VWorld.Server.EntityManager.CreateEntity(
                ComponentType.ReadWrite<FromCharacter>(),
                ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
            );
            entity.Write(player.ToFromCharacter());
            entity.Write<PlayerTeleportDebugEvent>(new()
            {
                Position = targetPosition,
                Target = PlayerTeleportDebugEvent.TeleportTarget.Self
            });
        }
        else
        {
            player.Character.Teleport(targetPosition);
        }
	}

    public class ResetOptions
    {
        public bool RemoveConsumables = false;
        public bool RemoveShapeshifts = false;
        public bool ResetCooldowns = true;
        public HashSet<PrefabGUID> BuffsToIgnore = new HashSet<PrefabGUID>();

        public static ResetOptions Default => new ResetOptions();
        public static ResetOptions FreshMatch = new ResetOptions
        {
            RemoveConsumables = true,
            RemoveShapeshifts = true,
            ResetCooldowns = true,
            BuffsToIgnore = new HashSet<PrefabGUID>()
        };
    }

	public static void Reset(this Player player, ResetOptions resetOptions = null)
	{
        resetOptions ??= ResetOptions.Default;

        if (resetOptions.ResetCooldowns)
		{
            ResetCooldown(player.Character);
		}
        ClearExtraBuffs(player.Character, resetOptions);
        //delay so that removing gun e / heart strike doesnt dmg you
        var action = new ScheduledAction(HealEntity, new object[] { player.Character });
		ActionScheduler.ScheduleAction(action, 3);

        GameEvents.RaisePlayerReset(player);
    }

	public static void MakeSCT(Player player, PrefabGUID sctPrefab, float value = 0, float3 pos = default)
	{
		if (pos.Equals(default(float3)))
		{
			pos = player.Position;
		}
		var sctEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.ScrollingCombatTextMessage);
		ScrollingCombatTextMessage.Create(VWorld.Server.EntityManager, Core.entityCommandBufferSystem.CreateCommandBuffer(), sctEntity, value, sctPrefab, pos, player.Character, player.Character);
	}

	public static void MakeSCTLocal(Player player, PrefabGUID sctPrefab)
	{
		var sctEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.ScrollingCombatTextMessage);
		ScrollingCombatTextMessage.CreateLocal(VWorld.Server.EntityManager, sctEntity, "hello", player.Position, new float3(0,0,0), player.User, 0f, sctPrefab);
	}

	public static void ResetCooldown(Entity Character)
	{
		var buffer = Character.ReadBuffer<AbilityGroupSlotBuffer>();
		foreach (var ability in buffer)
		{
			var abilityGroupSlotEntity = ability.GroupSlotEntity._Entity;
			if (abilityGroupSlotEntity.Exists())
			{
				var abilityGroupSlotData = abilityGroupSlotEntity.Read<AbilityGroupSlot>();
				var abilityGroupSlotStateEntity = abilityGroupSlotData.StateEntity._Entity;
				if (abilityGroupSlotStateEntity.Exists())
				{
					if (abilityGroupSlotStateEntity.Has<AbilityChargesState>())
					{
						var abilityChargesState = abilityGroupSlotStateEntity.Read<AbilityChargesState>();
						var abilityChargesData = abilityGroupSlotStateEntity.Read<AbilityChargesData>();
						abilityChargesState.CurrentCharges = abilityChargesData.MaxCharges;
						abilityChargesState.ChargeTime = 0;
						abilityGroupSlotStateEntity.Write(abilityChargesState);
					}

					var abilityStateBuffer = abilityGroupSlotStateEntity.ReadBuffer<AbilityStateBuffer>();
                    
                    foreach (var state in abilityStateBuffer)
					{
						var abilityState = state.StateEntity._Entity;
						if (abilityState.Exists())
						{
							var abilityCooldownState = abilityState.Read<AbilityCooldownState>();
							abilityCooldownState.CooldownEndTime = 0;
							abilityState.Write(abilityCooldownState);
						}
					}
				}
			}
		}
	}

	public static void HealEntity(Entity entity)
	{
		Health health = entity.Read<Health>();
		health.Value = health.MaxHealth;
		health.MaxRecoveryHealth = health.MaxHealth;
		entity.Write(health);
	}

	// Adds Blood Moon visual Effect + Witch + Rage on Toggle
	public static void ToggleBuffsOnPlayer(Player player)
	{
		if (!Helper.HasBuff(player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff))
		{
			Helper.BuffPlayer(player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff, out var buffEntity, Helper.NO_DURATION, true);
			Helper.BuffPlayer(player, Prefabs.AB_Consumable_SpellBrew_T02_Buff, out buffEntity, NO_DURATION, true);
			Helper.BuffPlayer(player, Prefabs.Buff_BloodMoon, out buffEntity, Helper.NO_DURATION, true);
		}
		else
		{
			Helper.RemoveBuff(player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff);
			Helper.RemoveBuff(player, Prefabs.AB_Consumable_SpellBrew_T02_Buff);
			Helper.RemoveBuff(player, Prefabs.Buff_BloodMoon);
		}
	}

	public static void ToggleBloodOnPlayer(Player player)
	{
		if (!Helper.HasBuff(player, Prefabs.AB_BloodBuff_Warrior_WeaponCooldown))
			SetPlayerBlood(player, Prefabs.BloodType_Warrior, 100f);
		else
			SetPlayerBlood(player, Prefabs.BloodType_None, 100f);
	}

	public static double GetServerTime()
	{
		return Core.traderSyncSystem._ServerTime.GetSingleton().TimeOnServer;
	}

	public static void KillPreviousEntities(string category)
	{
		var entities = Helper.GetEntitiesByComponentTypes<CanFly>(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (UnitFactory.TryGetSpawnedUnitFromEntity(entity, out SpawnedUnit spawnedUnit))
				{
					if (spawnedUnit.Unit.Category == category)
					{
						Helper.DestroyEntity(entity);
					}
				}
				else
				{
					if (entity.Has<ResistanceData>() && entity.Read<ResistanceData>().GarlicResistance_IncreasedExposureFactorPerRating == StringToFloatHash(category))
					{
						Helper.DestroyEntity(entity);
					}
				}
			}
		}
	}

    public static void ChangeBuffResistances(Entity entity, PrefabGUID prefabGuid)
	{
        entity.Add<BuffResistances>();
        var prefabEntity = GetPrefabEntityByPrefabGUID(prefabGuid);
        entity.Write(new BuffResistances
        {
            InitialSettingGuid = prefabGuid,
            SettingsEntity = ModifiableEntity.CreateFixed(prefabEntity)
        });
    }

    public static void MakePlayerCcImmune(Player player)
    {
        player.Character.Add<BuffResistances>();
        player.Character.Write(new BuffResistances
        {
            SettingsEntity = ModifiableEntity.Create(player.Character, VWorld.Server.EntityManager, Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_UberMob)),
            InitialSettingGuid = Prefabs.BuffResistance_UberMob
        });
    }

	public static void ApplyBuildImpairBuffToPlayer(Player player)
	{
		if (!player.IsAdmin)
		{
			if (Helper.BuffPlayer(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown, out var buffEntity2))
			{
				Helper.ModifyBuff(buffEntity2, BuffModificationTypes.BuildMenuImpair);
			}
		}
		else
		{
			Helper.RemoveBuff(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown);
		}
	}

	public static void ControlUnit(Player player, Entity unit)
	{
		Helper.BuffPlayer(player, Prefabs.Admin_Observe_Invisible_Buff, out var buffEntity);
		var action = () =>
		{
			var controlDebugEvent = new ControlDebugEvent
			{
				EntityTarget = unit,
				Target = unit.Read<NetworkId>()
			};

			if (unit.Has<AggroConsumer>())
			{
				var aggroConsumer = unit.Read<AggroConsumer>();
				aggroConsumer.Active.Value = false;
				unit.Write(aggroConsumer);
			}

			Core.debugEventsSystem.ControlUnit(player.ToFromCharacter(), controlDebugEvent);
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 0.05f);
	}

	public static void ControlOriginalCharacter(Player player)
	{
		var controlledEntity = player.ControlledEntity;
		float3 position = player.Position;
		if (controlledEntity.Exists())
		{
			if (controlledEntity.Has<LocalToWorld>())
			{
				position = controlledEntity.Read<LocalToWorld>().Position;
			}
			Helper.DestroyEntity(controlledEntity);
		}
		
		ControlUnit(player, player.Character);
		var action = () => {
			player.Teleport(position);
			Helper.RemoveBuff(player, Prefabs.Admin_Observe_Invisible_Buff);
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 0.05f);
	}

	public static void MakePlayerCcDefault(Player player)
    {
        player.Character.Add<BuffResistances>();
        player.Character.Write(new BuffResistances
        {
            SettingsEntity = ModifiableEntity.Create(player.Character, VWorld.Server.EntityManager, Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_Vampire)),
            InitialSettingGuid = Prefabs.BuffResistance_Vampire
        });
    }

    public static void AnnounceSiegeWeapon()
	{
		CreateEntityWithComponents<AnnounceSiegeWeapon, SpawnTag, DestroyOnSpawn, PrefabGUID>();
	}

    public enum SnapMode
    {
        NorthWest = 1, North, NorthEast, West, Center, East, SouthWest, South, SouthEast
    }

    public static float3 GetSnappedHoverPosition(Player player, SnapMode mode)
    {
        float3 originalPosition = player.Character.Read<EntityInput>().AimPosition;
        // Calculate the bottom-left corner of the tile
        float tileX = Mathf.Floor(originalPosition.x / 5) * 5;
        float tileZ = Mathf.Floor(originalPosition.z / 5) * 5;

        // Adjust position based on the snap mode
        switch (mode)
        {
            case SnapMode.NorthWest:
                return new float3(tileX, originalPosition.y, tileZ + 5);
            case SnapMode.North:
                return new float3(tileX + 2.5f, originalPosition.y, tileZ + 5);
            case SnapMode.NorthEast:
                return new float3(tileX + 5, originalPosition.y, tileZ + 5);
            case SnapMode.West:
                return new float3(tileX, originalPosition.y, tileZ + 2.5f);
            case SnapMode.Center:
                return new float3(tileX + 2.5f, originalPosition.y, tileZ + 2.5f);
            case SnapMode.East:
                return new float3(tileX + 5, originalPosition.y, tileZ + 2.5f);
            case SnapMode.SouthWest:
                return new float3(tileX, originalPosition.y, tileZ);
            case SnapMode.South:
                return new float3(tileX + 2.5f, originalPosition.y, tileZ);
            case SnapMode.SouthEast:
                return new float3(tileX + 5, originalPosition.y, tileZ);
            default:
                return originalPosition; // Default case to handle unexpected mode
        }
    }
}
