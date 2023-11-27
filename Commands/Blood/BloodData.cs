using System.Collections.Generic;
using PvpArena.Models;

namespace PvpArena.Data;
public static class BloodData
{
	public static List<PrefabData> BloodPrefabData = new List<PrefabData>()
	{
		new PrefabData(Prefabs.BloodType_Brute, "BloodType_Brute", "brute"),
		new PrefabData(Prefabs.BloodType_Creature, "BloodType_Creature", "creature"),
		new PrefabData(Prefabs.BloodType_Mutant, "BloodType_Mutant", "mutant"),
		new PrefabData(Prefabs.BloodType_None, "BloodType_None", "frailed"),
		new PrefabData(Prefabs.BloodType_Rogue, "BloodType_Rogue", "rogue"),
		new PrefabData(Prefabs.BloodType_Scholar, "BloodType_Scholar", "scholar"),
		new PrefabData(Prefabs.BloodType_Warrior, "BloodType_Warrior", "warrior"),
		new PrefabData(Prefabs.BloodType_Worker, "BloodType_Worker", "worker"),
	};
}
