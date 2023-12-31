using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Debugging;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using PvpArena.Configs;
using PvpArena.GameModes;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Dodgeball;
using PvpArena.GameModes.Domination;
using PvpArena.GameModes.Matchmaking1v1;
using PvpArena.GameModes.Moba;
using PvpArena.GameModes.OD;
using PvpArena.GameModes.Pacified;
using PvpArena.GameModes.Prison;
using PvpArena.GameModes.PrisonBreak;
using PvpArena.GameModes.Troll;
using PvpArena.Listeners;
using PvpArena.Models;
using PvpArena.Patches;
using PvpArena.Persistence.MySql;
using PvpArena.Persistence.MySql.AllDatabases;
using PvpArena.Persistence.MySql.MainDatabase;
using PvpArena.Persistence.MySql.PlayerDatabase;
using PvpArena.Services;
using Unity.Entities;
using static PvpArena.Configs.ConfigDtos;
using static PvpArena.PrefabSpawnerService;

namespace PvpArena;

public static class Core
{
	public static PlayerMatchmaking1v1DataStorage matchmaking1V1DataRepository;
	public static PlayerPointsStorage pointsDataRepository;
	public static PlayerMuteInfoStorage muteDataRepository;
	public static PlayerBanInfoStorage banDataRepository;
	public static PlayerImprisonInfoStorage imprisonDataRepository;
	public static PlayerConfigOptionsStorage playerConfigOptionsRepository;
	public static PlayerBulletHellDataStorage playerBulletHellDataRepository;
    public static DefaultJewelDataStorage defaultJewelStorage;
    public static DefaultLegendaryWeaponDataStorage defaultLegendaryWeaponStorage;
	public static MatchmakingArenasDataStorage matchmakingArenaStorage;
	public static List<ArenaLocationDto> matchmaking1v1ArenaLocations;

	public static DefaultGameMode defaultGameMode;
	public static DominationGameMode dominationGameMode;
	public static Matchmaking1v1GameMode matchmaking1v1GameMode;
    public static SpectatingGameMode spectatingGameMode;
	public static PrisonGameMode prisonGameMode;
	public static DodgeballGameMode dodgeballGameMode;
	public static PrisonBreakGameMode prisonBreakGameMode;
	public static NoHealingLimitGameMode noHealingLimitGameMode;
	public static MobaGameMode mobaGameMode;
	public static TrollGameMode trollGameMode;
	public static PacifiedGameMode pacifiedGameMode;

	public static bool HasInitialized = false;
	public static SQLHandler sqlHandler;
	public static DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
	public static NetworkIdSystem networkIdSystem = VWorld.Server.GetExistingSystem<NetworkIdSystem>();
	public static JewelSpawnSystem jewelSpawnSystem = VWorld.Server.GetExistingSystem<JewelSpawnSystem>();
	public static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();
	public static PrefabCollectionSystem prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
	public static ClanSystem_Server clanSystem = VWorld.Server.GetExistingSystem<ClanSystem_Server>();
	public static EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
	public static TraderSyncSystem traderSyncSystem = VWorld.Server.GetExistingSystem<TraderSyncSystem>();
	public static ServerBootstrapSystem serverBootstrapSystem = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();
	public static GameDataSystem gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
	public static ModificationSystem modificationSystem = VWorld.Server.GetExistingSystem<ModificationSystem>();
	public static GameplayEventDebuggingSystem gameplayEventDebuggingSystem = VWorld.Server.GetExistingSystem<GameplayEventDebuggingSystem>();
	public static GameplayEventsSystem gameplayEventsSystem = VWorld.Server.GetExistingSystem<GameplayEventsSystem>();

