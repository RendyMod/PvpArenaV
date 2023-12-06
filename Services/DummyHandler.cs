using ProjectM;
using Unity.Entities;
using PvpArena.GameModes;
using PvpArena.Helpers;
using PvpArena.Factories;
using static ProjectM.DeathEventListenerSystem;

namespace PvpArena.Services;


public static class DummyHandler
{

	public static void Initialize()
	{
		GameEvents.OnUnitBuffRemoved += HandleOnUnitBuffRemoved;
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
	}

	public static void Dispose()
	{
		GameEvents.OnUnitBuffRemoved -= HandleOnUnitBuffRemoved;
		GameEvents.OnUnitDeath -= HandleOnUnitDeath;
	}

	private static void HandleOnUnitBuffRemoved(Entity unit, Entity buffEntity)
	{
		if (buffEntity.Read<PrefabGUID>() == Helper.CustomBuff4 && UnitFactory.HasCategory(unit, "dummy"))
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
				Plugin.PluginLog.LogInfo("healing dummy");
			}
		}
	}

	private static void HandleOnUnitDeath(Entity victim, DeathEvent deathEvent)
	{
		if (UnitFactory.HasCategory(victim, "dummy"))
		{
			var spawnPosition = UnitFactory.GetSpawnPositionOfEntity(victim);
			if (spawnPosition.x != 0 && spawnPosition.y != 0 && spawnPosition.z != 0)
			{
				Dummy dummy = new Dummy(victim.Read<PrefabGUID>(), victim.Read<AggroConsumer>().Active.Value);
				UnitFactory.SpawnUnit(dummy, spawnPosition);
				Plugin.PluginLog.LogInfo("spawning dummy");
			}
		}
	}
}

