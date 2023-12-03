using System.Collections.Generic;
using ProjectM;
using ProjectM.Tiles;
using Unity.Entities;
using Unity.Transforms;
using Bloodstone.API;
using Unity.Mathematics;
using PvpArena.Data;
using ProjectM.CastleBuilding;
using PvpArena.Helpers;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using System.Numerics;
using System;

namespace PvpArena.Commands.Debug;
internal class TestCommands
{

	[Command("test", description: "Used for debugging", adminOnly: true)]
	public void TestCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity(sender.Character);
		if (entity.Has<EntityInput>())
		{
			var entityInput = entity.Read<EntityInput>();
			entityInput.SetAllAimPositions(sender.Position);
			entity.Write(entityInput);
		}
	}

	[Command("test2", description: "Used for debugging", adminOnly: true)]
	public void Test2Command(Player sender)
	{
		Helper.BuffPlayer(sender, Helper.CustomBuff4, out var buffEntity);
		var abilityBar = new AbilityBar
		{
			Spell1 = Prefabs.AB_Blood_Shadowbolt_AbilityGroup,
			Spell2 = Prefabs.AB_Blood_BloodRite_AbilityGroup,
			Weapon2 = Prefabs.AB_Vampire_Spear_Harpoon_Throw_AbilityGroup
		};
		abilityBar.ApplyChangesHard(buffEntity);
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

	[Command("rotate-unit", description: "Used for debugging", adminOnly: true)]
	public void RotateUnitCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity<EntityInput>(sender.Character);
		var entityInput = entity.Read<EntityInput>();
		var entityPosition = entity.Read<LocalToWorld>().Position;

		var currentAimPosition = entityInput.AimPosition;
		var aimDirection = currentAimPosition - entityPosition;

		var currentAngle = Math.Atan2(aimDirection.z, aimDirection.x) * (180 / Math.PI);
		currentAngle = (currentAngle + 360) % 360;

		// Tolerance for floating-point precision
		float tolerance = 0.01f;

		// Function to check if the angle is near a multiple of 45
		bool IsNearMultipleOf45(float angle)
		{
			float mod = angle % 45;
			return mod < tolerance || (45 - mod) < tolerance;
		}

		if (!IsNearMultipleOf45((float)currentAngle))
		{
			currentAngle = Math.Round(currentAngle / 45) * 45;
		}
		else
		{
			currentAngle += 45;
		}

		var newAngleRadians = currentAngle * (Math.PI / 180);
		var newAimDirection = new float3((float)Math.Cos(newAngleRadians), aimDirection.y, (float)Math.Sin(newAngleRadians));

		var newAimPosition = entityPosition + newAimDirection;
		entityInput.SetAllAimPositions(newAimPosition);
		entity.Write(entityInput);
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

	[Command(name: "log-hp", description: "Gets the hp of the hovered unit", usage: ".log-hp", adminOnly: true, includeInHelp: false)]
	public void LogHpCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity(sender.Character);
		if (entity.Has<Health>())
		{
			sender.ReceiveMessage(entity.Read<Health>().Value.ToString().White());
		}
		else
		{
			sender.ReceiveMessage("That entity has no health component.".White());
		}
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
	public void GetCoordinatesCommand(Player sender, int snapMode = (int)Helper.SnapMode.Center)
	{
		var snappedAimPosition = Helper.GetSnappedHoverPosition(sender, (Helper.SnapMode)snapMode);

		// Format and log the coordinates
		string logMessage = $"\"X\": {snappedAimPosition.x},\n\"Y\": {snappedAimPosition.y},\n\"Z\": {snappedAimPosition.z}";
		sender.ReceiveMessage(logMessage.White());
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
		Core.defaultJewelStorage.ExportJewels();
		Core.defaultLegendaryWeaponStorage.ExportLegendariesToDatabase();

		sender.ReceiveMessage("Data saved.");
	}


	[Command("claim-target", description: "Used for debugging", adminOnly: true)]
	public static void ClaimTargetCommand(Player sender)
	{
		var entity = Helper.GetHoveredEntity(sender.Character);

		entity.Write(sender.Character.Read<Team>());
		entity.Write(sender.Character.Read<TeamReference>());
	}

	[Command("make-sct", description: "Used for debugging", adminOnly: true)]
	public static void MakeSctCommand(Player sender, PrefabGUID sctPrefab)
	{
		Helper.MakeSCT(sender, sctPrefab);
	}
	
}
