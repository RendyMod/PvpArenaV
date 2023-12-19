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
using PvpArena.Factories;
using static DamageRecorderService;

namespace PvpArena.GameModes;

public class DefaultGameMode : BaseGameMode
{
	public override Player.PlayerState PlayerGameModeType => Player.PlayerState.Normal;
	public override string UnitGameModeType => "default";

	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveShapeshifts = false,
		RemoveConsumables = false,
		RemoveMinions = true,
		BuffsToIgnore = new HashSet<PrefabGUID>
		{
			Helper.TrollBuff
		}
	};

    public static Helper.ResetOptions TeamfightResetOptions { get; set; } = new Helper.ResetOptions
    {
        ResetCooldowns = true,
        RemoveShapeshifts = false,
        RemoveConsumables = false,
        RemoveMinions = false,
        BuffsToIgnore = new HashSet<PrefabGUID>
        {
            Helper.TrollBuff
        }
    };

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
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerDamageReported += HandleOnPlayerDamageReported;
		GameEvents.OnPlayerReset += HandleOnPlayerReset;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerPurchasedItem += HandleOnPlayerPurchasedItem;
	}
	public override void Dispose()
	{
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerDamageReported -= HandleOnPlayerDamageReported;
		GameEvents.OnPlayerReset -= HandleOnPlayerReset;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerPurchasedItem -= HandleOnPlayerPurchasedItem;
	}

	private static HashSet<string> AllowedCommands = new HashSet<string>
	{
		{ "all" },
	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

        if (player.Clan.Exists()) //if they might be in a teamfight then we don't want to remove summons just because they died
        {
            player.Reset(TeamfightResetOptions);
        }
		else
        {
            player.Reset(ResetOptions);
        }
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			Helper.ModifyBuff(buffEntity, BuffModifiers.PigModifications, true);

			var buffer = VWorld.Server.EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
			buffer.Clear();

			buffer.Add(BuffModifiers.ShapeshiftFastMoveSpeed);
			var buffer2 = buffEntity.ReadBuffer<CreateGameplayEventsOnDestroy>();
			buffer2.Clear();
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

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;


		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_BloodMend_Group)
		{
			eventEntity.Destroy();
			player.Reset(DefaultGameMode.ResetOptions);
		}
		else if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_ShareBlood_ExposeVein_Group)
		{
			eventEntity.Destroy();
			Helper.ToggleBloodOnPlayer(player);
		}
		else if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_BloodHunger_BloodSight_Group)
		{
			eventEntity.Destroy();
			Helper.ToggleBuffsOnPlayer(player);
		}
		else if (Shapeshifts.ContainsKey(enterShapeshiftEvent.Shapeshift))
		{
			foreach (var shapeshift in Shapeshifts)
			{
				if (enterShapeshiftEvent.Shapeshift == shapeshift.Key)
				{
					var abilityBarShared = player.Character.Read<AbilityBar_Shared>();
					AbilityUtilitiesServer.TryInstantiateAbilityGroup(VWorld.Server.EntityManager, Core.prefabCollectionSystem.PrefabLookupMap, player.Character, shapeshift.Key, false, out Entity abilityGroupEntity);
					abilityBarShared.ServerInterruptCounter++;
					player.Character.Write(abilityBarShared);
					eventEntity.Destroy();
					if (Helper.BuffPlayer(player, shapeshift.Value, out var buffEntity, Helper.NO_DURATION, false)) //in order to skip the slow cast / slow movement we just apply the shapeshift buff directly
					{
						Helper.FixIconForShapeshiftBuff(player, buffEntity, abilityGroupEntity.Read<PrefabGUID>());
						break;
					} 					
				}
			}
		}
		else
		{
			eventEntity.Destroy();
			player.ReceiveMessage("That form is disabled in this mode".Error());
		}
	}

	public virtual void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

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

	public virtual void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		var prefabGuid = buffEntity.Read<PrefabGUID>();
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

	public void HandleOnPlayerDamageReported(Player source, Entity target, PrefabGUID ability, DamageInfo damageInfo)
	{
		if (source.CurrentState != this.PlayerGameModeType) return;

		if (target.Has<PlayerCharacter>() || UnitFactory.HasGameMode(target, "dummy"))
		{
			DamageRecorderService.RecordDamageDone(source, ability, damageInfo);
		}
	}

	public virtual void HandleOnPlayerReset(Player player)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		var sctEntity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.ScrollingCombatTextMessage);
		ScrollingCombatTextMessage.Create(VWorld.Server.EntityManager, Core.entityCommandBufferSystem.CreateCommandBuffer(), sctEntity, 0, Prefabs.SCT_Type_MAX, player.Position, player.Character, player.Character);
		Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_NormalForm_Buff, out var buffEntity);
	}

	public override void HandleOnPlayerPurchasedItem(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		if (!eventEntity.Exists()) return;
		var purchaseEvent = eventEntity.Read<TraderPurchaseEvent>();
		Entity trader = Core.networkIdSystem._NetworkIdToEntityMap[purchaseEvent.Trader];
		if (UnitFactory.HasGameMode(trader, "moba")) //don't let regular players buy stuff from the moba traders, or they will waste their vpoints
		{
			eventEntity.Destroy();
		};
		var costBuffer = trader.ReadBuffer<TradeCost>();
		var cost = -1 * (costBuffer[purchaseEvent.ItemIndex].Amount);
		if (cost > 0)
		{
			if (player.PlayerPointsData.TotalPoints >= cost)
			{
				player.PlayerPointsData.TotalPoints -= cost;
				Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { player.PlayerPointsData });
				player.ReceiveMessage($"Purchased for {cost.ToString().Emphasize()} {"VPoints".Warning()}. New total points: {player.PlayerPointsData.TotalPoints.ToString().Warning()}".Success());
			}
			else
			{
				eventEntity.Destroy();
				player.ReceiveMessage($"Not enough {"VPoints".Warning()} to purchase! {player.PlayerPointsData.TotalPoints.ToString().Warning()} / {cost}".Error());
			}
		}
		
		base.HandleOnPlayerPurchasedItem(player, eventEntity);
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

