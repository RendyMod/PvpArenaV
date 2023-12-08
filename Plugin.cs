using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.GameModes.Matchmaking1v1;
using PvpArena.Helpers;
using PvpArena.Services;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PvpArena;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin, IRunOnInitialized
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;

	public override void Load()
	{
		PluginLog = Log;
		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
		// Harmony patching
		Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
	}

	public override bool Unload()
	{
		try
		{
			Core.Dispose();
			Harmony?.UnpatchSelf();
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Ran into error while unloading: {e.ToString()}");
		}
		return true;
	}

	public void OnGameInitialized()
	{
		Initialize();
	}

	public static void OnServerStart()
	{
		PluginLog.LogInfo("Running OnServerStart code");
		
		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			var entities = Helper.GetEntitiesByComponentTypes<User>(true);
			foreach (var entity in entities)
			{
				try
				{
					var player = PlayerService.GetPlayerFromUser(entity);
					player.Teleport(new float3(0, 0, 0));
				}
				catch (Exception e)
				{

				}
			}
			entities.Dispose();
		}

		Initialize();
	}

	public static void ModifyPrefabs()
	{
		var castleEntities = Helper.GetPrefabEntitiesByComponentTypes<CastleHeartConnection>();
		foreach (var entity in castleEntities)
		{

		};


		List<PrefabGUID> abilitiesToRemoveMovementMods = new List<PrefabGUID>
		{
			Prefabs.AB_Werewolf_Howl_Buff,
			Prefabs.Buff_BloodMoon
		};

		List<PrefabGUID> consumableAbilitiesToModify = new List<PrefabGUID>
		{
			Prefabs.AB_Consumable_RoseTea_T02_AbilityGroup,
			Prefabs.AB_Consumable_RoseTea_T01_AbilityGroup,
		};
		Entity prefabEntity;
		foreach (var ability in abilitiesToRemoveMovementMods)
		{
			prefabEntity = Helper.GetPrefabEntityByPrefabGUID(ability);
			prefabEntity.Remove<ModifyMovementSpeedBuff>();
		}

		prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_Castle_Fence_Iron02);
		prefabEntity.Remove<Immortal>();

		prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Item_Consumable_Salve_Vermin);
		var itemData = prefabEntity.Read<ItemData>();
		itemData.RemoveOnConsume = false;
		prefabEntity.Write(itemData);

		prefabEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Item_Consumable_TrippyShroom);
		itemData = prefabEntity.Read<ItemData>();
		itemData.RemoveOnConsume = false;
		prefabEntity.Write(itemData);

		var entities = Helper.GetPrefabEntitiesByComponentTypes<AbilityGroupConsumeItemOnCast>();
		foreach (var entity in entities)
		{
			var prefabGuid = entity.Read<PrefabGUID>();
			if (consumableAbilitiesToModify.Contains(prefabGuid))
			{
				entity.Remove<AbilityGroupConsumeItemOnCast>();
			}
		}
		entities.Dispose();

		//modify bags
		entities = Helper.GetPrefabEntitiesByComponentTypes<Restricted_InventoryBuffer>();
		foreach (var entity in entities)
		{
			var buffer = entity.ReadBuffer<Restricted_InventoryBuffer>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var tempInv = buffer[i];
				tempInv.RestrictedItemCategory = ItemCategory.Weapon | ItemCategory.Armor | ItemCategory.Gem | ItemCategory.BloodBound |
							  ItemCategory.Flower | ItemCategory.Lumber | ItemCategory.Stone | ItemCategory.BloodEssence |
							  /*ItemCategory.SoulBound*/ ItemCategory.Silver | ItemCategory.LoseDurabilityOnDeath |
							  ItemCategory.Knowledge | ItemCategory.Blood | /* Skip ItemCategory.Relic, */ ItemCategory.Coin |
							  ItemCategory.Consumable | ItemCategory.Herb | ItemCategory.Bag | ItemCategory.Saddle |
							  ItemCategory.FishCommon | ItemCategory.FishUncommon | ItemCategory.FishRare;
				;
				buffer[i] = tempInv;
			}
		}
		entities.Dispose();

		var startingTombPrefab = Helper.GetPrefabEntityByPrefabGUID(Prefabs.TM_Respawn_TombCoffin);
		startingTombPrefab.Add<DestroyOnSpawn>();

		List<PrefabGUID> PrefabsToIgnore = new List<PrefabGUID>
		{
			Prefabs.CHAR_Illusion_Mosquito,
			/*Prefabs.CHAR_Unholy_FallenAngel,*/
			Prefabs.CHAR_Unholy_DeathKnight,
			Prefabs.CHAR_Unholy_Baneling,
			Prefabs.CHAR_Unholy_SkeletonWarrior_Summon,
			Prefabs.CHAR_Unholy_SkeletonApprentice_Summon,
			Prefabs.CHAR_Spectral_Guardian,
			Prefabs.CHAR_Spectral_SpellSlinger,
			Prefabs.CHAR_NecromancyDagger_SkeletonBerserker_Armored_Farbane,
			Prefabs.AB_Vampire_VeilOfFrost_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfIllusion_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfStorm_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfBlood_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfBones_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfChaos_ConfuseDummy,
			Prefabs.AB_Vampire_VeilOfChaos_SpellMod_BonusDummy,
			Prefabs.AB_Storm_BallLightning_Projectile,
			Prefabs.CHAR_Trader_Farbane_RareGoods_T01,
			Prefabs.CHAR_Trader_Dunley_RareGoods_T02,
			Prefabs.CHAR_Cursed_Witch_Exploding_Mosquito,
			Prefabs.CHAR_Militia_Longbowman
	};
		entities = Helper.GetEntitiesByComponentTypes<AggroConsumer>(true);
		PreventUnitSpawns(entities, PrefabsToIgnore);
		entities.Dispose();
		entities = Helper.GetPrefabEntitiesByComponentTypes<AggroConsumer>();
		PreventUnitSpawns(entities, PrefabsToIgnore);
		entities.Dispose();

		entities = Helper.GetPrefabEntitiesByComponentTypes<ArmorLevelSource>();
		foreach (var itemEntity in entities)
		{
			ArmorLevelSource armorLevelSource = itemEntity.Read<ArmorLevelSource>();
			armorLevelSource.Level = 80;
			itemEntity.Write(armorLevelSource);

			EquippableData equippableData = itemEntity.Read<EquippableData>();
			equippableData.EquipmentSet = Prefabs.SetBonus_T08_Shadowmoon;
			itemEntity.Write(equippableData);
		}
		entities.Dispose();
	}

	private static void PreventUnitSpawns(NativeArray<Entity> entities, List<PrefabGUID> PrefabsToIgnore)
	{
		foreach (var entity in entities)
		{
			var prefabGuid = entity.Read<PrefabGUID>();
			if (entity.Has<AggroConsumer>() && !entity.Has<CanFly>())
			{
				if (!PrefabsToIgnore.Contains(prefabGuid))
				{
					if (!prefabGuid.LookupNameString().ToLower().Contains("summon"))
					{
						entity.Add<DestroyOnSpawn>();
					}
					else
					{
						entity.Remove<DestroyOnSpawn>();
					}
				}
				else
				{
					entity.Remove<DestroyOnSpawn>();
				}
			}
		}
		entities.Dispose();
	}

	public static void Initialize()
	{
		if (!HasLoaded())
		{
			return;
		}
		Core.Initialize();
		var action = new ScheduledAction(HandleDebugSettings);
		ActionScheduler.ScheduleAction(action, 30);

		var radialZoneSystem_Holy_Server = VWorld.Server.GetExistingSystem<RadialZoneSystem_Holy_Server>();
		var radialZoneSystem_Garlic_Server = VWorld.Server.GetExistingSystem<RadialZoneSystem_Garlic_Server>();
		var radialZoneSystem_Curse_Server = VWorld.Server.GetExistingSystem<RadialZoneSystem_Curse_Server>();
		var respawnAiEventSystem = VWorld.Server.GetExistingSystem<RespawnAiEventSystem>();
		var resetBloodOnRespawnSystem = VWorld.Server.GetExistingSystem<ResetBloodOnRespawnSystem>();
		var onDeathSystem = VWorld.Server.GetExistingSystem<OnDeathSystem>();
		var playerCombatBuffSystem = VWorld.Server.GetExistingSystem<PlayerCombatBuffSystem>();
		var createGameplayEventsOnDeathSystem = VWorld.Server.GetExistingSystem<CreateGameplayEventsOnDeathSystem>();
		var spawnRegionSpawnSystem = VWorld.Server.GetExistingSystem<SpawnRegionSpawnSystem>();
		var initializeNewSpawnChainSystem = VWorld.Server.GetExistingSystem<InitializeNewSpawnChainSystem>();
		var dropInInventoryOnSpawn = VWorld.Server.GetExistingSystem<DropInInventoryOnSpawnSystem>();
		var unitSpawnerReactSystem = VWorld.Server.GetExistingSystem<UnitSpawnerReactSystem>();
		var pavementBonusSystem = VWorld.Server.GetExistingSystem<PavementBonusSystem>();
		var patrolMoveSystem = VWorld.Server.GetExistingSystem<PatrolMoveSystem>();

		radialZoneSystem_Holy_Server.Enabled = false;
		radialZoneSystem_Garlic_Server.Enabled = false;
		radialZoneSystem_Curse_Server.Enabled = false;
		respawnAiEventSystem.Enabled = false;
		resetBloodOnRespawnSystem.Enabled = false; //stops you from going to frailed blood when you die
		onDeathSystem.Enabled = false; //stops you from dropping your hat when you die
		playerCombatBuffSystem.Enabled = false; //setting to false totally removes pve in combat buff, but also can mess up hp regen
		spawnRegionSpawnSystem.Enabled = false;
		initializeNewSpawnChainSystem.Enabled = true;
		dropInInventoryOnSpawn.Enabled = false;
		unitSpawnerReactSystem.Enabled = true;
		pavementBonusSystem.Enabled = false;
		patrolMoveSystem.Enabled = false;
		/*createGameplayEventsOnDeathSystem.Enabled = true;*/
		if (PvpArenaConfig.Config.MatchmakingEnabled)
		{
			MatchmakingService.Start();
		}

		ModifyPrefabs();


		Unity.Debug.Log("Loading player data");
	}

	private static void HandleDebugSettings()
	{
		SetDebugSettingEvent DayNightCycleDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.DayNightCycleDisabled,
			Value = true
		};

		SetDebugSettingEvent CastleHeartBloodEssenceDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.CastleHeartBloodEssenceDisabled,
			Value = true
		};

		SetDebugSettingEvent DropsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.DropsDisabled,
			Value = false
		};

		SetDebugSettingEvent BloodDrainDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BloodDrainDisabled,
			Value = true
		};

		SetDebugSettingEvent DurabilityDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.DurabilityDisabled,
			Value = true
		};

		SetDebugSettingEvent DynamicCloudsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.DynamicCloudsDisabled,
			Value = true
		};

		SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BuildCostsDisabled,
			Value = true
		};

		SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
			Value = true
		};

		SetDebugSettingEvent CastleHeartConnectionRequirementDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.CastleHeartConnectionRequirementDisabled,
			Value = true
		};

		SetDebugSettingEvent FreeBuildingPlacementEnabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.FreeBuildingPlacementEnabled,
			Value = true
		};

		SetDebugSettingEvent SunDamageDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.SunDamageDisabled,
			Value = true
		};

		SetDebugSettingEvent LightningStrikesDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.LightningStrikesDisabled,
			Value = true
		};

		var debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
		debugEventsSystem.SetDebugSetting(0, ref DayNightCycleDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref CastleHeartBloodEssenceDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref DropsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref BloodDrainDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref DurabilityDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref DynamicCloudsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref BuildingPlacementRestrictionsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref CastleHeartConnectionRequirementDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref FreeBuildingPlacementEnabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref SunDamageDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref LightningStrikesDisabledSetting);
	}

	private static bool HasLoaded()
	{
		// Hack, check to make sure that entities loaded enough because this function
		// will be called when the plugin is first loaded, when this will return 0
		// but also during reload when there is data to initialize with.
		var collectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
		return collectionSystem?.SpawnableNameToPrefabGuidDictionary.Count > 0;
	}
}
