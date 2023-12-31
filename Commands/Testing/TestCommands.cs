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
using System;
using PvpArena.GameModes.Dodgeball;
using ProjectM.Network;
using PvpArena.Services;
using PvpArena.Patches;
using PvpArena.Factories;
using Unity.Collections;
using PvpArena.GameModes.Troll;
using PvpArena.GameModes.PrisonBreak;
using ProjectM.Gameplay.Scripting;
using ProjectM.Pathfinding;
using ProjectM.Behaviours;
using static ProjectM.Gameplay.Systems.StatChangeMutationSystem;
using ProjectM.Gameplay.Systems;
using Epic.OnlineServices.P2P;
using ProjectM.Shared;
using System.Linq;
using Epic.OnlineServices.Sessions;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Entities.UniversalDelegates;
using static ProjectM.HitColliderCast;
using PvpArena.GameModes.OD;

namespace PvpArena.Commands.Debug;
internal class TestCommands
{
	public static Entity MapIcon;
	[Command("test", description: "Used for debugging", adminOnly: true)]
	public void TestCommand(Player sender)
	{		
		sender.ReceiveMessage("done");
	}

	[Command("start-od", description: "Starts an OD match", aliases: new string[] { "od-fight" }, adminOnly: true)]
	public void StartODFightCommand(Player sender, Player player1, Player player2)
	{
		if (player1.IsAlliedWith(player2))
		{
			sender.ReceiveMessage("Cannot start a match against people in the same clan".Error());
			return;
		}
		if (player1.CurrentState != Player.PlayerState.Normal)
		{
			sender.ReceiveMessage($"{player1.Name} is already in a game mode".Error());
			return;
		}
		if (player2.CurrentState != Player.PlayerState.Normal)
		{
			sender.ReceiveMessage($"{player2.Name} is already in a game mode".Error());
			return;
		}
		var player1Pos = sender.Position + new float3(10, 0, 0);
		var player2Pos = sender.Position + new float3(-10, 0, 0);
		player1.Teleport(player1Pos);
		player2.Teleport(player2Pos);
		
		ODManager.StartMatch(player1, player2);
		sender.ReceiveMessage("OD'd".White());
	}

	[Command("end-od", description: "Starts an OD match", adminOnly: true)]
	public void EndODFightCommand(Player sender, Player player)
	{
		if (player.CurrentState != Player.PlayerState.OD)
		{
			sender.ReceiveMessage($"{player.Name} isn't in an OD match".Error());
			return;
		}

		var matchNumber = ODManager.FindMatchNumberByPlayer(player);
		if (matchNumber != -1)
		{
			ODManager.EndMatch(matchNumber, 0);
			sender.ReceiveMessage("Match ended".White());
			return;
		}

		sender.ReceiveMessage("Could not find a match with that player".Error());
	}

	[Command("spawn-fire", description: "Spawns a single ring of fire", adminOnly: true)]
	public void SpawnFireCommand(Player sender, float safeZoneRadius = 5)
	{
		var myPos = sender.Position;

		// Define the size of a tile
		float tileSize = 5.0f;

		// Calculate the radiusIncrement based on tileSize
		float radiusIncrement = tileSize / 2; // Half-tile increment

		// Calculate the number of fires to spawn in the ring
		int numFires = Mathf.CeilToInt(2 * Mathf.PI * safeZoneRadius / radiusIncrement);

		// Spawn the fires in the ring
		for (int i = 0; i < numFires; i++)
		{
			// Calculate the angle for this fire
			float angle = (2 * Mathf.PI / numFires) * i;

			// Calculate the fire position using polar coordinates
			Vector3 firePosition = new Vector3(
				myPos.x + safeZoneRadius * Mathf.Cos(angle),
				myPos.y, // Assuming y is constant and the fire effect spawns at ground level
				myPos.z + safeZoneRadius * Mathf.Sin(angle)
			);

			// Spawn fire at this position
			PrefabSpawnerService.SpawnWithCallback(Prefabs.AB_Shared_FireArea, firePosition, (e) => 
			{
				e.Remove<HitColliderCast>();
				e.Remove<HitTrigger>();
				e.Remove<ApplyBuffOnGameplayEvent>();
				e.Remove<GameplayEventListeners>();
				e.Remove<CollisionCastOnUpdate>();
				e.Remove<CreateGameplayEventsOnTick>();
				e.Remove<CreateGameplayEventsOnHit>();
			});
		}

		sender.ReceiveMessage($"Ring of fire created with a radius of {safeZoneRadius}. Total fires spawned: {numFires}.");
	}

