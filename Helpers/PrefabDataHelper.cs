using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using PvpArena.Services;
using PvpArena.Models;
using PvpArena.Data;
using PvpArena.Configs;
using Il2CppSystem;
using Unity.Physics;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static bool TryGetPrefabGUIDFromString(string prefabNameOrId, out PrefabGUID prefabGUID)
	{
		if (Core.prefabCollectionSystem.NameToPrefabGuidDictionary.ContainsKey(prefabNameOrId))
		{
			prefabGUID = Core.prefabCollectionSystem.NameToPrefabGuidDictionary[prefabNameOrId];
			return true;
		}
		else
		{
			if (int.TryParse(prefabNameOrId, out int prefabGuidId))
			{
				var prefabGuid = new PrefabGUID(prefabGuidId);
				if (Core.prefabCollectionSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGuid))
				{
					prefabGUID = prefabGuid;
					return true;
				}
			}
		}

		prefabGUID = default;
		return false;
	}

	// Create a structure to store item and its matching score
	struct MatchItem
	{
		public int Score;
		public PrefabData PrefabData;
	}

	public static bool TryGetPrefabDataFromString(string needle, List<PrefabData> prefabDataList, out PrefabData matchedItem)
	{
		List<MatchItem> matchedItems = new List<MatchItem>();

		// Check for direct string match
		if (Core.prefabCollectionSystem.NameToPrefabGuidDictionary.TryGetValue(needle, out var prefabGUID))
		{
			matchedItem = prefabDataList.FirstOrDefault(item => item.PrefabGUID.Equals(prefabGUID));
			if (matchedItem != null)
			{
				return true;
			}
			else
			{
				matchedItem = new PrefabData(prefabGUID, prefabGUID.LookupName());
				return true;
			}
		}

		// Check for direct GUID match
		if (int.TryParse(needle, out int prefabGuidId))
		{
			var prefabGuid = new PrefabGUID(prefabGuidId);
			if (Core.prefabCollectionSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGuid))
			{
				matchedItem = prefabDataList.FirstOrDefault(item => item.PrefabGUID.Equals(prefabGuid));
				if (matchedItem != null)
					return true;
			}
		}

		foreach (var prefabData in prefabDataList)
		{
			int score = IsSubsequence(needle, prefabData.OverrideName.ToLower() + "s");
			if (score != -1)
			{
				matchedItems.Add(new MatchItem { Score = score, PrefabData = prefabData });
			}

			score = IsSubsequence(needle, prefabData.FormalPrefabName.ToLower() + "s");
			if (score != -1)
			{
				matchedItems.Add(new MatchItem { Score = score, PrefabData = prefabData });
			}

			if (int.TryParse(needle, out int result) && result == prefabData.PrefabGUID.GuidHash)
			{
				matchedItems.Add(new MatchItem { Score = score, PrefabData = prefabData });
			}
		}

		var bestMatch = matchedItems.OrderByDescending(m => m.Score).FirstOrDefault();
		if (bestMatch.Score == 0)
		{
			matchedItem = default;
			return false;
		}

		if (!bestMatch.Equals(default(MatchItem)))
		{
			matchedItem = bestMatch.PrefabData;
			return true;
		}

		matchedItem = default;
		return false;
	}


	public static bool TryGetBloodPrefabDataFromString(string needle, out PrefabData bloodPrefab)
	{
		return TryGetPrefabDataFromString(needle, BloodData.BloodPrefabData, out bloodPrefab);
	}

	public static bool TryGetJewelPrefabDataFromString(string needle, out PrefabData jewelPrefab)
	{
		return TryGetPrefabDataFromString(needle, JewelData.JewelPrefabData, out jewelPrefab);
	}

	public static bool TryGetItemPrefabDataFromString(string needle, out PrefabData itemPrefab)
	{
		return TryGetPrefabDataFromString(needle, Items.GiveableItemsPrefabData, out itemPrefab);
	}

	private static int IsSubsequence(string needle, string haystack)
	{
		int j = 0;
		int maxConsecutiveMatches = 0;
		int currentConsecutiveMatches = 0;

		for (int i = 0; i < needle.Length; i++)
		{
			while (j < haystack.Length && haystack[j] != needle[i])
			{
				j++;
			}

			if (j == haystack.Length)
			{
				return -1;
			}

			if (i > 0 && needle[i - 1] == haystack[j - 1])
			{
				currentConsecutiveMatches++;
			}
			else
			{
				if (currentConsecutiveMatches > maxConsecutiveMatches)
				{
					maxConsecutiveMatches = currentConsecutiveMatches;
				}

				currentConsecutiveMatches = 1;
			}

			j++;
		}

		if (currentConsecutiveMatches > maxConsecutiveMatches)
		{
			maxConsecutiveMatches = currentConsecutiveMatches;
		}

		return maxConsecutiveMatches;
	}
}
