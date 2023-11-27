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

namespace PvpArena.Patches;


[HarmonyPatch(typeof(InteractSystemServer), nameof(InteractSystemServer.OnUpdate))]
public static class InteractSystemServerPatch
{
	public static void Prefix(InteractSystemServer __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var interactor = entity.Read<Interactor>();
			if (interactor.Target.Has<Mountable>() && !Team.IsAllies(interactor.Target.Read<Team>(), entity.Read<Team>()))
			{
				interactor.Target = entity;
				entity.Write(interactor);
			}
		}
	}
}

