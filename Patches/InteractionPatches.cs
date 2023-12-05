using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Gameplay.Systems;
using System;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(InteractSystemServer), nameof(InteractSystemServer.OnUpdate))]
public static class InteractSystemServerPatch
{
	public static void Prefix(InteractSystemServer __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var interactor = entity.Read<Interactor>();
				if (interactor.Target.Has<Mountable>() && !Team.IsAllies(interactor.Target.Read<Team>(), entity.Read<Team>()))
				{
					interactor.Target = entity;
					entity.Write(interactor);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

