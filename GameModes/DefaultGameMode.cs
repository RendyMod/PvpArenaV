using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using ProjectM.Network;
using PvpArena.Data;
using Bloodstone.API;
using PvpArena.Configs;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;
using ProjectM.Gameplay.Systems;
using ProjectM.CastleBuilding;
using PvpArena.Services;

namespace PvpArena.GameModes;

public class DefaultGameMode : BaseGameMode
{
	private static List<PrefabGUID> ShapeshiftsToModify = new List<PrefabGUID>
	{
		Prefabs.AB_Shapeshift_Wolf_Buff,
		Prefabs.AB_Shapeshift_Wolf_Skin01_Buff,
		Prefabs.AB_Shapeshift_Bear_Buff,
		Prefabs.AB_Shapeshift_Bear_Skin01_Buff,
		Prefabs.AB_Shapeshift_Human_Buff,
		Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff,
		Prefabs.AB_Shapeshift_Rat_Buff,
		Prefabs.AB_Shapeshift_Toad_Buff
	};

	private static Dictionary<PrefabGUID, PrefabGUID> Shapeshifts = new Dictionary<PrefabGUID, PrefabGUID>
	{
		{ Prefabs.AB_Shapeshift_Wolf_Group, Prefabs.AB_Shapeshift_Wolf_Buff },
		{ Prefabs.AB_Shapeshift_Wolf_Skin01_Group, Prefabs.AB_Shapeshift_Wolf_Skin01_Buff },
		{ Prefabs.AB_Shapeshift_Bear_Group, Prefabs.AB_Shapeshift_Bear_Buff },
		{ Prefabs.AB_Shapeshift_Bear_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Buff },
		{ Prefabs.AB_Shapeshift_Rat_Group, Prefabs.AB_Shapeshift_Rat_Buff },
		{ Prefabs.AB_Shapeshift_Human_Group, Prefabs.AB_Shapeshift_Human_Buff },
		{ Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Group, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff },
		{ Prefabs.AB_Shapeshift_Toad_Group, Prefabs.AB_Shapeshift_Toad_Buff },
		{ Prefabs.AB_Shapeshift_Bat_Group, Prefabs.AB_Shapeshift_Human_Buff },
		{ Prefabs.AB_Shapeshift_BloodMend_Group, Prefabs.AB_Shapeshift_Human_Buff },
	};

	private static List<PrefabGUID> Wolves = new List<PrefabGUID> { Prefabs.AB_Shapeshift_Wolf_Buff, Prefabs.AB_Shapeshift_Wolf_Skin01_Buff };
	private static List<PrefabGUID> Bears = new List<PrefabGUID> { Prefabs.AB_Shapeshift_Bear_Buff, Prefabs.AB_Shapeshift_Bear_Skin01_Buff };
	private static List<PrefabGUID> Frog = new List<PrefabGUID> { Prefabs.AB_Shapeshift_Toad_Buff };
	private static List<PrefabGUID> AllShapeshiftBuffs = new List<PrefabGUID>
	{
		Prefabs.AB_Shapeshift_Wolf_Buff,
		Prefabs.AB_Shapeshift_Wolf_Skin01_Buff,
		Prefabs.AB_Shapeshift_Bear_Buff,
		Prefabs.AB_Shapeshift_Bear_Skin01_Buff,
		Prefabs.AB_Shapeshift_Human_Buff,
		Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff,
		Prefabs.AB_Shapeshift_Rat_Buff,
		Prefabs.AB_Shapeshift_Toad_Buff
	};
	private static Dictionary<PrefabGUID, List<PrefabGUID>> AbilitiesToNotCauseBuffDestruction = new Dictionary<PrefabGUID, List<PrefabGUID>>
	{
		{ Prefabs.AB_Shapeshift_Wolf_Bite_Group, Wolves },
		{ Prefabs.AB_Shapeshift_Wolf_Howl_AbilityGroup, Wolves },
		{ Prefabs.AB_Shapeshift_Wolf_Leap_Travel_AbilityGroup, Wolves },
		{ Prefabs.AB_Shapeshift_Bear_Roar_AbilityGroup, Bears },
		{ Prefabs.AB_Shapeshift_Bear_MeleeAttack_Group, Bears },
		{ Prefabs.AB_Bear_Shapeshift_AreaAttack_Group, Bears },
		{ Prefabs.AB_Shapeshift_Toad_Leap_Travel_AbilityGroup, Frog },
		{ Prefabs.AB_Interact_OpenGate_AbilityGroup, AllShapeshiftBuffs },
		{ Prefabs.AB_Interact_OpenDoor_AbilityGroup, AllShapeshiftBuffs }
	};

	private static Dictionary<PrefabGUID, bool> AbilitiesToInterrupt = new Dictionary<PrefabGUID, bool>
	{
		{ Prefabs.AB_Shapeshift_Bear_MeleeAttack_Group, true },
		{ Prefabs.AB_Bear_Shapeshift_AreaAttack_Group, true },
	};

