using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using PvpArena.Data;
using ProjectM.UI;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Models;
using PvpArena.Helpers;

namespace PvpArena.Commands.Building;
internal class BuildingCommands
{
	[Command("claim-structures", description: "Used for debugging", adminOnly: true)]
	public static void ClaimStructuresCommand(Player sender)
	{
		Entity HeartEntity = Entity.Null;
		var entities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection>(true);
		foreach (var entity in entities)
		{
			if (entity.Read<PrefabGUID>() == Prefabs.TM_BloodFountain_Pylon_Station)
			{
				HeartEntity = entity;
				break;
			}
		}
		entities.Dispose();

		entities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection>(true);
		foreach (var entity in entities)
		{
			if (entity.Has<Team>())
			{
				var team = entity.Read<Team>();
				team.Value = sender.Character.Read<Team>().Value;
				team.FactionIndex = sender.Character.Read<Team>().FactionIndex;
				entity.Write(team);
				entity.Write(new CastleHeartConnection
				{
					CastleHeartEntity = HeartEntity
				});
			}
		}
		entities.Dispose();
		sender.ReceiveMessage(sender.Character.Read<Team>().Value.ToString());
	}

	[Command("unclaim-structures", description: "Used for debugging", adminOnly: true)]
	public static void UnclaimStructuresCommand(Player sender)
	{
		var entities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection>(true);
		foreach (var entity in entities)
		{
			if (entity.Has<Team>())
			{
				var team = entity.Read<Team>();
				team.Value = 1;
				team.FactionIndex = sender.Character.Read<Team>().FactionIndex;
				entity.Write(team);
				entity.Write(new CastleHeartConnection
				{
					CastleHeartEntity = Entity.Null
				});
			}
		}
		entities.Dispose();
		sender.ReceiveMessage(sender.Character.Read<Team>().Value.ToString());
	}

	[Command("enable-freebuild", adminOnly: true)]
	public void EnableFreeBuildCommand(Player sender)
	{

		SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BuildCostsDisabled,
			Value = true
		};

		SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
			Value = true
		};

		SetDebugSettingEvent CastleHeartConnectionRequirementDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.CastleHeartConnectionRequirementDisabled,
			Value = true
		};

		SetDebugSettingEvent FreeBuildingPlacementEnabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.FreeBuildingPlacementEnabled,
			Value = true
		};

		var debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
		debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref BuildingPlacementRestrictionsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref CastleHeartConnectionRequirementDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref FreeBuildingPlacementEnabledSetting);
		sender.ReceiveMessage("Free build enabled");
	}
	[Command("disable-freebuild", adminOnly: true)]
	public void DisableFreeBuildCommand(Player sender)
	{

		SetDebugSettingEvent BuildCostsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BuildCostsDisabled,
			Value = true
		};

		SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.BuildingPlacementRestrictionsDisabled,
			Value = false
		};

		SetDebugSettingEvent CastleHeartConnectionRequirementDisabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.CastleHeartConnectionRequirementDisabled,
			Value = false
		};

		SetDebugSettingEvent FreeBuildingPlacementEnabledSetting = new SetDebugSettingEvent()
		{
			SettingType = DebugSettingType.FreeBuildingPlacementEnabled,
			Value = false
		};

		var debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
		debugEventsSystem.SetDebugSetting(0, ref BuildCostsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref BuildingPlacementRestrictionsDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref CastleHeartConnectionRequirementDisabledSetting);
		debugEventsSystem.SetDebugSetting(0, ref FreeBuildingPlacementEnabledSetting);
		sender.ReceiveMessage("Free build disabled");
	}

	[Command("name-chest", description: "Used for debugging", adminOnly: true)]
	public void LockHoveredUnit(Player sender, string name)
	{
		var mousePosition = sender.Character.Read<EntityInput>().AimPosition;
		var entities = Helper.GetEntitiesByComponentTypes<NameableInteractable>();
		if (entities.Length > 0)
		{
			Helper.SortEntitiesByDistance(entities, mousePosition);
			var target = entities[0];
			target.Remove<Interactable>();
			target.Write(new NameableInteractable
			{
				Name = name,
				OnlyAllyRename = false,
				OnlyAllySee = false
			});
			sender.ReceiveMessage("Named and made uninteractable");
			return;
		}
		sender.ReceiveMessage("Found no valid entities".Error());
	}
}
