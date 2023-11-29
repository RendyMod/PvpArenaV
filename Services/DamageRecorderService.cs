using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using PvpArena;
using PvpArena.Models;

public static class DamageRecorderService
{
	private static Dictionary<Player, Queue<DamageEvent>> _playerDamageEventQueues = new Dictionary<Player, Queue<DamageEvent>>();
	private static Dictionary<Player, Dictionary<PrefabGUID, float>> _playerDamageRecord = new Dictionary<Player, Dictionary<PrefabGUID, float>>();

	public static void RecordDamageDealtInitial(Player player, DealDamageEvent dealDamageEvent)
	{
		if (!_playerDamageEventQueues.ContainsKey(player))
		{
			_playerDamageEventQueues[player] = new Queue<DamageEvent>();
		}
		_playerDamageEventQueues[player].Enqueue(new DamageEvent { Ability = dealDamageEvent.SpellSource.Read<PrefabGUID>() });
	}

	public static void RecordDamageDoneFinal(Player player, float damageAmount, PrefabGUID damageType)
	{
		if (_playerDamageEventQueues.TryGetValue(player, out var queue) && queue.Count > 0)
		{
			var damageEvent = queue.Dequeue();
			damageEvent.DamageAmount = damageAmount;
			damageEvent.DamageType = damageType;
			UpdateDamageRecord(player, damageEvent);
		}
		// Consider handling the case where there is no corresponding spell cast in the queue
	}

	private static void UpdateDamageRecord(Player player, DamageEvent damageEvent)
	{
		if (!_playerDamageRecord.ContainsKey(player))
		{
			_playerDamageRecord[player] = new Dictionary<PrefabGUID, float>();
		}

		if (!_playerDamageRecord[player].ContainsKey(damageEvent.Ability))
		{
			_playerDamageRecord[player][damageEvent.Ability] = 0;
		}

		_playerDamageRecord[player][damageEvent.Ability] += damageEvent.DamageAmount;
	}
	public static void ReportDamageResults(Player player)
	{
		if (_playerDamageRecord.TryGetValue(player, out var abilityDamage))
		{
			float totalDamage = abilityDamage.Sum(kvp => kvp.Value);
			var groupedDamage = GroupRelatedAbilities(abilityDamage);
			var sortedGroupedDamage = groupedDamage.OrderByDescending(kvp => kvp.Value.Sum(val => val.Value));

			foreach (var group in sortedGroupedDamage)
			{
				float groupTotalDamage = group.Value.Sum(val => val.Value);
				float percentage = (groupTotalDamage / totalDamage) * 100;
				string groupName = FindLargestSharedSubstring(group.Value.Keys.Select(k => k.LookupNameString()).ToList());
				float roundedGroupTotalDamage = (float)Math.Round(groupTotalDamage, 2);

				if (roundedGroupTotalDamage > 0)
				{
					player.ReceiveMessage($"{groupName.Colorify(ExtendedColor.ServerColor)} - {roundedGroupTotalDamage} ({percentage:F2}%)".White());
				}
			}

			_playerDamageRecord[player].Clear();
		}
	}

	private static Dictionary<string, Dictionary<PrefabGUID, float>> GroupRelatedAbilities(Dictionary<PrefabGUID, float> abilityDamage)
	{
		var groupedAbilities = new Dictionary<string, Dictionary<PrefabGUID, float>>();
		foreach (var kvp in abilityDamage)
		{
			string groupName = GroupKey(kvp.Key);
			if (!groupedAbilities.ContainsKey(groupName))
			{
				groupedAbilities[groupName] = new Dictionary<PrefabGUID, float>();
			}
			groupedAbilities[groupName].Add(kvp.Key, kvp.Value);
		}
		return groupedAbilities;
	}

	private static string GroupKey(PrefabGUID prefabGUID)
	{
		string name = prefabGUID.LookupName();
		string[] parts = name.Split('_');

		// Return the first three parts joined by underscores, or the entire name if less than three parts
		return parts.Length >= 3 ? string.Join("_", parts.Take(3)) : name;
	}


	private static string FindLargestSharedSubstring(List<string> names)
	{
		if (names.Count == 0)
			return "";

		string reference = names[0];
		int length = reference.Length;
		string longestCommonSubstring = "";

		for (int i = 0; i < length; i++)
		{
			for (int j = i + 1; j <= length; j++)
			{
				string substring = reference.Substring(i, j - i);
				if (substring.EndsWith("_") && names.All(name => name.StartsWith(substring)) && substring.Length > longestCommonSubstring.Length)
				{
					longestCommonSubstring = substring;
				}
			}
		}

		return longestCommonSubstring != "" ? longestCommonSubstring.TrimEnd('_') : reference;
	}
}

public class DamageEvent
{
	public PrefabGUID Ability;
	public PrefabGUID DamageType;
	public float DamageAmount;
}
