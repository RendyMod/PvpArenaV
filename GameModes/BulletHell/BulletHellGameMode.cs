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
	public Player player = null;
	public bool HasStarted = false;
	public List<Timer> Timers = new List<Timer>();
	public Stopwatch stopwatch = new Stopwatch();
	public int ArenaNumber = 0;
	public CircleZone FightZone;
	public TemplateUnitSpawn UnitSpawns = new TemplateUnitSpawn();

	public BulletHellGameMode(CircleZone fightZone, TemplateUnitSpawn unitSpawns, int arenaNumber)
	{
		FightZone = fightZone;
		UnitSpawns = unitSpawns;
		ArenaNumber = arenaNumber;
	}

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
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
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
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
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
	}

	private static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{

	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

		ResetPlayer(player);
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			buffEntity.Add<BuffModificationFlagData>();
			buffEntity.Write(BuffModifiers.PigModifications);
		}
		BulletHellManager.EndMatch(this);
		//end match and tp to training + report time and record score
	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

		var pos = player.Position;
		Helper.RespawnPlayer(player, pos);
		player.Reset();
		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		BulletHellManager.EndMatch(this);
		//end match and tp to training + report time and record score
	}
	/*public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}*/
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3()); //replace this with training tp
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

		player.CurrentState = Player.PlayerState.Normal;
		//end the match
	}

	public override void HandleOnItemWasThrown(Player player, Entity eventEntity)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!player.IsInBulletHell() || player != this.player) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
	}

	private bool IsOutOfBounds()
	{
		return !FightZone.Contains(player);
	}

	public void HandleOnGameFrameUpdate()
	{
		if (HasStarted && IsOutOfBounds())
		{
			player.ReceiveMessage("You have gone out of bounds!".Error());
			BulletHellManager.EndMatch(this);
		}
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

