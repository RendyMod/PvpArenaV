using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using ProjectM.Shared.Systems;
using System;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(AnnounceSiegeWeaponSystem), nameof(AnnounceSiegeWeaponSystem.OnUpdate))]
public static class AnnounceSiegeWeaponSystemPatch
{
	public static void Prefix(AnnounceSiegeWeaponSystem __instance)
	{
		try
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
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo(e.ToString());
		}
	}
}
