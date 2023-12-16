using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using PvpArena;
using PvpArena.Data;
using PvpArena.Models;
using UnityEngine;

public static class DamageRecorderService
{
	public class DamageInfo
	{ 
		public float TotalDamage { get; set; }
		public float CritDamage { get; set; }
		public float DamageAbsorbed { get; set; }

		public static DamageInfo operator +(DamageInfo a, DamageInfo b)
		{
			return new DamageInfo
			{
				TotalDamage = a.TotalDamage + b.TotalDamage,
				CritDamage = a.CritDamage + b.CritDamage,
				DamageAbsorbed = a.DamageAbsorbed + b.DamageAbsorbed
			};
		}
	}

	private static Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>> _playerDamageDealtRecord = new Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>>();
	private static Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>> _playerDamageReceivedRecord = new Dictionary<Player, Dictionary<PrefabGUID, DamageInfo>>();

	public static void RecordDamageDone(Player player, PrefabGUID ability, DamageInfo damageInfo)
	{
		if (!_playerDamageDealtRecord.ContainsKey(player))
		{
			_playerDamageDealtRecord[player] = new Dictionary<PrefabGUID, DamageInfo>();
		}

		if (!_playerDamageDealtRecord[player].ContainsKey(ability))
		{
			_playerDamageDealtRecord[player][ability] = new DamageInfo();
		}

		_playerDamageDealtRecord[player][ability] += damageInfo;
	}

	public static void RecordDamageReceived(Player player, PrefabGUID ability, DamageInfo damageInfo)
	{
		if (!_playerDamageReceivedRecord.ContainsKey(player))
		{
			_playerDamageReceivedRecord[player] = new Dictionary<PrefabGUID, DamageInfo>();
		}

		if (!_playerDamageReceivedRecord[player].ContainsKey(ability))
		{
			_playerDamageReceivedRecord[player][ability] = new DamageInfo();
		}

		_playerDamageReceivedRecord[player][ability] += damageInfo;
	}

	public static void ReportDamageResults(Player player)
	{
		if (_playerDamageDealtRecord.TryGetValue(player, out var abilityDamage))
		{
			float totalDamage = abilityDamage.Sum(kvp => kvp.Value.TotalDamage);
			var groupedDamage = GroupRelatedAbilities(abilityDamage);
			var sortedGroupedDamage = groupedDamage.OrderByDescending(kvp => kvp.Value.Sum(val => val.Value.TotalDamage));

			foreach (var group in sortedGroupedDamage)
			{
				float groupTotalDamage = group.Value.Sum(val => val.Value.TotalDamage);
				float percentage = (float)Math.Round((groupTotalDamage / totalDamage) * 100);

				// Get the group name and color
				string groupName = null;
				Color32 groupColor = ExtendedColor.ServerColor; // Default color
				foreach (var prefabGUID in group.Value.Keys)
				{
					if (groupName == null && AbilityData.AbilityPrefabToName.TryGetValue(prefabGUID, out var name))
					{
						groupName = name;
					}

					if (AbilityData.AbilityPrefabToColor.TryGetValue(prefabGUID, out var color))
					{
						groupColor = color; // Use the color associated with this ability
						break;
					}
				}

				float roundedGroupTotalDamage = (float)Math.Round(groupTotalDamage);

				if (roundedGroupTotalDamage > 0)
				{
					player.ReceiveMessage($"{groupName.Colorify(groupColor)} - {roundedGroupTotalDamage} ({percentage}%)".White());
				}
			}

			_playerDamageDealtRecord[player].Clear();
		}
	}

	private static Dictionary<string, Dictionary<PrefabGUID, DamageInfo>> GroupRelatedAbilities(Dictionary<PrefabGUID, DamageInfo> abilityDamage)
	{
		var groupedAbilities = new Dictionary<string, Dictionary<PrefabGUID, DamageInfo>>();
		foreach (var kvp in abilityDamage)
		{
			string groupName = GetGroupName(kvp.Key);
			if (!groupedAbilities.ContainsKey(groupName))
			{
				groupedAbilities[groupName] = new Dictionary<PrefabGUID, DamageInfo>();
			}
			groupedAbilities[groupName].Add(kvp.Key, kvp.Value);
		}
		return groupedAbilities;
	}

	private static string GetGroupName(PrefabGUID prefabGUID)
	{
		if (AbilityData.AbilityPrefabToName.TryGetValue(prefabGUID, out var name))
		{
			return name;
		}
		name = prefabGUID.LookupNameString();
		return name;
	}
}

public class DamageEvent
{
	public PrefabGUID Ability;
	public PrefabGUID DamageType;
	public float DamageAmount;
}
