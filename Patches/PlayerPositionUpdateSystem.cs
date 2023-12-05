using HarmonyLib;
using ProjectM;
using PvpArena.GameModes;
using PvpArena.Helpers;
using PvpArena.Services;
using Unity.Entities;

namespace PvpArena.Patches;



[HarmonyPatch(typeof(UserTranslationCopySystem), nameof(UserTranslationCopySystem.OnUpdate))]
public static class DetectPlayerEnteredZoneSystem
{
	private static long count = 0;
	public static void Prefix(UserTranslationCopySystem __instance)
	{
		if (count % 2 == 0)
		{
			GameEvents.RaiseGameFrameUpdate();
		}
		if (count % 30 == 0) //move this into game mode actions later
		{
			foreach (var player in PlayerService.OnlinePlayers.Keys)
			{
				if (!player.HasControlledEntity())
				{
					GameEvents.RaisePlayerHasNoControlledEntity(player);
					Helper.ControlOriginalCharacter(player);
				}
			}
		}
		count++;
	}
}


