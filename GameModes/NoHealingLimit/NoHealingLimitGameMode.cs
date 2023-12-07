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

public class NoHealingLimitGameMode : DefaultGameMode
{
	public static HashSet<Player> Players = new HashSet<Player>();
	public override Player.PlayerState GameModeType => Player.PlayerState.NoHealingLimit;

	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = true,
		RemoveShapeshifts = true,
		BuffsToIgnore = new HashSet<PrefabGUID> { Helper.CustomBuff4 }
	};

	private static List<ModifyUnitStatBuff_DOTS> NoHealingLimitMods = new List<ModifyUnitStatBuff_DOTS>
	{
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.SpellLifeLeech,
			Value = .3f
		},
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.PhysicalLifeLeech,
			Value = .3f
		},
		new ModifyUnitStatBuff_DOTS
		{
			Id = ModificationIdFactory.NewId(),
			ModificationType = ModificationType.Set,
			Priority = 100,
			StatType = UnitStatType.PrimaryLifeLeech,
			Value = .3f
		}
		//
	};

	public override void Initialize()
	{
		base.Initialize();
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
	}
	public override void Dispose()
	{
		base.Dispose();
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
		foreach (var player in Players)
		{
			player.CurrentState = Player.PlayerState.Normal;
		}
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

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		base.HandleOnPlayerBuffed(player, buffEntity);

		if (buffEntity.Read<PrefabGUID>() == Helper.CustomBuff4)
		{
			var buffer = buffEntity.AddBuffer<ModifyUnitStatBuff_DOTS>();
			buffer.Clear();
			foreach (var mod in NoHealingLimitMods)
			{
				buffer.Add(mod);
			}
		}
	}

	public void HandleOnGameFrameUpdate()
	{
		foreach (var player in Players)
		{
			if (player.CurrentState != GameModeType) return;
			var health = player.Character.Read<Health>();
			health.MaxRecoveryHealth = health.MaxHealth;
			player.Character.Write(health);
		}
		
	}
}

