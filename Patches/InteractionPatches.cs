using HarmonyLib;
using ProjectM;
using Unity.Collections;
using ProjectM.Gameplay.Systems;
using System;
using PvpArena.GameModes;
using PvpArena.Services;

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
				if (entity.Has<PlayerCharacter>())
				{
					var playerCharacter = entity.Read<PlayerCharacter>();
					if (playerCharacter.UserEntity.Exists())
					{
						var player = PlayerService.GetPlayerFromCharacter(entity);
						GameEvents.RaisePlayerInteracted(player, interactor);
					}
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}
}

