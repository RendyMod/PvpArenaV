using HarmonyLib;
using ProjectM;
using PvpArena.Data;
using PvpArena.GameModes;
using PvpArena.Helpers;
using PvpArena.Services;
using Unity.Entities;

namespace PvpArena.Patches;



/*[HarmonyPatch(typeof(UserTranslationCopySystem), nameof(UserTranslationCopySystem.OnUpdate))]
public static class DetectPlayerEnteredZoneSystem
{
	private static long count = 0;
	public static void Postfix(UserTranslationCopySystem __instance)
	{
		GameEvents.RaiseGameFrameUpdate();
		
		if (count % 30 == 0) //move this into game mode actions later
		{
			foreach (var player in PlayerService.OnlinePlayers.Keys)
			{
				if (!player.HasControlledEntity())
				{
					GameEvents.RaisePlayerHasNoControlledEntity(player);
					Helper.ControlOriginalCharacter(player);
					Helper.RemoveBuff(player, Prefabs.Admin_Observe_Invisible_Buff);
				}
			}
		}
		count++;
	}
}*/