	public override void Initialize()
	{
		/*GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown += HandleOnItemWasThrown;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDamageReported += HandleOnPlayerDamageReported;
	}
	public override void Dispose()
	{
		/*GameEvents.OnPlayerRespawn -= HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown -= HandleOnItemWasThrown;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDamageReported -= HandleOnPlayerDamageReported;
	}

	private static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{
		{ "all", true },
	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (!player.IsInDefaultMode()) return;

		ResetPlayer(player);
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			buffEntity.Add<BuffModificationFlagData>();
			buffEntity.Write(BuffModifiers.PigModifications);
		}

		if (killer.Has<PlayerCharacter>())
		{
			var KillerPlayer = PlayerService.GetPlayerFromCharacter(killer);

			if (player.ConfigOptions.SubscribeToKillFeed)
			{
				player.ReceiveMessage($"You were killed by {KillerPlayer.Name.Error()}.".White());
			}
			if (KillerPlayer.ConfigOptions.SubscribeToKillFeed)
			{
				KillerPlayer.ReceiveMessage($"You killed {player.Name.Success()}!".White());
			}
		}
	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (!player.IsInDefaultMode()) return;

		var pos = player.Position;
		Helper.RespawnPlayer(player, pos);
		Helper.Reset(player);
		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
	}
	/*public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}*/
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (!player.IsInDefaultMode()) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (!player.IsInDefaultMode()) return;


		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_BloodMend_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			Helper.Reset(player);
		}
		else if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_ShareBlood_ExposeVein_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			Helper.ToggleBloodOnPlayer(player);
		}
		else if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_BloodHunger_BloodSight_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			Helper.ToggleBuffsOnPlayer(player);
			
		}
		else
		{
			foreach (var shapeshift in Shapeshifts)
			{
				if (enterShapeshiftEvent.Shapeshift == shapeshift.Key)
				{
					var abilityBarShared = player.Character.Read<AbilityBar_Shared>();
					AbilityUtilitiesServer.TryInstantiateAbilityGroup(VWorld.Server.EntityManager, Core.prefabCollectionSystem.PrefabLookupMap, player.Character, shapeshift.Key, false, out Entity abilityGroupEntity);
					abilityBarShared.ServerInterruptCounter++;
					player.Character.Write(abilityBarShared);
					VWorld.Server.EntityManager.DestroyEntity(eventEntity);
					if (Helper.BuffPlayer(player, shapeshift.Value, out var buffEntity, Helper.NO_DURATION, false)) //in order to skip the slow cast / slow movement we just apply the shapeshift buff directly
					{
						var abilityOwner = buffEntity.Read<AbilityOwner>();
						abilityOwner.Ability = abilityBarShared.CastAbility;
						abilityOwner.AbilityGroup = abilityGroupEntity;
						buffEntity.Write(abilityOwner); //shapeshift buffs forcibly applied without casting have an annoying white icon unless you set this

						break;
					} 					
				}
			}
		}
	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (!player.IsInDefaultMode()) return;

	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (!player.IsInDefaultMode()) return;

		var abilityCastStartedEvent = eventEntity.Read<AbilityCastStartedEvent>();
		if (abilityCastStartedEvent.AbilityGroup.Index <= 0) return;

		var abilityGuid = abilityCastStartedEvent.AbilityGroup.Read<PrefabGUID>();

		//interrupt bear attacks
		if (AbilitiesToInterrupt.ContainsKey(abilityGuid))
		{
			player.Interrupt();
		}

		//prevent shapeshift spells from breaking players out of their shapeshift (due to the abnormal way we gave them shapeshift)
		if (AbilitiesToNotCauseBuffDestruction.TryGetValue(abilityGuid, out var buffs))
		{
			foreach (var buff in buffs)
			{
				if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, abilityCastStartedEvent.Character, buff, out Entity buffEntity))
				{
					if (buffEntity.Has<DestroyOnAbilityCast>())
					{
						var destroyOnAbilityCast = buffEntity.Read<DestroyOnAbilityCast>();
						destroyOnAbilityCast.CastCount = 0;
						buffEntity.Write(destroyOnAbilityCast);
						break;
					}
				}
			}
		}
	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (!player.IsInDefaultMode()) return;

		var prefabGuid = buffEntity.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.Witch_PigTransformation_Buff)
		{
			var Buffer = VWorld.Server.EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
			Buffer.Clear();
			Buffer.Add(BuffModifiers.ShapeshiftFastMoveSpeed);
			buffEntity.Add<BuffModificationFlagData>();
			buffEntity.Write(BuffModifiers.PigModifications);
		}
		if (ShapeshiftsToModify.Contains(prefabGuid))
		{
			var buffer = buffEntity.ReadBuffer<ModifyUnitStatBuff_DOTS>();
			buffer.Add(BuffModifiers.OnDeathFastMoveSpeed);

			if (buffEntity.Has<ModifyMovementSpeedBuff>())
			{
				var modifyMovementSpeedBuff = buffEntity.Read<ModifyMovementSpeedBuff>();
				modifyMovementSpeedBuff.MoveSpeed = 1f;
			}
			buffEntity.Add<DisableAggroBuff>();
			buffEntity.Write(new DisableAggroBuff
			{
				Mode = DisableAggroBuffMode.OthersDontAttackTarget
			});
			buffEntity.Add<DestroyOnAbilityCast>();


			if (prefabGuid != Prefabs.AB_Shapeshift_Rat_Buff)
			{
				buffEntity.Add<BuffModificationFlagData>();
				buffEntity.Write(BuffModifiers.DefaultShapeshiftModifications);
			}
		}
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.IsInDefaultMode()) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}

	public override void HandleOnItemWasThrown(Player player, Entity eventEntity)
	{
		if (!player.IsInDefaultMode()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!player.IsInDefaultMode()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		if (damageDealtEvent.Target.Has<PlayerCharacter>())
		{
			DamageRecorderService.RecordDamageDealtInitial(player, damageDealtEvent);
		}
		
		var isStructure = (damageDealtEvent.Target.Has<CastleHeartConnection>());
		if (isStructure)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
	}

	public void HandleOnPlayerDamageReported(Player source, Player target, PrefabGUID type, float damageDealt)
	{
		DamageRecorderService.RecordDamageDoneFinal(source, damageDealt, type);
	}

	public override void ResetPlayer(Player player)
	{
		player.Reset();
	}

	public static new Dictionary<string, bool> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

