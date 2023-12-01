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
using static PvpArena.Configs.ConfigDtos;
using Il2CppSystem.Collections.Generic;

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static void GenerateLegendaryViaEvent(Player player, string weapon, string infusion, string mods,
		float power = 1)
	{
		var weaponPrefabGUID = LegendaryData.weaponToPrefabDictionary[weapon];
		var infusionPrefabGUID = LegendaryData.infusionToPrefabDictionary[infusion];

		var itemEventEntity = VWorld.Server.EntityManager.CreateEntity(
			ComponentType.ReadWrite<FromCharacter>(),
			ComponentType.ReadWrite<CreateLegendaryWeaponDebugEvent>(),
			ComponentType.ReadWrite<HandleClientDebugEvent>(),
			ComponentType.ReadWrite<NetworkEventType>(),
			ComponentType.ReadWrite<ReceiveNetworkEventTag>()
		);

		var legendaryWeaponDebugEvent = new CreateLegendaryWeaponDebugEvent();
		legendaryWeaponDebugEvent.WeaponPrefabGuid = weaponPrefabGUID;
		legendaryWeaponDebugEvent.Tier = 2;
		legendaryWeaponDebugEvent.InfuseSpellMod = infusionPrefabGUID;
		power = 0.15f + power * (1 - 0.15f);

		if (mods.Length > 0)
		{
			var mod1 = System.Convert.ToInt32(mods[0].ToString(), 16) - 1;
			legendaryWeaponDebugEvent.StatMod1 = LegendaryData.statMods[mod1];
			legendaryWeaponDebugEvent.StatMod1Power = power;
			if (mods.Length > 1)
			{
				var mod2 = System.Convert.ToInt32(mods[1].ToString(), 16) - 1;
				legendaryWeaponDebugEvent.StatMod2 = LegendaryData.statMods[mod2];
				legendaryWeaponDebugEvent.StatMod2Power = power;
				if (mods.Length > 2)
				{
					var mod3 = System.Convert.ToInt32(mods[2].ToString(), 16) - 1;
					legendaryWeaponDebugEvent.StatMod3 = LegendaryData.statMods[mod3];
					legendaryWeaponDebugEvent.StatMod3Power = power;
				}
			}
		}

		var handleClientDebugEvent = itemEventEntity.Read<HandleClientDebugEvent>();
		handleClientDebugEvent.FromUserIndex = player.User.Read<User>().Index;

		itemEventEntity.Write(handleClientDebugEvent);
		itemEventEntity.Write(player.ToFromCharacter());
		itemEventEntity.Write(legendaryWeaponDebugEvent);
	}

	public static void GiveDefaultLegendaries(Player player)
	{
        System.Collections.Generic.List<LegendaryDto> legendaries = Core.defaultLegendaryWeaponStorage.GetLegendaryWeaponsForPlayer(player.SteamID);
		legendaries = legendaries
			.OrderBy(weapon => weapon.Slot == -1)
			.ThenBy(weapon => weapon.Slot)
			.ToList();

		foreach (var weapon in legendaries)
		{
			GenerateLegendaryViaEvent(player, weapon.WeaponName.ToLower(), weapon.Infusion, weapon.Mods);
		}
	}
}