	[Command("extinguish", description: "Used for debugging", adminOnly: true)]
	public void ExtinguishCommand(Player sender)
	{
		var entities = Helper.GetEntitiesByComponentTypes<SpellTarget>();
		foreach (var entity in entities)
		{
			if (entity.GetPrefabGUID() == Prefabs.AB_Shared_FireArea)
			{
				Helper.DestroyEntity(entity);
			}
		}
		sender.ReceiveMessage("Extinguished");
	}

	/*var map = Core.gameDataSystem.ItemHashLookupMap;
	itemData.ItemCategory |= ItemCategory.BloodBound;
	map[Prefabs.Item_Headgear_BearTrophy] = itemData;
	*//*var entity = Helper.GetPrefabEntityByPrefabGUID(Prefabs.Item_Headgear_BearTrophy);
	var prefabItemData = entity.Read<ItemData>();
	prefabItemData.ItemCategory |= ItemCategory.BloodBound;
	entity.Write(prefabItemData);*/

	[Command("admin-reset", description: "Used for debugging", adminOnly: true)]
	public void AdminResetCommand(Player sender, Player target = null)
	{
		if (target == null)
		{
			target = sender;
		}
		target.Reset(new Helper.ResetOptions
		{
			ResetCooldowns = true,
			RemoveBuffs = false
		});
	}


