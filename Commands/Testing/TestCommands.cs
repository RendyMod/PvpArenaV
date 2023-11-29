using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PvpArena.Patches;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.LightningStorm;
using ProjectM.Network;
using ProjectM.Terrain;
using ProjectM.Tiles;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using ProjectM.Auth;
using ProjectM.Gameplay.Systems;
using ProjectM.Shared;
using System.Reflection;
using static Il2CppSystem.Diagnostics.Tracing.EventProvider;
using Unity.Collections;
using Lidgren.Network;
using Stunlock.Network;
using ProjectM.Scripting;
using Bloodstone.API;
using ProjectM.Gameplay.Scripting;
using Il2CppSystem;
using Unity.Mathematics;
using static ProjectM.BuffUtility;
using PvpArena.Data;
using ProjectM.CastleBuilding;
using UnityEngine;
using static ProjectM.Network.ClanEvents_Server;
using ProjectM.Gameplay.Clan;
using ProjectM.UI;
using PvpArena.Matchmaking;
using UnityEngine.UIElements;
using ProjectM.Sequencer;
using System.Threading;
using Unity.Services.Core;
using PvpArena.Services;
using static ProjectM.Network.InteractEvents_Client;
using ProjectM.Gameplay;
using Unity.Jobs;
using ProjectM.CastleBuilding.Teleporters;
using Il2CppSystem.Linq.Expressions.Interpreter;
using Unity.Physics.Systems;
using static RootMotion.FinalIK.AimPoser;
using UnityEngine.TextCore;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using PvpArena.Helpers;
using ProjectM.CastleBuilding.Placement;
using ProjectM.Hybrid;
using static ProjectM.SharedModifiableFunctions;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Configs;
using PvpArena.Factories;
using ProjectM.Sequencer.Debugging;
using Stunlock.Sequencer;
using Unity.Services.Core.Telemetry.Internal;
using Discord;
using UnityEngine.Jobs;
using ProjectM.Behaviours;
using ProjectM.Pathfinding;
using static ProjectM.SpawnRegionSpawnSystem;
using ProjectM.Audio;
using LibCpp2IL.BinaryStructures;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Domination;
using Unity.Entities.UniversalDelegates;
using static ProjectM.Scripting.Game;
using System.Text.Json;
using System.IO;
using static PlayerJewels;
using Stunlock.Fmod;
using FMOD;
using PvpArena.GameModes.BulletHell;
using ProjectM.Debugging;
using Il2CppSystem.Xml.Schema;
using UnityEngine.Rendering;
using PvpArena.Services.Moderation;
using Unity.Core;

namespace PvpArena.Commands.Debug;
internal class TestCommands
{

	[Command("test", description: "Used for debugging", adminOnly: true)]
	public void TestCommand(Player sender, int rotationMode = 0)
	{
		var prefab = Helper.GetPrefabEntityByPrefabGUID(Prefabs.ScrollingCombatTextMessage);
		var commandBuffer = Core.entityCommandBufferSystem.CreateCommandBuffer();
		string myCustomText = "Your custom message here";
		var bytes = Encoding.UTF8.GetBytes(myCustomText);
		var assetGuid = AssetGuid.FromBytes(bytes);
		
		var entity = ScrollingCombatTextMessage.Create(VWorld.Server.EntityManager, commandBuffer, prefab, assetGuid, sender.Position, sender.Position, sender.Character, 0f, Prefabs.SCT_Type_InfoMessage, sender.User);
		
		/*var sct = entity.Read<ScrollingCombatTextMessage>();
		sct.OverrideText = "HELLO THERE";
		entity.Write(sct);*/
		//ScrollingCombatTextMessage.CreateLocal(VWorld.Server.EntityManager, prefab, new FixedString512("testinggg"), sender.Position, sender.Position, sender.Character);
	}

