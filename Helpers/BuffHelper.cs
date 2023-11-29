using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using Il2CppSystem.Runtime.Remoting;
using Newtonsoft.Json.Utilities;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;

namespace PvpArena.Helpers;

public static partial class Helper
{
	public const int NO_DURATION = 0;
	public const int DEFAULT_DURATION = -1;

	private static PrefabGUID AbilityImpairBuff = Prefabs.Gloomrot_Voltage_VBlood_Emote_OnAggro_Buff;
	public static PrefabGUID CustomBuff = Prefabs.Buff_InCombat_Manticore; //good blank slate for adding modifiers, but doesn't persist through death
	public static PrefabGUID CustomBuff2 = Prefabs.Buff_Manticore_ImmaterialHomePos; //good blank slate for adding modifiers, but doesn't persist through death
	public static void ApplyMatchInitializationBuff(Player player)
	{
		Helper.BuffPlayer(player, Prefabs.Buff_General_Gloomrot_LightningStun, out var buffEntity, 5);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);
	}

	public static void ApplyFastAfterRespawnBuff(Player player)
	{
		Helper.BuffPlayer(player, Prefabs.Buff_General_Phasing, out var buffEntity, 3);
		ApplyStatModifier(buffEntity, BuffModifiers.FastRespawnMoveSpeed);
		Helper.RemoveBuffModifications(buffEntity, BuffModificationTypes.Immaterial);
	}

	public static void ApplyLoserMatchEndBuff(Player player)
	{
		Helper.BuffPlayer(player, Prefabs.AB_Emote_Vampire_Surrender_Buff, out var buffEntity, 6);
		buffEntity.Remove<DestroyBuffOnMove>();
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);	
	}

	public static void ApplyWinnerMatchEndBuff(Player player)
	{
		Helper.BuffPlayer(player, Prefabs.AB_Emote_Vampire_Point_Buff, out var buffEntity, 6);
		buffEntity.Remove<DestroyBuffOnMove>();
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);
		Helper.BuffPlayer(player, Prefabs.AB_Werewolf_Howl_Buff, out buffEntity, 6);
	}

	public static void ModifyBuff(Entity buffEntity, BuffModificationTypes buffModificationTypes, bool overwrite = false)
	{
		buffEntity.Add<BuffModificationFlagData>();
		var buffModificationFlagData = buffEntity.Read<BuffModificationFlagData>();
		if (overwrite)
		{
			buffModificationFlagData.ModificationTypes = (long)BuffModificationTypes.None;
		}
		buffModificationFlagData.ModificationTypes |= (long)buffModificationTypes;
		buffEntity.Write(buffModificationFlagData);
	}

	public static void RemoveBuffModifications(Entity buffEntity, BuffModificationTypes buffModificationTypes)
	{
		buffEntity.Add<BuffModificationFlagData>();
		var buffModificationFlagData = buffEntity.Read<BuffModificationFlagData>();
		buffModificationFlagData.ModificationTypes &= ~(long)buffModificationTypes;
		buffEntity.Write(buffModificationFlagData);
	}

	public static void ApplyAbilityImpairBuff(this Player player, float duration = DEFAULT_DURATION, bool persistsThroughDeath = false)
	{
		Helper.BuffPlayer(player, AbilityImpairBuff, out var buffEntity, duration, persistsThroughDeath);
	}

	public static void Interrupt(this Player player)
	{
		ApplyAbilityImpairBuff(player, .05f);
	}


	public static void ApplyStatModifier(Entity buffEntity, ModifyUnitStatBuff_DOTS statMod)
	{
		var buffer = VWorld.Server.EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
		buffer.Clear();
		buffer.Add(statMod);
	}

	public static List<PrefabGUID> GetEntityBuffs(Entity entity)
	{
		List<PrefabGUID> entityBuffs = new List<PrefabGUID>();
		if (entity.Has<BuffBuffer>())
		{
			var buffs = entity.ReadBuffer<BuffBuffer>();
			foreach (var buff in buffs)
			{
				if (buff.Entity.Read<EntityOwner>().Owner != entity)
				{
					entityBuffs.Add(buff.PrefabGuid);
				}
			}
		}

		var buffEntities = Helper.GetEntitiesByComponentTypes<Buff, PrefabGUID>();
		foreach (var buffEntity in buffEntities)
		{
			if (buffEntity.Read<EntityOwner>().Owner == entity)
			{
				entityBuffs.Add(buffEntity.Read<PrefabGUID>());
			}
		}
		return entityBuffs;
	}

	public static bool HasBuff(Entity entity, PrefabGUID buff)
	{
		return BuffUtility.HasBuff(VWorld.Server.EntityManager, entity, buff);
	}

	public static bool HasBuff(this Player player, PrefabGUID buff)
	{
		return HasBuff(player.Character, buff);
	}

	public static bool TryGetBuff(Entity entity, PrefabGUID buff, out Entity buffEntity)
	{
		return BuffUtility.TryGetBuff(VWorld.Server.EntityManager, entity, buff, out buffEntity);
	}

	public static bool TryGetBuff(Player player, PrefabGUID buff, out Entity buffEntity)
	{
		return TryGetBuff(player.Character, buff, out buffEntity);
	}

	public static void DestroyBuff(Entity buff)
	{
		DestroyUtility.Destroy(VWorld.Server.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);
	}

	public static void RemoveBuff(Entity unit, PrefabGUID buff)
	{
		if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, unit, buff, out var buffEntity))
		{
			DestroyBuff(buffEntity);
		}
	}

	public static void RemoveBuff(Player player, PrefabGUID buff)
	{
		RemoveBuff(player.Character, buff);
	}

	public static void RemoveAllShieldBuffs(Player player)
	{
		var buffer = player.Character.ReadBuffer<BuffBuffer>();
		foreach (var buff in buffer)
		{
			if (buff.Entity.Has<AbsorbBuff>())
			{
				DestroyBuff(buff.Entity);
			}
		}
	}

	public static void CompletelyRemoveAbilityBarFromBuff(Entity buffEntity)
	{
		var buffer = VWorld.Server.EntityManager.AddBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);
		for (var i = 0; i < 8; i++)
		{
			buffer.Add(new ReplaceAbilityOnSlotBuff
			{
				Slot = i,
				CastBlockType = GroupSlotModificationCastBlockType.WholeCast,
				NewGroupId = PrefabGUID.Empty,
				ReplaceGroupId = PrefabGUID.Empty,
				Priority = 100,
				Target = ReplaceAbilityTarget.BuffOwner
			});
		}
		buffEntity.Add<ReplaceAbilityOnSlotData>();
	}

	public static void RemoveNewAbilitiesFromBuff(Entity buffEntity)
	{
		var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
		buffer.Clear();
	}

	public static void FixIconForShapeshiftBuff(Player player, Entity buffEntity, PrefabGUID abilityGroupPrefab)
	{
		var abilityOwner = buffEntity.Read<AbilityOwner>();
		var abilityBarShared = player.Character.Read<AbilityBar_Shared>();
		AbilityUtilitiesServer.ValidateAbilityExists(VWorld.Server.EntityManager, Core.prefabCollectionSystem.PrefabLookupMap, player.Character, abilityGroupPrefab, out Entity abilityGroupEntity);
		abilityOwner.Ability = abilityBarShared.CastAbility;
		abilityOwner.AbilityGroup = abilityGroupEntity;
		buffEntity.Write(abilityOwner); //shapeshift buffs forcibly applied without casting have an annoying white icon unless you set this
	}

	public static bool BuffPlayer(Player player, PrefabGUID buff, out Entity buffEntity, float duration = DEFAULT_DURATION, bool attemptToPersistThroughDeath = false, bool effectsOnStart = false)
	{
		return BuffEntity(player.Character, buff, out buffEntity, duration, attemptToPersistThroughDeath, effectsOnStart);
	}

	public static bool BuffEntity(Entity entity, PrefabGUID buff, out Entity buffEntity, float duration = DEFAULT_DURATION, bool attemptToPersistThroughDeath = false, bool effectsOnStart = false)
	{
		var des = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
		var buffEvent = new ApplyBuffDebugEvent()
		{
			BuffPrefabGUID = buff
		};
		var fromCharacter = new FromCharacter()
		{
			User = PlayerService.UserCache.Keys.ElementAt(0),
			Character = entity
		};

		des.ApplyBuff(fromCharacter, buffEvent);
		if (TryGetBuff(entity, buff, out buffEntity))
		{
			if (!effectsOnStart)
			{
				if (buffEntity.Has<CreateGameplayEventsOnSpawn>())
				{
					buffEntity.Remove<CreateGameplayEventsOnSpawn>();
				}

				if (buffEntity.Has<GameplayEventListeners>())
				{
					buffEntity.Remove<GameplayEventListeners>();
				}
			}

			if (attemptToPersistThroughDeath)
			{
				buffEntity.Add<Buff_Persists_Through_Death>();
				if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
				{
					buffEntity.Remove<RemoveBuffOnGameplayEvent>();
				}

				if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
				{
					buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
				}
			}
			else
			{
				buffEntity.Remove<Buff_Persists_Through_Death>();
				buffEntity.Add<Buff_Destroy_On_Owner_Death>();
			}

			if (duration > 0 && duration != DEFAULT_DURATION)
			{
				if (buffEntity.Has<LifeTime>())
				{
					var lifetime = buffEntity.Read<LifeTime>();
					lifetime.Duration = duration;
					buffEntity.Write(lifetime);
				}
			}
			else if (duration == NO_DURATION)
			{
				if (buffEntity.Has<LifeTime>())
				{
					var lifetime = buffEntity.Read<LifeTime>();
					lifetime.Duration = 0; //duration must be -1 to stop timer from showing up, but -1 logs errors. 0 shows no errors and usually works, but will crash the client on some buffs like bloodrite
					lifetime.EndAction = LifeTimeEndAction.None;
					buffEntity.Write(lifetime);

				}

				if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
				{
					buffEntity.Remove<RemoveBuffOnGameplayEvent>();
				}

				if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
				{
					buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
				}
			}

			return true;
		}
		return false;
	}


	public static void ClearExtraBuffs(Entity unit, bool removeConsumables = false, bool removeShapeshifts = false, List<string> buffsToIgnore = default)
	{
		if (unit.Has<BuffBuffer>())
		{
			var buffs = unit.ReadBuffer<BuffBuffer>();
			var stringsToIgnore = new List<string>
			{
				"AB_BloodBuff",
				"SetBonus",
				"EquipBuff",
				"VBlood_Ability_Replace",
				"Buff_Gloomrot_SentryOfficer_TurretCooldown",
				"JumpFromCliffs",
				"General_Disconnected"
			};

			if (!removeConsumables && !removeShapeshifts)
			{
				stringsToIgnore.Add("Shapeshift");
			}

			if (!removeConsumables)
			{
				stringsToIgnore.Add("UseRelic");
				stringsToIgnore.Add("AB_Consumable");
				stringsToIgnore.Add("Buff_BloodMoon");
			}

			if (buffsToIgnore != null && buffsToIgnore.Count > 0)
			{
				stringsToIgnore.AddRange(buffsToIgnore);
			}

			foreach (var buff in buffs)
			{
				bool shouldRemove = true;
				foreach (string word in stringsToIgnore)
				{
					if (buff.PrefabGuid.LookupName().Contains(word))
					{
						shouldRemove = false;
						break;
					}
				}

				if (shouldRemove)
				{
					Helper.DestroyBuff(buff.Entity);
				}
			}
		}


		if (unit.Has<Equipment>())
		{
			var equipment = unit.Read<Equipment>();
			if (!equipment.IsEquipped(Prefabs.Item_Cloak_Main_ShroudOfTheForest, out var equipmentType) && Helper.HasBuff(unit, Prefabs.EquipBuff_ShroudOfTheForest))
			{
				Helper.RemoveBuff(unit, Prefabs.EquipBuff_ShroudOfTheForest);
			}
		}
	}

	public static void ClearConsumablesAndShards(Entity player)
	{
		ClearConsumables(player);
		ClearShards(player);
	}

	public static void ClearConsumables(Entity player)
	{
		var buffs = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
		var stringsToRemove = new List<string>
		{
			"Consumable",
		};

		foreach (var buff in buffs)
		{
			bool shouldRemove = false;
			foreach (string word in stringsToRemove)
			{
				if (buff.PrefabGuid.LookupName().Contains(word))
				{
					shouldRemove = true;
					break;
				}
			}

			if (shouldRemove)
			{
				DestroyUtility.Destroy(VWorld.Server.EntityManager, buff.Entity, DestroyDebugReason.TryRemoveBuff);
			}
		}
	}

	public static void ClearShards(Entity player)
	{
		var buffs = VWorld.Server.EntityManager.GetBuffer<BuffBuffer>(player);
		var stringsToRemove = new List<string>
		{
			"UseRelic",
		};

		foreach (var buff in buffs)
		{
			bool shouldRemove = false;
			foreach (string word in stringsToRemove)
			{
				if (buff.PrefabGuid.LookupName().Contains(word))
				{
					shouldRemove = true;
					break;
				}
			}

			if (shouldRemove)
			{
				DestroyUtility.Destroy(VWorld.Server.EntityManager, buff.Entity, DestroyDebugReason.TryRemoveBuff);
			}
		}
	}

	public static void MakeBuffCcImmune(Entity e)
	{
		e.Add<BuffResistances>();
		e.Write(new BuffResistances
		{
			SettingsEntity = ModifiableEntity.CreateFixed(Helper.GetPrefabEntityByPrefabGUID(Prefabs.BuffResistance_Golem)),
			InitialSettingGuid = Prefabs.BuffResistance_Golem
		});
	}
}
