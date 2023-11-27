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
	public static void GenerateJewelViaEvent(Player player, string spellName, string mods = "",
		float power = 1, int tier = 3)
	{
		PrefabGUID abilityPrefab = JewelData.abilityToPrefabDictionary[spellName];
		power = 0.15f + power * (1 - 0.15f);
		if (tier == 4)
		{
			GenerateJewelDebugEvent generateJewelDebugEvent = new GenerateJewelDebugEvent();
			generateJewelDebugEvent.AbilityPrefabGuid = abilityPrefab;
			generateJewelDebugEvent.Power = 1;
			generateJewelDebugEvent.Tier = 3;

			Core.debugEventsSystem.GenerateJewelEvent(player.User.Read<User>().Index, ref generateJewelDebugEvent);
		}
		else
		{
			if (tier == 3 && mods.Length >= 3)
			{
				int mod0 = System.Convert.ToInt32(mods[0].ToString(), 16) - 1;
				int mod1 = System.Convert.ToInt32(mods[1].ToString(), 16) - 1;
				int mod2 = System.Convert.ToInt32(mods[2].ToString(), 16) - 1;

				CreateJewelDebugEventV2 createJewelDebugEvent = new CreateJewelDebugEventV2();
				createJewelDebugEvent.AbilityPrefabGuid = abilityPrefab;
				createJewelDebugEvent.Tier = 2;
				if (JewelData.SpellMods.ContainsKey(spellName))
				{
					if (mod0 >= 0 && mod0 < JewelData.SpellMods.Count)
					{
						createJewelDebugEvent.SpellMod1 = JewelData.SpellMods[spellName][mod0].Key;
						createJewelDebugEvent.SpellMod1Power = power;
					}
					else
					{
						Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod0}");
						return;
					}

					if (mod1 >= 0 && mod1 < JewelData.SpellMods.Count)
					{
						createJewelDebugEvent.SpellMod2 = JewelData.SpellMods[spellName][mod1].Key;
						createJewelDebugEvent.SpellMod2Power = power;
					}
					else
					{
						Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod1}");
						return;
					}

					if (mod2 >= 0 && mod2 < JewelData.SpellMods.Count)
					{
						createJewelDebugEvent.SpellMod3 = JewelData.SpellMods[spellName][mod2].Key;
						createJewelDebugEvent.SpellMod3Power = power;
					}
					else
					{
						Plugin.PluginLog.LogInfo($"Tried to spawn jewel with invalid mod, check your config!: {mod2}");
						return;
					}

					var jewelEventEntity = VWorld.Server.EntityManager.CreateEntity(
						ComponentType.ReadWrite<FromCharacter>(),
						ComponentType.ReadWrite<CreateJewelDebugEventV2>(),
						ComponentType.ReadWrite<HandleClientDebugEvent>(),
						ComponentType.ReadWrite<NetworkEventType>(),
						ComponentType.ReadWrite<ReceiveNetworkEventTag>()
					);

					jewelEventEntity.Write(createJewelDebugEvent);

					HandleClientDebugEvent handleClientDebugEvent = jewelEventEntity.Read<HandleClientDebugEvent>();
					handleClientDebugEvent.FromUserIndex = player.User.Read<User>().Index;
					jewelEventEntity.Write(handleClientDebugEvent);
				}
				else
				{
					Plugin.PluginLog.LogInfo(
						$"Tried to spawn jewel with invalid ability, check your config!: {spellName}");
				}
			}
		}
	}

	public static void CreateJewel(FromCharacter fromData, string spellName, string mods = "", float power = 1)
	{
		PrefabGUID abilityPrefab = JewelData.abilityToPrefabDictionary[spellName];
		if (Core.jewelSpawnSystem.TryCreateJewelAndAddToInventory(fromData.Character, abilityPrefab, 2,
				out Entity jewelEntity))
		{
			bool randomPower = false;
			if (power == JewelData.RANDOM_POWER)
			{
				randomPower = true;
			}

			if (power > 1)
			{
				power = 1;
			}
			else if (power < 0)
			{
				power = 0;
			}

			power = (float)(0.15 + power * (1 - 0.15));
			SpellModSetComponent spellModSet = jewelEntity.Read<SpellModSetComponent>();

			if (mods == "")
			{
				for (var i = 0; i < JewelData.SpellMods[spellName].Count && i < 8; i++)
				{
					mods += (i + 1).ToString();
				}
			}

			var mod0 = System.Convert.ToInt32(mods[0].ToString(), 16) - 1;
			var mod1 = System.Convert.ToInt32(mods[1].ToString(), 16) - 1;
			var mod2 = System.Convert.ToInt32(mods[2].ToString(), 16) - 1;
			spellModSet.SpellMods.Mod0.Id = JewelData.SpellMods[spellName][mod0].Key;
			if (randomPower)
			{
				power = (float)random.NextDouble();
				power = 0.15f + power * (1 - 0.15f);
			}

			spellModSet.SpellMods.Mod0.Power = power;
			spellModSet.SpellMods.Mod1.Id = JewelData.SpellMods[spellName][mod1].Key;
			if (randomPower)
			{
				power = (float)random.NextDouble();
				power = 0.15f + power * (1 - 0.15f);
			}

			spellModSet.SpellMods.Mod1.Power = power;
			spellModSet.SpellMods.Mod2.Id = JewelData.SpellMods[spellName][mod2].Key;
			if (randomPower)
			{
				power = (float)random.NextDouble();
				power = 0.15f + power * (1 - 0.15f);
			}

			spellModSet.SpellMods.Mod2.Power = power;
			if (mods.Length > 3)
			{
				int mod3 = int.Parse(mods[3].ToString()) - 1;
				spellModSet.SpellMods.Mod3.Id = JewelData.SpellMods[spellName][mod3].Key;
				if (randomPower)
				{
					power = (float)random.NextDouble();
					power = 0.15f + power * (1 - 0.15f);
				}

				spellModSet.SpellMods.Mod3.Power = power;
				if (mods.Length > 4)
				{
					int mod4 = int.Parse(mods[4].ToString()) - 1;
					spellModSet.SpellMods.Mod4.Id = JewelData.SpellMods[spellName][mod4].Key;
					if (randomPower)
					{
						power = (float)random.NextDouble();
						power = 0.15f + power * (1 - 0.15f);
					}

					spellModSet.SpellMods.Mod4.Power = power;
					if (mods.Length > 5)
					{
						int mod5 = int.Parse(mods[5].ToString()) - 1;
						spellModSet.SpellMods.Mod5.Id = JewelData.SpellMods[spellName][mod5].Key;
						if (randomPower)
						{
							power = (float)random.NextDouble();
							power = 0.15f + power * (1 - 0.15f);
						}

						spellModSet.SpellMods.Mod5.Power = power;
						if (mods.Length > 6)
						{
							int mod6 = int.Parse(mods[6].ToString()) - 1;
							spellModSet.SpellMods.Mod6.Id = JewelData.SpellMods[spellName][mod6].Key;
							if (randomPower)
							{
								power = (float)random.NextDouble();
								power = 0.15f + power * (1 - 0.15f);
							}

							spellModSet.SpellMods.Mod6.Power = power;
							if (mods.Length > 7)
							{
								int mod7 = int.Parse(mods[7].ToString()) - 1;
								spellModSet.SpellMods.Mod7.Id = JewelData.SpellMods[spellName][mod7].Key;
								if (randomPower)
								{
									power = (float)random.NextDouble();
									power = 0.15f + power * (1 - 0.15f);
								}

								spellModSet.SpellMods.Mod7.Power = power;
							}
						}
					}
				}
			}

			jewelEntity.Write(spellModSet);
		}
	}
}
