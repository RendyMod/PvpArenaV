using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using PvpArena.Helpers;
using PvpArena.Factories;
using Unity.Collections;
using ProjectM.Shared;
using PvpArena.Data;
using static PvpArena.Helpers.Helper;

namespace PvpArena.GameModes.Troll;

public class TrollGameMode : DefaultGameMode
{
	public HashSet<Player> Players = new();
	public override Player.PlayerState PlayerGameModeType => Player.PlayerState.Troll;
    public override string UnitGameModeType => "troll";

    public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
    {
        RemoveConsumables = true,
        RemoveShapeshifts = false,
		BuffsToIgnore = new HashSet<PrefabGUID> { Helper.TrollBuff, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff }
    };

	private static List<ModifyUnitStatBuff_DOTS> TrollMods = new List<ModifyUnitStatBuff_DOTS>
	{
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.AttackSpeed,
			Value = 5
		},
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.CooldownModifier,
			Value = 0
		},
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.MovementSpeed,
			Value = 20
		}
		
		//
	};

    public TrollGameMode() : base()
	{
		Initialize();
	}

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
		GameEvents.OnPlayerProjectileCreated += HandleOnPlayerProjectileCreated;
		GameEvents.OnPlayerHitColliderCastCreated += HandleOnPlayerHitColliderCastCreated;
		GameEvents.OnPlayerAoeCreated += HandleOnPlayerAoeCreated;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
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
		GameEvents.OnPlayerProjectileCreated -= HandleOnPlayerProjectileCreated;
		GameEvents.OnPlayerAoeCreated -= HandleOnPlayerAoeCreated;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerPurchasedItem -= HandleOnPlayerPurchasedItem;

		Players.Clear();
	}

	private static HashSet<string> AllowedCommands = new HashSet<string>
	{
		{ "all" }
	};

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
	
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		eventEntity.Destroy();
	}

	public override void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		var prefabGuid = buffEntity.Read<PrefabGUID>();
		if (prefabGuid == Helper.TrollBuff)
		{
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.ImmuneToHazards | BuffModificationTypes.Invulnerable | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision);
			var buffer = buffEntity.AddBuffer<ModifyUnitStatBuff_DOTS>();
			buffer.Clear();
			foreach (var mod in TrollMods)
			{
				buffer.Add(mod);
			}
		}
	}

    public void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity)
    {
        if (player.CurrentState != this.PlayerGameModeType) return;
		if (buffEntity.Read<PrefabGUID>() == Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff)
        {
            buffEntity.Remove<DestroyTag>();
            buffEntity.Write(new DestroyState
            {
                Value = DestroyStateEnum.NotDestroyed
            });
            var destroyReason = buffEntity.Read<DestroyData>().DestroyReason;
            if (destroyReason != DestroyReason.Default)
            { 
                TrollModeManager.RemoveTroll(player);
            }
        }
    }

	public void HandleOnPlayerProjectileCreated(Player player, Entity projectileEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		var projectile = projectileEntity.Read<Projectile>();
		projectile.Range = 100;
		projectileEntity.Write(projectile);
	}

	public void HandleOnPlayerHitColliderCastCreated(Player player, Entity hitCastCollider)
	{
		var buffer = hitCastCollider.ReadBuffer<HitColliderCast>();
		for (var i = 0; i < buffer.Length; i++)
		{
			var hitColliderCast = buffer[i];
			hitColliderCast.IgnoreImmaterial = true;
			buffer[i] = hitColliderCast;
		}
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		if (eventEntity.Exists())
		{
			eventEntity.Destroy();
		}
	}

	public void HandleOnPlayerAoeCreated(Player player, Entity aoeEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

		var buffer = aoeEntity.ReadBuffer<HitColliderCast>();
		for (var i = 0; i < buffer.Length; i++)
		{
			var hitColliderCast = buffer[i];
			hitColliderCast.IgnoreImmaterial = true;
			buffer[i] = hitColliderCast;
		}

		var lifeTime = aoeEntity.Read<LifeTime>();
		lifeTime.Duration = 0;
		aoeEntity.Write(lifeTime);
	}

	public override void HandleOnPlayerReset(Player player)
	{
		
	}
}

