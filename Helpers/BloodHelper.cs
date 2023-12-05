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
	//the ConsumeBloodDebugEvent wants a unit prefab as the source, and then uses that to set the blood to that unit's default bloodtype.
	//Ideally we would just pass it an existing prefab of a unit with that bloodtype, but there are no prefabs with frailed
	//Therefore, to make it do what we want, we have to change the unit's prefab blood type before running it, then change it back -.-
	public static void SetPlayerBlood(Player player, PrefabGUID bloodType, float quality = 100)
	{
		if (!player.User.Exists())
		{
			return;
		}
		PrefabGUID unitPrefab;
		PrefabGUID originalBloodType;
		quality = Clamp(quality, 0, 100);
		var bloodEntities = GetPrefabEntitiesByComponentTypes<BloodConsumeSource>();
		var entity = bloodEntities[0];

		var bloodConsumeSource = entity.Read<BloodConsumeSource>();
		originalBloodType = bloodConsumeSource.UnitBloodType;
		bloodConsumeSource.UnitBloodType = bloodType;
		entity.Write(bloodConsumeSource);

		unitPrefab = entity.Read<PrefabGUID>();
		var consumeBloodEvent = new ConsumeBloodDebugEvent
		{
			Amount = 100,
			Quality = quality,
			Source = unitPrefab,
		};

		Core.debugEventsSystem.ConsumeBloodEvent(player.User.Read<User>().Index, ref consumeBloodEvent);
		bloodConsumeSource.UnitBloodType = originalBloodType;
		entity.Write(bloodConsumeSource);
		bloodEntities.Dispose();

		//Helper.BuffCharacter(User.Read<User>().LocalCharacter._Entity, Prefabs.Buff_General_Phasing);

		Helper.BuffPlayer(player, Prefabs.AB_Werewolf_Howl_Buff, out var buffEntity, 1);
	}

	public static void SetDefaultBlood(Player player, string defaultBlood)
	{
		var blood = player.Character.Read<Blood>();
		bool bloodModified = false;
		if (blood.BloodType == Prefabs.BloodType_None || blood.BloodType == Prefabs.BloodType_Worker || blood.BloodType == Prefabs.BloodType_Mutant || blood.Quality != 100)
		{
			if (defaultBlood != "frailed")
			{
				if (defaultBlood == "warrior")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Warrior, 100);
					bloodModified = true;
				}
				else if (defaultBlood == "scholar")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Scholar, 100);
					bloodModified = true;
				}
				else if (defaultBlood == "rogue")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Rogue, 100);
					bloodModified = true;
				}
				else if (defaultBlood == "brute")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Brute, 100);
					bloodModified = true;
				}
				else if (defaultBlood == "creature")
				{
					Helper.SetPlayerBlood(player, Prefabs.BloodType_Creature, 100);
					bloodModified = true;
				}
			}
		}
		else if (defaultBlood == "frailed")
		{
			Helper.SetPlayerBlood(player, Prefabs.BloodType_None, 100);
			bloodModified = true;
		}
		if (bloodModified)
		{
			player.ReceiveMessage($"Your blood type isn't eligible for this game mode. Setting to the configured default blood".Error());
		}
	}
}
