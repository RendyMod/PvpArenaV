using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using PvpArena.Helpers;
using PvpArena.Factories;
using Unity.Collections;
using ProjectM.Shared;
using PvpArena.Data;

namespace PvpArena.GameModes.Troll;

public class TrollGameMode : DefaultGameMode
{
	Player player;
	public override Player.PlayerState GameModeType => Player.PlayerState.Troll;

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
			StatType = UnitStatType.SpellPower,
			Value = -100
		},
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.PhysicalPower,
			Value = -100
		},
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

    public TrollGameMode(Player player) : base()
	{
		this.player = player;
		Initialize();
	}

	public override void Initialize()
	{
		base.Initialize();
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerProjectileCreated += HandleOnPlayerHitColliderCreated;
        GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
        GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
    }
	public override void Dispose()
	{
		base.Dispose();
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerProjectileCreated -= HandleOnPlayerHitColliderCreated;
        GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
        GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
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
		if (player.CurrentState != this.GameModeType || player != this.player) return;

		eventEntity.Destroy();
	}

	public override void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType || player != this.player) return;

	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != this.GameModeType || player != this.player) return;

		if (buffEntity.Read<PrefabGUID>() == Helper.TrollBuff)
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
        if (player.CurrentState != this.GameModeType || player != this.player) return;

        if (buffEntity.Read<PrefabGUID>() == Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff)
        {
            TrollModeManager.RemoveTroll(player);
        }
    }

    public void HandleOnPlayerHitColliderCreated(Player player, Entity hitColliderEntity)
	{
        if (player.CurrentState != this.GameModeType || player != this.player) return;

        var buffer = hitColliderEntity.ReadBuffer<HitColliderCast>();
        for (var i = 0; i < buffer.Length; i++)
        {
            var hitColliderCast = buffer[i];
            hitColliderCast.IgnoreImmaterial = true;
            hitColliderCast.PrimaryFilterFlags = ProjectM.Physics.CollisionFilterFlags.Unit;
            buffer[i] = hitColliderCast;
        }

        if (hitColliderEntity.Has<Projectile>())
        {
            var projectile = hitColliderEntity.Read<Projectile>();
            projectile.Range = 100;
            hitColliderEntity.Write(projectile);
        }
		else if (hitColliderEntity.Has<TargetAoE>() && hitColliderEntity.Has<LifeTime>())
		{
			var lifeTime = hitColliderEntity.Read<LifeTime>();
			lifeTime.Duration = 0;
			hitColliderEntity.Write(lifeTime);
		}
	}
}

