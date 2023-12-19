using ProjectM;
using Unity.Entities;
using PvpArena.GameModes;
using PvpArena.Helpers;
using PvpArena.Factories;
using static ProjectM.DeathEventListenerSystem;
using System.Runtime.CompilerServices;
using PvpArena.Models;

namespace PvpArena.Services;


public static class DummyHandler
{

	public static void Initialize()
	{
		GameEvents.OnUnitBuffRemoved += HandleOnUnitBuffRemoved;
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
	}

	public static void Dispose()
	{
		GameEvents.OnUnitBuffRemoved -= HandleOnUnitBuffRemoved;
		GameEvents.OnUnitDeath -= HandleOnUnitDeath;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
	}

	private static void HandleOnUnitBuffRemoved(Entity unit, Entity buffEntity)
	{
		if (buffEntity.Read<PrefabGUID>() == Helper.CustomBuff2 && UnitFactory.HasGameMode(unit, "dummy"))
		{
			var health = unit.Read<Health>();
			var destroyReason = buffEntity.Read<DestroyData>().DestroyReason;
			if (health.Value > 0 && !health.IsDead && destroyReason == DestroyReason.Duration)
			{
				var spawnPosition = UnitFactory.GetSpawnPositionOfEntity(unit);
				unit.Teleport(spawnPosition);
				health.Value = health.MaxHealth;
				health.MaxRecoveryHealth = health.MaxHealth;
				unit.Write(health);
			}
		}
	}

	private static void HandleOnUnitDeath(Entity victim, DeathEvent deathEvent)
	{
		if (victim.Exists())
		{
			if (UnitFactory.HasGameMode(victim, "dummy"))
			{
				var spawnPosition = UnitFactory.GetSpawnPositionOfEntity(victim);
				if (spawnPosition.x != 0 && spawnPosition.y != 0 && spawnPosition.z != 0)
				{
					Dummy dummy = new Dummy(victim.Read<PrefabGUID>(), victim.Read<AggroConsumer>().Active.Value);
					UnitFactory.SpawnUnit(dummy, spawnPosition);
				}
			}
		}
	}

	private static void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!eventEntity.Exists()) return;
		var dealDamageEvent = eventEntity.Read<DealDamageEvent>();
		if (UnitFactory.HasGameMode(dealDamageEvent.Target, "dummy"))
		{
			if (Helper.TryGetBuff(dealDamageEvent.Target, Helper.CustomBuff2, out var buffEntity))
			{
				var age = buffEntity.Read<Age>();
				age.Value = 0;
				buffEntity.Write(age);
			}
			else
			{
				Helper.BuffEntity(dealDamageEvent.Target, Helper.CustomBuff2, out buffEntity, Dummy.ResetTime);
			}
		}
	}
}

