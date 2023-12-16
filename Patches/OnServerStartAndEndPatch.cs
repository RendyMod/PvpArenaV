using HarmonyLib;
using ProjectM;
using PvpArena.Services;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnGameDataInitialized))]
public static class InitializationPatch1
{
	[HarmonyPostfix]
	public static void OneShot_AfterLoad_InitializationPatch1()
	{
		var action = () => Plugin.OnServerStart();
		ActionScheduler.RunActionOnceAfterDelay(action, 6);
		Plugin.Harmony.Unpatch(typeof(ServerBootstrapSystem).GetMethod("OnGameDataInitialized"), typeof(InitializationPatch1).GetMethod("OneShot_AfterLoad_InitializationPatch1"));
	}
}