	[Command("test2", description: "Used for debugging", adminOnly: true)]
	public void Test2Command(Player sender)
	{
		var sortedPlayers = PlayerService.UserCache.Values.OrderByDescending(p =>
		{
			float.TryParse(p.PlayerBulletHellData.BestTime, out float longestTime);
			return longestTime;
		}).ToList();
		sender.ReceiveMessage(sender.PlayerBulletHellData.BestTime);
	}



	[Command("test3", description: "Used for debugging", adminOnly: true)]
	public void Test3Command(Player sender, bool friendly = true)
	{
		var entity = Helper.CreateEntityWithComponents<AbilityGroupSlotModificationBuffer>();
		var buffer = entity.ReadBuffer<AbilityGroupSlotModificationBuffer>();
		List<PrefabGUID> abilities = new List<PrefabGUID>
		{
			PrefabGUID.Empty, // auto
			PrefabGUID.Empty, // q
			PrefabGUID.Empty, // dash
			PrefabGUID.Empty, // ??
			PrefabGUID.Empty, // e
			PrefabGUID.Empty, // r
			PrefabGUID.Empty, //c
			PrefabGUID.Empty  //t

		};
		for (int i = 0; i < abilities.Count; i++)
		{
			buffer.Add(new AbilityGroupSlotModificationBuffer
			{
				NewAbilityGroup = abilities[i],
				Owner = sender.Character,
				Target = sender.Character,
				Slot = i,
				CopyCooldown = false,
				Priority = 101
			});
		}

		/*entity.Add<AbilityGroupSlotModificationDestroy>();
		entity.Add<ReplaceAbilityOnSlotBuff_AllInitialized>();*/


	}

