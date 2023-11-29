using PvpArena.Commands;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Network;
using PvpArena.Matchmaking;
using PvpArena.Services;
using UnityEngine;
using ProjectM.Shared.Systems;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(AnnounceSiegeWeaponSystem), nameof(AnnounceSiegeWeaponSystem.OnUpdate))]
public static class AnnounceSiegeWeaponSystemPatch
{
	public static void Prefix(AnnounceSiegeWeaponSystem __instance)
	{
		var entities = __instance._AnnounceSiegeWeaponQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (entity.Has<DestroyOnSpawn>())
			{
				entity.Add<DestroyTag>(); //this is to prevent the message from going off in chat more than once when we manually announce the siege weapon
			}
		}
		entities.Dispose();
	}
}
