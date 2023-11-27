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
using PvpArena.GameModes;

namespace PvpArena.Patches;



[HarmonyPatch(typeof(UserTranslationCopySystem), nameof(UserTranslationCopySystem.OnUpdate))]
public static class DetectPlayerEnteredZoneSystem
{
	private static int count = 0;
	public static void Prefix(UserTranslationCopySystem __instance)
	{
		if (count % 5 == 0)
		{
			GameEvents.RaiseGameFrameUpdate(); //every 5 frames should be good enough
		}
		
	}
}