	[Command("test4", description: "Used for debugging", adminOnly: true)]
	public void Test4Command(Player sender, Player player)
	{
		Helper.BuffPlayer(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown, out var buffEntity, Helper.NO_DURATION, true);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.BuildMenuImpair);
	}

	[Command("move-structures", description: "Used for debugging", adminOnly: true)]
	public void MoveStructuresCommand(Player sender, int range, float xOffset = 0, float yOffset = 0, float zOffset = 0)
	{
		var entities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection>(true);
		foreach (var entity in entities)
		{
			var translation = entity.Read<Translation>();
			if (entity.Has<LocalToWorld>())
			{
				var rotation = entity.Read<LocalToWorld>().Rotation;
				if (math.distance(sender.Position, translation.Value) <= range)
				{
					sender.ReceiveMessage(Rotations.GetRotationModeFromQuaternion(rotation).ToString());
					translation.Value.x += xOffset;
					translation.Value.y += yOffset;
					translation.Value.z += zOffset;
					entity.Write(translation);
				}
			}
			else
			{
				entity.LogPrefabName();
			}
		}
		sender.ReceiveMessage("Done!");
	}


	[Command("move-structure", description: "Used for debugging", adminOnly: true)]
	public void MoveStructuresCommand(Player sender, PrefabGUID prefab, int range, float xOffset = 0, float yOffset = 0, float zOffset = 0)
	{
		var entities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection>(true);
		foreach (var entity in entities)
		{
			var translation = entity.Read<Translation>();
			if (entity.Has<LocalToWorld>())
			{
				var rotation = entity.Read<LocalToWorld>().Rotation;
				if (math.distance(sender.Position, translation.Value) <= range)
				{
					sender.ReceiveMessage(Rotations.GetRotationModeFromQuaternion(rotation).ToString());
					translation.Value.x += xOffset;
                    translation.Value.x = (float)System.Math.Round(translation.Value.x, 1);
					translation.Value.y += yOffset;
					translation.Value.z += zOffset;
					translation.Value.z = (float)System.Math.Round(translation.Value.z, 1);
					entity.Write(translation);
				}
			}
			else
			{
				entity.LogPrefabName();
			}
		}
		sender.ReceiveMessage("Done!");
	}

	[Command("export-structures", description: "Used for debugging", adminOnly: true)]
	public void ExportStructuresCommand(Player sender, int range = -1)
	{
		SavedStructures.ExportStructures(sender, range);
		sender.ReceiveMessage("Done!");
	}

	[Command("adjust-rotations", description: "Used for debugging", adminOnly: true)]
	public void AdjustRotationsCommand(Player sender)
	{
		var sys = VWorld.Server.GetExistingSystem<SpawnCastleTeamSystem>();
		sys.Enabled = false;
		var entities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection, LocalToWorld, TileModel>();
		for (var i = 0; i < entities.Length; i++)
		{
			var entity = entities[i];
			var rotation = entity.Read<Rotation>();
			var rotationMode = Rotations.GetRotationModeFromQuaternion(rotation.Value);
			rotationMode %= 4;
			rotationMode += 1;
			var newRotation = Rotations.RotationModes[rotationMode];
			rotation.Value = newRotation;
			entity.Write(rotation);
		}
		
		sender.ReceiveMessage("Done!");
	}


	[Command("import-structures", description: "Used for debugging", adminOnly: true)]
	public void ImportStructuresCommand(Player sender)
	{
		var sys = VWorld.Server.GetExistingSystem<SpawnCastleTeamSystem>();
		sys.Enabled = false;
		for (var i = 0; i < SavedStructures.Structures.Count; i++)
		{
			var structure = SavedStructures.Structures[i];
			PrefabSpawnerService.SpawnWithCallback(structure.PrefabGUID, structure.Location.ToFloat3(), (e) =>
			{
				if (e.Has<DyeableCastleObject>())
				{
					var dyeable = e.Read<DyeableCastleObject>();
					dyeable.ActiveColorIndex = (byte)structure.DyeColor;
					e.Write(dyeable);
				}
			}, structure.Rotation, -1, true);
		}
		sys.Enabled = true;
		sender.ReceiveMessage("Done!");
	}

	[Command("enable-spawns", description: "Used for debugging", adminOnly: true)]
	public void EnableSpawnsCommand(Player sender)
	{
		var entities = Helper.GetPrefabEntitiesByComponentTypes<PrefabGUID, Health>();
		foreach (var entity in entities)
		{
			entity.Remove<DestroyOnSpawn>();
		}

		sender.ReceiveMessage("Spawns enabled until disabled or next restart");
	}

	[Command(name: "log-zone", description: "Gets the zone assuming you are at the bottom left", usage: ".get-zone", adminOnly: true, includeInHelp: false)]
	public void GetZoneCommand(Player player, int x, int z)
	{
		player.ReceiveMessage($"{RectangleZone.GetZoneByCurrentCoordinates(player, x, z)}");
		Plugin.PluginLog.LogInfo($"{RectangleZone.GetZoneByCurrentCoordinates(player, x, z)}");
	}

	[Command(name: "check-zone", description: "Does debug things", usage: ".check-zone", adminOnly: true, includeInHelp: false)]
	public void CheckZoneCommand(Player player)
	{
		if (CaptureThePancakeConfig.Config.Team1EndZone.ToRectangleZone().Contains(player))
		{
			player.ReceiveMessage("in zone".Success());
		}
		else
		{
			player.ReceiveMessage("not in zone".Error());
		}
	}

	[Command(name: "get-coordinates", description: "Logs centered coordinates of mouse position", usage: ".get-coordinates", adminOnly: true, includeInHelp: false)]
	public void GetCoordinatesCommand(Player player, string test1 = "hi", int num = 4)
	{
		var input = player.Character.Read<EntityInput>();
		float3 aimPosition = input.AimPosition;

		// Centering X and Z coordinates
		float centeredX = (float)System.Math.Floor(aimPosition.x) + 0.5f;
		float centeredZ = (float)System.Math.Floor(aimPosition.z) + 0.5f;

		// Format and log the coordinates
		string logMessage = $"\"X\": {centeredX},\n\"Y\": {aimPosition.y},\n\"Z\": {centeredZ}";
		Plugin.PluginLog.LogInfo(logMessage);
	}




	[Command("get-nearby-structures", description: "Used for debugging", adminOnly: true)]
	public static void GetNearbyCommand(Player sender, int range = 10)
	{
		var entities = Helper.GetEntitiesByComponentTypes<TileModel>();
		Helper.SortEntitiesByDistance(entities, sender.Position);
		foreach (var entity in entities)
		{
			if (!entity.Has<LocalToWorld>()) continue;
			var distance = math.distance(sender.Position, entity.Read<LocalToWorld>().Position);
			if (entity.Has<CastleHeartConnection>() && distance < range)
			{
				sender.ReceiveMessage(entity.Read<PrefabGUID>().LookupNameString());
			}
		}
	}

	[Command("destroy-structures", description: "Used for debugging", adminOnly: true)]
	public static void DestroyStructuresCommand(Player sender, int range = 10, PrefabGUID structureType = default)
	{
		var entities = Helper.GetEntitiesByComponentTypes<TileModel>();
		Helper.SortEntitiesByDistance(entities, sender.Position);
		foreach (var entity in entities)
		{
			if (!entity.Has<LocalToWorld>()) continue;
			var distance = math.distance(sender.Position, entity.Read<LocalToWorld>().Position);
			if (entity.Has<CastleHeartConnection>() && distance < range) 
			{
				if (structureType == default)
				{
					sender.ReceiveMessage(entity.Read<PrefabGUID>().LookupNameString());
					Helper.DestroyEntity(entity);
				}
				else
				{
					if (entity.Read<PrefabGUID>() == structureType)
					{
						Helper.DestroyEntity(entity);
						sender.ReceiveMessage($"Killed entity: {entity.Read<PrefabGUID>().LookupName()}".Success());
					}
				}
			}
		}
	}

	[Command("teleport-prefab", description: "Used for debugging", adminOnly: true)]
	public static void TeleportPrefabCommand(Player sender)
	{
		Entity entity = Helper.GetHoveredEntity(sender.Character);
		entity.Write(sender.Character.Read<LocalToWorld>());
		entity.Write(sender.Character.Read<Translation>());
		sender.ReceiveMessage("Attempted to teleport entity".Success());
	}

	/*	[Command("make-moveable", description: "Used for debugging", adminOnly: true)]
		public static void MakePrefabMoveableCommand(Player sender)
		{
			var Character = ctx.Event.SenderCharacterEntity;
			Entity entity = Helper.GetHoveredEntity(Character);
			var example = Helper.GetEntitiesByComponentTypes<EditableTileModel>()[0];
			entity.Add<EditableTileModel>();
			entity.Write(example.Read<EditableTileModel>());
			sender.ReceiveMessage("Done"); //not currently working
		}*/

	[Command("update-rotation", description: "Used for debugging", adminOnly: true)]
	public static void UpdateRotationCommand(Player sender)
	{
		var Character = sender.Character;
		Entity entity = Helper.GetHoveredEntity(Character);
		var rotation = entity.Read<Rotation>();
		var rotationMode = Rotations.GetRotationModeFromQuaternion(rotation.Value);
		rotationMode %= 4;
		rotationMode += 1;
		rotation.Value = Rotations.RotationModes[rotationMode];
		entity.Write(rotation);
		sender.ReceiveMessage($"Done: {entity.LookupName()} to: {rotation.Value} {Rotations.TileRotationModes[rotationMode]}"); //works only on some structures
		var tilePosition = entity.Read<TilePosition>();
		tilePosition.TileRotation = Rotations.TileRotationModes[rotationMode]; //tile rotation may not be lined up correctly with regular rotation
		entity.Write(tilePosition);
	}


	[Command("save", description: "Used for debugging", adminOnly: true)]
	public static void SaveCommand(Player sender)
	{
		PlayerJewels.ExportJewels();
		PlayerLegendaries.ExportLegendaries();

		sender.ReceiveMessage("Data saved.");
	}


	[Command("claim-target", description: "Used for debugging", adminOnly: true)]
	public static void ClaimTargetCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity(sender.Character);

		entity.Write(sender.Character.Read<Team>());
		entity.Write(sender.Character.Read<TeamReference>());
	}
}
