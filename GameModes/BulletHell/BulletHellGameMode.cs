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
using System.Threading;
using System.Diagnostics;
using static PvpArena.Configs.ConfigDtos;
using PvpArena.Factories;

namespace PvpArena.GameModes.BulletHell;

public class BulletHellGameMode : BaseGameMode
{
	public override Player.PlayerState GameModeType => Player.PlayerState.BulletHell;
    public Player player = null;
	public bool HasStarted = false;
	public List<Timer> Timers = new List<Timer>();
	public Stopwatch stopwatch = new Stopwatch();
	public int ArenaNumber = 0;
	public CircleZone FightZone;
	public TemplateUnitSpawn UnitSpawns = new TemplateUnitSpawn();
	private long lastReportedInterval = 0;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
    {
        RemoveConsumables = true,
        RemoveShapeshifts = true,
    };

    public BulletHellGameMode(CircleZone fightZone, TemplateUnitSpawn unitSpawns, int arenaNumber)
	{
		FightZone = fightZone;
		UnitSpawns = unitSpawns;
		ArenaNumber = arenaNumber;
	}

	public override void Initialize()
	{
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnUnitProjectileCreated += HandleOnUnitProjectileCreated;
		GameEvents.OnUnitAoeCreated += HandleOnUnitAoeCreated;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
	}
	public override void Dispose()
	{
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnUnitProjectileCreated -= HandleOnUnitProjectileCreated;
		GameEvents.OnUnitAoeCreated -= HandleOnUnitAoeCreated;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;

		HasStarted = false;
		stopwatch.Reset();
		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
		player = null;
		lastReportedInterval = 0;
	}

	private static HashSet<string> AllowedCommands = new HashSet<string>
    {

	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

		player.Reset(ResetOptions);
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			buffEntity.Add<BuffModificationFlagData>();
			buffEntity.Write(BuffModifiers.PigModifications);
		}
		BulletHellManager.EndMatch(this);
		//end match and tp to training + report time and record score
	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != GameModeType) return;

		eventEntity.Destroy();
		player.ReceiveMessage("You can't drink those during a bullet hell match!".Error());
	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

        BulletHellManager.EndMatch(this);
        base.HandleOnPlayerDisconnected(player);
	}

	private bool IsOutOfBounds()
	{
		return !FightZone.Contains(player);
	}

	public void HandleOnGameFrameUpdate()
	{
		if (HasStarted)
		{
			if (!IsOutOfBounds())
			{
				long elapsedTimeInSeconds = stopwatch.ElapsedMilliseconds / 1000;
				long nextInterval = lastReportedInterval + 5;

				if (elapsedTimeInSeconds >= nextInterval)
				{
					var pos = player.Position;
					pos.y += 2;
					Helper.MakeSCT(player, Prefabs.SCT_Type_InfoMessage, nextInterval, pos);
					lastReportedInterval = nextInterval;
				}
				
			}
			else
			{
				player.ReceiveMessage("You have gone out of bounds!".Error());
				BulletHellManager.EndMatch(this);
			}
			
		}

	}

	public void HandleOnUnitProjectileCreated(Entity unit, Entity projectile)
	{
		if (!UnitFactory.HasCategory(unit, "bullethell")) { return; }

		var prefabGuid = projectile.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.AB_Sorceress_Projectile)
		{
			var buffer = projectile.ReadBuffer<HitColliderCast>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var hitCollider = buffer[i];
				hitCollider.IgnoreImmaterial = true;
				buffer[i] = hitCollider;
			}
		}
	}

	public void HandleOnUnitAoeCreated(Entity unit, Entity aoe)
	{
		if (!UnitFactory.HasCategory(unit, "bullethell")) { return; }

		var prefabGuid = aoe.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.AB_Sorceress_AoE_Throw)
		{
			var buffer = aoe.ReadBuffer<HitColliderCast>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var hitColliderCast = buffer[i];
				hitColliderCast.IgnoreImmaterial = true;
				buffer[i] = hitColliderCast;
			}
		}
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