	[Command("rat", description: "Makes player a rat", adminOnly: true)]
	public void RatCommand(Player sender, Player player)
	{
		if (!Helper.HasBuff(player, Prefabs.Admin_Observe_Invisible_Buff) && !Helper.HasBuff(player, Prefabs.Admin_Observe_Ghost_Buff) && player.CurrentState == Player.PlayerState.Normal || player.CurrentState == Player.PlayerState.Pacified)
		{
			Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_Rat_Buff, out var buffEntity, Helper.NO_DURATION);
			Helper.FixIconForShapeshiftBuff(player, buffEntity, Prefabs.AB_Shapeshift_Rat_Group);
		}
	}


	[Command("rat-area", description: "Rats players in an area", adminOnly: true)]
	public void RatAreaCommand(Player sender, int distance = 20)
	{
		foreach (var player in PlayerService.OnlinePlayers)
		{
			if (math.distance(player.Position, sender.Position) <= distance)
			{
				if (player == sender) continue;
				if (!Helper.HasBuff(player, Prefabs.Admin_Observe_Invisible_Buff) && !Helper.HasBuff(player, Prefabs.Admin_Observe_Ghost_Buff) && (player.CurrentState == Player.PlayerState.Normal || player.CurrentState == Player.PlayerState.Pacified))
				{
					Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_Rat_Buff, out var buffEntity, Helper.NO_DURATION);
					Helper.FixIconForShapeshiftBuff(player, buffEntity, Prefabs.AB_Shapeshift_Rat_Group);
				}
			}
		}
	}

	[Command("buff-area", description: "Buffs players in an area", adminOnly: true)]
	public void BuffAreaCommand(Player sender, PrefabGUID buffGuid, int distance = 20)
	{
		foreach (var player in PlayerService.OnlinePlayers)
		{
			if (math.distance(player.Position, sender.Position) <= distance)
			{
				if (player == sender) continue;
				if (!Helper.HasBuff(player, Prefabs.Admin_Observe_Invisible_Buff) && !Helper.HasBuff(player, Prefabs.Admin_Observe_Ghost_Buff) && player.CurrentState == Player.PlayerState.Normal || player.CurrentState == Player.PlayerState.Pacified) 
				{
					Helper.BuffPlayer(player, buffGuid, out var buffEntity, Helper.NO_DURATION);
				}
			}
		}
	}

	[Command("tp-area", description: "Teleports players in an area", adminOnly: true)]
	public void TeleportAreaCommand(Player sender, string tpNameOrId, int distance = 20)
	{
		foreach (var player in PlayerService.OnlinePlayers)
		{
			if (math.distance(player.Position, sender.Position) <= distance)
			{
				if (WaypointManager.TryFindWaypoint(tpNameOrId, out var Waypoint))
				{
					player.Teleport(Waypoint.Position);
				}
			}
		}
	}

	[Command("shuffle-teams", description: "Makes everyone in an area join a clan and teleports them into two parallel lines", aliases: new string[] { "st", "shuffle teams" }, adminOnly: true)]
	public void ShuffleTeamsCommand(Player sender, int distance = 25)
	{
		var playersToDivide = new List<Player>();
		foreach (var player in PlayerService.OnlinePlayers)
		{
			if (math.distance(player.Position, sender.Position) <= distance)
			{
				if (!Helper.HasBuff(player, Prefabs.Admin_Observe_Invisible_Buff) && !Helper.HasBuff(player, Prefabs.Admin_Observe_Ghost_Buff) && player.CurrentState == Player.PlayerState.Normal || player.CurrentState == Player.PlayerState.Pacified)
				{
					playersToDivide.Add(player);
				}
			}
		}

		if (playersToDivide.Count >= 2) // Ensure there are at least 2 players to form two clans
		{
			// Set the first two players as clan leaders and remove them from the list
			var clanLeader1 = playersToDivide[0];
			var clanLeader2 = playersToDivide[1];
			playersToDivide.RemoveAt(1); // Remove second leader first to maintain index integrity
			playersToDivide.RemoveAt(0); // Remove first leader

			// Shuffle the remaining players
			System.Random rng = new System.Random();
			int n = playersToDivide.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				Player value = playersToDivide[k];
				playersToDivide[k] = playersToDivide[n];
				playersToDivide[n] = value;
			}

			// Create clans and add the leaders
			Helper.RemoveFromClan(clanLeader1);
			Helper.CreateClanForPlayer(clanLeader1);
			Helper.RemoveFromClan(clanLeader2);
			Helper.CreateClanForPlayer(clanLeader2);

			// Determine starting positions for two parallel lines
			float3 line1StartPos = sender.Position + new float3(5, 0, 0); // 5 units to the right of the sender on the x-axis
			float3 line2StartPos = sender.Position - new float3(5, 0, 0); // 5 units to the left of the sender on the x-axis
			float spacing = 2; // Space between players in a line on the z-axis

			// Teleport clan leaders to the start of each line
			clanLeader1.Teleport(line1StartPos);
			clanLeader2.Teleport(line2StartPos);

			// Assign the rest of the players to clans and teleport them to form two parallel lines
			for (int i = 0; i < playersToDivide.Count; i++)
			{
				Vector3 targetPosition;
				if (i % 2 == 0)
				{
					// Add player to Clan 1 and set position in line 1
					Helper.AddPlayerToPlayerClanForce(playersToDivide[i], clanLeader1);
					targetPosition = line1StartPos + new float3(0, 0, ((i / 2 + 1) * spacing)); // +1 because leader is at the start
				}
				else
				{
					// Add player to Clan 2 and set position in line 2
					Helper.AddPlayerToPlayerClanForce(playersToDivide[i], clanLeader2);
					targetPosition = line2StartPos + new float3(0, 0, ((i / 2 + 1) * spacing)); // +1 because leader is at the start
				}
				playersToDivide[i].Teleport(targetPosition);
			}
		}
		sender.ReceiveMessage("Shuffled.".White());
	}


	[Command("add-region-points", description: "Used for debugging", adminOnly: true)]
	public void AddRegionPoints(Player sender, Player target, int amount)
	{
		target.PlayerPointsData.AddPointsToCurrentRegion(amount);
		sender.ReceiveMessage($"Added {amount} points to {target.Name}".White());
		target.ReceiveMessage($"Received {amount} points".White());
	}

	[Command("kick", description: "Used for debugging", adminOnly: true)]
	public void KickCommand(Player sender, string platformId)
	{
		Helper.KickPlayer(ulong.Parse(platformId));

		sender.ReceiveMessage("Kicked");
	}

	[Command("hurt", description: "Used for debugging", adminOnly: false)]
	public void HurtCommand(Player sender, int amount = 100)
	{
		var statChangeEventEntity = Helper.CreateEntityWithComponents<StatChangeEvent>();
		var statChangeEvent = statChangeEventEntity.Read<StatChangeEvent>();

		statChangeEvent.Change = -Math.Abs(amount);
		statChangeEvent.OriginalChange = amount;
		statChangeEvent.StatChangeFlags |= (int)StatChangeFlag.ShowSct;
		statChangeEvent.StatType = StatType.Health;
		statChangeEvent.Entity = sender.Character;
		statChangeEvent.StatChangeEntity = statChangeEventEntity;

		statChangeEventEntity.Write(statChangeEvent);
	}

	[Command("pacify-target", description: "Used for debugging", adminOnly: true)]
	public void PacifyCommand(Player sender, Player target)
	{
		target.CurrentState = Player.PlayerState.Pacified;
		target.Teleport(sender.Position);
		if (Helper.BuffPlayer(target, Helper.CustomBuff5, out var buffEntity, Helper.NO_DURATION))
		{
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);
		}
		target.ReceiveMessage("You have been pacified.".White());

		sender.ReceiveMessage("Pacified");
	}

	[Command("pacify", description: "Used for debugging", adminOnly: true)]
	public void PacifyCommand(Player sender, int distance = 15)
	{
		var nearbyPlayers = Helper.GetPlayersNearPlayer(sender, distance);

		if (nearbyPlayers.Count > 0)
		{
			float angleStep = 360.0f / nearbyPlayers.Count;
			float radius = 5.0f; // Adjust the radius as needed
			var senderPosition = sender.Position;

			for (int i = 0; i < nearbyPlayers.Count; i++)
			{
				float angle = i * angleStep;
				var teleportPosition = senderPosition + new float3(
					radius * Mathf.Cos(Mathf.Deg2Rad * angle),
					0,
					radius * Mathf.Sin(Mathf.Deg2Rad * angle)
				);

				nearbyPlayers[i].CurrentState = Player.PlayerState.Pacified;
				nearbyPlayers[i].Teleport(teleportPosition);
				if (Helper.BuffPlayer(nearbyPlayers[i], Helper.CustomBuff5, out var buffEntity, Helper.NO_DURATION))
				{
					Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.AbilityCastImpair);
				}
				nearbyPlayers[i].ReceiveMessage("You have been pacified.".White());
			}
		}

		sender.ReceiveMessage("Pacified");
	}

	[Command("unpacify", description: "Used for debugging", adminOnly: true)]
	public void UnpacifyCommand(Player sender, int distance = 15)
	{
		var nearbyPlayers = Helper.GetPlayersNearPlayer(sender, distance);

		foreach (var nearbyPlayer in nearbyPlayers)
		{
			nearbyPlayer.CurrentState = Player.PlayerState.Normal;
			Helper.RemoveBuff(nearbyPlayer, Helper.CustomBuff5);
			nearbyPlayer.ReceiveMessage("You are you no longer pacified.".White());
		}

		sender.ReceiveMessage("Unpacified");
	}

	[Command("unpacify-target", description: "Used for debugging", adminOnly: true)]
	public void UnpacifyCommand(Player sender, Player target)
	{
		target.CurrentState = Player.PlayerState.Normal;
		Helper.RemoveBuff(target, Helper.CustomBuff5);
		target.ReceiveMessage("You are you no longer pacified.".White());

		sender.ReceiveMessage("Unpacified");
	}

	//this needs to be peeled out into the open world mod
	[Command("toggleresources", description: "Used for debugging", adminOnly: true)]
	public void EnableResourcesCommand(Player sender)
	{
		if (Helper.HasBuff(sender, Helper.CustomBuff1))
		{
			var entities = Helper.GetEntitiesByComponentTypes<MapIconTargetEntity, CanFly>();
			foreach (var entity in entities)
			{
				var mapIconTargetEntity = entity.Read<MapIconTargetEntity>();
				if (mapIconTargetEntity.TargetEntity._Entity == sender.User) 
				{
					Helper.DestroyEntity(entity);
					break;
				}
			}

			Helper.RemoveBuff(sender, Helper.CustomBuff1);
		}
		else
		{
			if (Helper.BuffPlayer(sender, Helper.CustomBuff1, out var buffEntity, Helper.NO_DURATION, true))
			{
				Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
				{
					Id = ModificationIdFactory.NewId(),
					ModificationType = ModificationType.Multiply,
					Priority = 100,
					StatType = UnitStatType.ResourcePower,
					Value = 1.25f
				}, true);

				Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
				{
					Id = ModificationIdFactory.NewId(),
					ModificationType = ModificationType.Multiply,
					Priority = 100,
					StatType = UnitStatType.ResourceYield,
					Value = 1.25f
				}, false);
				PrefabSpawnerService.SpawnWithCallback(Prefabs.MapIcon_DraculasCastle, sender.Position, (e) =>
				{
					var mapIconData = e.Read<MapIconData>();
					mapIconData.EnemySetting = MapIconShowSettings.Global;
					mapIconData.AllySetting = MapIconShowSettings.Global;
					mapIconData.RequiresReveal = false;
					mapIconData.ShowOutsideVision = true;
					mapIconData.ShowOnMinimap = true;
					mapIconData.TargetUser = sender.User;
					e.Write(mapIconData);

					var mapIconTargetEntity = e.Read<MapIconTargetEntity>();
					mapIconTargetEntity.TargetEntity = NetworkedEntity.ServerEntity(sender.User);
					mapIconTargetEntity.TargetNetworkId = sender.User.Read<NetworkId>();
					e.Write(mapIconTargetEntity);
				});
			}
		}

		sender.ReceiveMessage("done");
	}

	[Command("destroymapicons", description: "Used for debugging", adminOnly: true)]
	public void DestroyMapIcons(Player sender)
	{
		var entities = Helper.GetEntitiesByComponentTypes<MapIconTargetEntity>();
		foreach (var entity in entities)
		{
			if (entity.GetPrefabGUID() == Prefabs.MapIcon_DraculasCastle)
			{
				Helper.DestroyEntity(entity);
			}
		}
	}

	[Command("become", description: "Used for debugging", adminOnly: true)]
	public void BecomeCommand(Player sender, PrefabGUID prefabGuid = default, Player player = null)
	{
		var target = sender;
		if (player != null)
		{
			target = player;
		}
		if (prefabGuid == default)
		{
			prefabGuid = Prefabs.CHAR_BatVampire_VBlood;
		}
		PrefabSpawnerService.SpawnWithCallback(prefabGuid, target.Position, (e) => 
		{
			Helper.ControlUnit(target, e);
			if (e.Has<UnitLevel>())
			{
				var level = e.Read<UnitLevel>();
				level.Level = 200;
				e.Write(level);
			}
		}, 0, -1);
	}

	[Command("test2", description: "Used for debugging", adminOnly: true)]
	public void Test2Command(Player sender, Player player = null)
	{
		if (Helper.TryGetBuff(sender, Prefabs.AB_Blood_BloodRite_Immaterial, out var buffEntity))
		{
			var lifetime = buffEntity.Read<LifeTime>();
			lifetime.Duration = 1;
			buffEntity.Write(lifetime);
		}
	}

	[Command("test3", description: "Used for debugging", adminOnly: true)]
	public void Test3Command(Player sender)
	{
        sender.User.LogComponentTypes();
        var buffer = sender.User.ReadBuffer<AttachedBuffer>();
        for (var i = 0; i < buffer.Length; i++)
        {
            var attached = buffer[i].Entity;
            if (attached.Read<PrefabGUID>() == Prefabs.ProgressionCollection)
            {
                var buffer2 = attached.ReadBuffer<UnlockedShapeshiftElement>();
                buffer2.Clear();

                var buffer3 = attached.ReadBuffer<UnlockedRecipeElement>();
                buffer3.Clear();

                var buffer4 = attached.ReadBuffer<UnlockedBlueprintElement>();
                buffer4.Clear();

                var buffer5 = attached.ReadBuffer<UnlockedPassiveElement>();
                buffer5.Clear();

                var buffer6 = attached.ReadBuffer<UnlockedAbilityElement>();
                buffer6.Clear();

                var buffer7 = attached.ReadBuffer<UnlockedVBlood>();
                buffer7.Clear();
            }
            else if (attached.Read<PrefabGUID>() == Prefabs.AchievementDataPrefab)
            {
                var buffer2 = attached.ReadBuffer<AchievementInProgressElement>();
                buffer2.Clear();

                var buffer3 = attached.ReadBuffer<AchievementClaimedElement>();
                buffer3.Clear();

                var buffer4 = attached.ReadBuffer<Snapshot_AchievementInProgressElement>();
                buffer4.Clear();
            }
        }
    }

    [Command("set-abilities", description: "Used for debugging", adminOnly: true)]
    public void SetAbilitiesCommand(Player sender, PrefabGUID weapon1, PrefabGUID weapon2, Player player = null)
    {

        if (player == null)
        {
            player = sender;
        }
        Helper.BuffPlayer(player, Helper.CustomBuff2, out var buffEntity, Helper.NO_DURATION);
        var abilityBar = new AbilityBar
        {
            Weapon1 = weapon1,
            Weapon2 = weapon2
        };
        abilityBar.ApplyChangesSoft(buffEntity);
    }

    [Command("test4", description: "Used for debugging", adminOnly: true)]
	public void Test4Command(Player sender)
	{
		Helper.MakeSCT(sender, Prefabs.SCT_Type_InfoMessage, 5);
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

	[Command("enable-building", description: "Gives a player temporary build permissions", adminOnly: true)]
	public void EnableBuildingCommand(Player sender, Player builder)
	{
		Helper.RemoveBuff(builder, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown);
		BuildingPermissions.AuthorizedBuilders[builder] = true;
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
		var entity = Helper.GetHoveredEntity(sender.User);
		if (entity.Has<Health>())
		{
			sender.ReceiveMessage(entity.Read<Health>().Value.ToString().White());
		}
		else
		{
			sender.ReceiveMessage("That entity has no health component.".White());
		}
	}

	[Command(name: "log-zone", description: "Gets the zone assuming you are at the bottom left facing north, right then up", usage: ".get-zone", adminOnly: true, includeInHelp: false)]
	public void LogZoneCommand(Player player, int x, int z)
	{
		player.ReceiveMessage($"{RectangleZone.GetZoneByCurrentCoordinates(player, x, z)}");
		Plugin.PluginLog.LogInfo($"{RectangleZone.GetZoneByCurrentCoordinates(player, x, z)}");
	}

	[Command(name: "check-zone", description: "Does debug things", usage: ".check-zone", adminOnly: true, includeInHelp: false)]
	public void CheckZoneCommand(Player player)
	{
/*		if (DodgeballConfig.Config.Team2Zone.ToRectangleZone().Contains(player))
		{
			player.ReceiveMessage("in zone".Success());
		}
		else
		{
			player.ReceiveMessage("not in zone".Error());
		}*/
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
		Entity entity = Helper.GetHoveredEntity(sender.User);
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
		var entity = Helper.GetHoveredEntity(sender.User);

		entity.Write(sender.Character.Read<Team>());
		entity.Write(sender.Character.Read<TeamReference>());
	}

	[Command("make-sct", description: "Used for debugging", adminOnly: true)]
	public static void MakeSctCommand(Player sender, PrefabGUID sctPrefab)
	{
		Helper.MakeSCT(sender, sctPrefab);
	}
	
}