	public static void Initialize()
	{
		if (HasInitialized) return;
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		
		DiscordBotConfig.Load();
		DiscordBot.InitializeAsync();
		PlayerService.Initialize();
		PvpArenaConfig.Load();
		PlayerJewels.Load();
		PlayerLegendaries.Load();
		CaptureThePancakeConfig.Load();
		DominationConfig.Load();
		PrisonConfig.Load();
        TradersConfig.Load();
		DodgeballConfig.Load();
		PrisonBreakConfig.Load();

		matchmaking1V1DataRepository = new PlayerMatchmaking1v1DataStorage(PvpArenaConfig.Config.ServerDatabase);
		matchmaking1V1DataRepository.LoadDataAsync();

		pointsDataRepository = new PlayerPointsStorage(PvpArenaConfig.Config.MainDatabase);
		pointsDataRepository.LoadDataAsync();

		banDataRepository = new PlayerBanInfoStorage(PvpArenaConfig.Config.MainDatabase);
		banDataRepository.LoadDataAsync();

		muteDataRepository = new PlayerMuteInfoStorage(PvpArenaConfig.Config.MainDatabase);
		muteDataRepository.LoadDataAsync();

		imprisonDataRepository = new PlayerImprisonInfoStorage(PvpArenaConfig.Config.MainDatabase);
		imprisonDataRepository.LoadDataAsync();

		playerConfigOptionsRepository = new PlayerConfigOptionsStorage(PvpArenaConfig.Config.MainDatabase);
		playerConfigOptionsRepository.LoadDataAsync();

		playerBulletHellDataRepository = new PlayerBulletHellDataStorage(PvpArenaConfig.Config.MainDatabase);
		playerBulletHellDataRepository.LoadDataAsync();

		defaultJewelStorage = new DefaultJewelDataStorage(PvpArenaConfig.Config.MainDatabase, PvpArenaConfig.Config.ServerDatabase);
		defaultJewelStorage.LoadAllJewelDataAsync();

		defaultLegendaryWeaponStorage = new DefaultLegendaryWeaponDataStorage(PvpArenaConfig.Config.MainDatabase, PvpArenaConfig.Config.ServerDatabase);
		defaultLegendaryWeaponStorage.LoadAllLegendaryDataAsync();

		matchmakingArenaStorage = new MatchmakingArenasDataStorage(PvpArenaConfig.Config.MainDatabase);
		matchmaking1v1ArenaLocations = matchmakingArenaStorage.LoadAllArenasAsync().Result;
		
		EntityQueryOptions options = EntityQueryOptions.Default;

		EntityQueryDesc queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<CanFly>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};
		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		Listener.AddListener(query, new ManuallySpawnedPrefabListener());

		queryDesc = new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				new ComponentType(Il2CppType.Of<TargetAoE>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<HitColliderCast>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};
		query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		Listener.AddListener(query, new TargetAoeListener());

		//this is a listener for debugging to find which events get triggered when you do things in-game
		/*		queryDesc = new EntityQueryDesc
				{
					All = new ComponentType[]
					{
								new ComponentType(Il2CppType.Of<FromCharacter>(), ComponentType.AccessMode.ReadWrite)
					},
					Options = options
				};
				query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
				Listener.AddListener(query, new FromCharacterListener());*/

		FileRoleStorage.Initialize();

		defaultGameMode = new DefaultGameMode();
		defaultGameMode.Initialize();

		pacifiedGameMode = new PacifiedGameMode();
		pacifiedGameMode.Initialize();

		if (PvpArenaConfig.Config.MatchmakingEnabled)
		{
			MatchmakingService.Initialize();
		}

		matchmaking1v1GameMode = new Matchmaking1v1GameMode();
		matchmaking1v1GameMode.Initialize();

		spectatingGameMode = new SpectatingGameMode();
		spectatingGameMode.Initialize();

		CaptureThePancakeManager.Initialize();

		dominationGameMode = new DominationGameMode();

		BulletHellManager.Initialize();

		dodgeballGameMode = new DodgeballGameMode();

		prisonGameMode = new PrisonGameMode();
		prisonGameMode.Initialize();

		prisonBreakGameMode = new PrisonBreakGameMode();

		noHealingLimitGameMode = new NoHealingLimitGameMode();

		trollGameMode = new TrollGameMode();

		DummyHandler.Initialize();
		PlayerSpawnHandler.Initialize();

		mobaGameMode = new MobaGameMode();

		if (PvpArenaConfig.Config.PointSystemEnabled)
		{
			LoginPointsService.Initialize();
			ScheduleAnnouncementService.Initialize();
		}

		HasInitialized = true;
	}

	public static void Dispose()
	{
		HasInitialized = false;
		PlayerService.Dispose();
		BulletHellManager.Dispose();
        defaultGameMode.Dispose();
		pacifiedGameMode.Dispose();
		matchmaking1v1GameMode.Dispose();
		ODManager.Dispose();
        CaptureThePancakeManager.Dispose();
		if (dominationGameMode != null)
		{
			DominationHelper.EndMatch();
		}
		if (dodgeballGameMode != null)
		{
			DodgeballHelper.EndMatch();
		}
		if (prisonBreakGameMode != null)
		{
			PrisonBreakHelper.EndMatch();
		}
		if (mobaGameMode != null)
		{
			MobaHelper.EndMatch();
		}
		
        spectatingGameMode.Dispose();
		prisonGameMode.Dispose();
		DummyHandler.Dispose();
		PlayerSpawnHandler.Dispose();
		TrollModeManager.Dispose();
		noHealingLimitGameMode.Dispose();
		LoginPointsService.Dispose();
		ScheduleAnnouncementService.Dispose();
		
		Listener.Dispose();
		DiscordBot.Dispose();
	}
}

