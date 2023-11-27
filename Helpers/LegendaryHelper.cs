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
		if (PlayerLegendaries.LegendaryWeaponsData.ContainsKey(player.SteamID))
		{
			foreach (var weapon in PlayerLegendaries.LegendaryWeaponsData[player.SteamID])
			{
				GenerateLegendaryViaEvent(player, weapon.WeaponName, weapon.Infusion, weapon.Mods);
			}
		}
		if ((PlayerLegendaries.LegendaryWeaponsData.ContainsKey(player.SteamID) && PlayerLegendaries.LegendaryWeaponsData[player.SteamID].Count < 5) || !PlayerLegendaries.LegendaryWeaponsData.ContainsKey(player.SteamID))
		{
			GenerateLegendaryViaEvent(player, "slashers", PvpArenaConfig.Config.DefaultLegendaries["Slashers"].Infusion,
	PvpArenaConfig.Config.DefaultLegendaries["Slashers"].Mods);
			GenerateLegendaryViaEvent(player, "spear", PvpArenaConfig.Config.DefaultLegendaries["Spear"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Spear"].Mods);
			GenerateLegendaryViaEvent(player, "axes", PvpArenaConfig.Config.DefaultLegendaries["Axes"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Axes"].Mods);
			GenerateLegendaryViaEvent(player, "greatsword", PvpArenaConfig.Config.DefaultLegendaries["GreatSword"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["GreatSword"].Mods);
			GenerateLegendaryViaEvent(player, "crossbow", PvpArenaConfig.Config.DefaultLegendaries["Crossbow"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Crossbow"].Mods);
			GenerateLegendaryViaEvent(player, "pistols", PvpArenaConfig.Config.DefaultLegendaries["Pistols"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Pistols"].Mods);
			GenerateLegendaryViaEvent(player, "reaper", PvpArenaConfig.Config.DefaultLegendaries["Reaper"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Reaper"].Mods);
			GenerateLegendaryViaEvent(player, "sword", PvpArenaConfig.Config.DefaultLegendaries["Sword"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Sword"].Mods);
			GenerateLegendaryViaEvent(player, "mace", PvpArenaConfig.Config.DefaultLegendaries["Mace"].Infusion,
				PvpArenaConfig.Config.DefaultLegendaries["Mace"].Mods);
		}

	}
}
