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
		RemoveShapeshifts = false,
		BuffsToIgnore = new HashSet<PrefabGUID> {  }
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

