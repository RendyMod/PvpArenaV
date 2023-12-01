using HarmonyLib;
using ProjectM;
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


