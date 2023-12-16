using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ProjectM;
using PvpArena.GameModes;
using Unity.Collections;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(AggroSystem), nameof(AggroSystem.OnUpdate))]
public static class AggroSystemPatch
{
	public static void Postfix(AggroSystem __instance)
	{
		var entities = __instance.__SortAndSetTarget_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			GameEvents.RaiseAggroPostUpdate(entity);
		}
		entities.Dispose();
	}
}
