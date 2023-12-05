using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Bloodstone.API;
using PvpArena.Data;
using System.Collections.Generic;
using ProjectM.Network;
using PvpArena.Services;
using PvpArena.GameModes;
using static ProjectM.CastleBuilding.CastleBlockSystem;
using UnityEngine.TextCore;
using PvpArena.Helpers;
using PvpArena.Models;
using System;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(ShapeshiftSystem), nameof(ShapeshiftSystem.OnUpdate))]
public static class ShapeshiftSystemPatch
{
	public static void Prefix(ShapeshiftSystem __instance)
	{
		if (__instance.__EnterShapeshiftJob_entityQuery.HasAnyMatches)
		{
			var entities = __instance.__EnterShapeshiftJob_entityQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				try
				{
					if (entity.Exists())
					{
						var fromCharacter = entity.Read<FromCharacter>();

						var Player = PlayerService.GetPlayerFromUser(fromCharacter.User);
						GameEvents.RaisePlayerShapeshifted(Player, entity);
					}
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo(e.ToString());
				}
			}
			entities.Dispose();
		}
	}
}
