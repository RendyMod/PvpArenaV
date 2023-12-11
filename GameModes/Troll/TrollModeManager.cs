using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay.Scripting;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.GameModes.Domination;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Mathematics;
using UnityEngine;
using static PvpArena.Factories.UnitFactory;
using static PvpArena.Helpers.Helper;

namespace PvpArena.GameModes.Troll;

public static class TrollModeManager
{
	private static Dictionary<Player, TrollGameMode> trollGameModes = new Dictionary<Player, TrollGameMode>();


	public static void Dispose()
	{
		foreach (var kvp in trollGameModes)
		{
			RemoveTroll(kvp.Key);
		}
		trollGameModes.Clear();
	}

	public static void AddTroll(Player player)
	{
		player.CurrentState = Player.PlayerState.Troll;
		player.Reset(TrollGameMode.ResetOptions);
		trollGameModes[player] = new TrollGameMode(player);
		
		if (Helper.BuffPlayer(player, Helper.TrollBuff, out var buffEntity, Helper.NO_DURATION, true))
		{
			buffEntity.Add<DisableAggroBuff>();
			buffEntity.Write(new DisableAggroBuff
			{
				Mode = DisableAggroBuffMode.OthersDontAttackTarget
			});
			if (Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff, out var grandmaBuffEntity, Helper.NO_DURATION))
			{
				var scriptBuffShapeshiftDataShared = grandmaBuffEntity.Read<Script_Buff_Shapeshift_DataShared>();
				grandmaBuffEntity.Write(scriptBuffShapeshiftDataShared);
				grandmaBuffEntity.Remove<DestroyOnAbilityCast>();
				Helper.FixIconForShapeshiftBuff(player, grandmaBuffEntity, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Group);
				Helper.ChangeBuffResistances(grandmaBuffEntity, Prefabs.BuffResistance_UberMobNoKnockbackOrGrab);
				player.ReceiveMessage($"You have entered {"troll".Emphasize()} mode.".White());
			}
		}
	}

	public static void RemoveTroll(Player player)
	{
		player.CurrentState = Player.PlayerState.Normal;
		trollGameModes[player].Dispose();
		trollGameModes.Remove(player);
		Helper.RemoveBuff(player, Helper.TrollBuff);
		Helper.RemoveBuff(player, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff);
		player.ReceiveMessage($"You have left {"troll".Emphasize()} mode.".White());
	}
}
