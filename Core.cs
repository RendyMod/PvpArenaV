using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using PvpArena.Configs;
using PvpArena.GameModes;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Dodgeball;
using PvpArena.GameModes.Domination;
using PvpArena.GameModes.Matchmaking1v1;
using PvpArena.GameModes.Prison;
using PvpArena.Listeners;
using PvpArena.Models;
using PvpArena.Patches;
using PvpArena.Persistence.Json;
using PvpArena.Persistence.MySql;
using PvpArena.Persistence.MySql.AllDatabases;
using PvpArena.Persistence.MySql.PlayerDatabase;
using PvpArena.Services;
using Unity.Entities;
using static PvpArena.Configs.ConfigDtos;
using static PvpArena.PrefabSpawnerService;

namespace PvpArena;

public static class Core
{
	public static IDataStorage<PlayerMatchmaking1v1Data> matchmaking1V1DataRepository;
	public static IDataStorage<PlayerPoints> pointsDataRepository;
	public static IDataStorage<PlayerMuteInfo> muteDataRepository;
	public static IDataStorage<PlayerBanInfo> banDataRepository;
	public static IDataStorage<PlayerImprisonInfo> imprisonDataRepository;
	public static IDataStorage<PlayerConfigOptions> playerConfigOptionsRepository;
	public static IDataStorage<PlayerBulletHellData> playerBulletHellDataRepository;
    public static DefaultJewelDataStorage defaultJewelStorage;
    public static DefaultLegendaryWeaponDataStorage defaultLegendaryWeaponStorage;
	public static MatchmakingArenasDataStorage matchmakingArenaStorage;
	public static List<ArenaLocationDto> matchmaking1v1ArenaLocations;

	public static DefaultGameMode defaultGameMode;
	public static CaptureThePancakeGameMode captureThePancakeGameMode;
	public static DominationGameMode dominationGameMode;
	public static Matchmaking1v1GameMode matchmaking1v1GameMode;
    public static SpectatingGameMode spectatingGameMode;
	public static PrisonGameMode prisonGameMode;
	public static DodgeballGameMode dodgeballGameMode;
	public static bool HasInitialized = false;
	/*public static DiscordBot discordBot;*/
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
	
	public static void Initialize()
	{
		if (HasInitialized) return;
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		
		PlayerService.LoadAllPlayers();
		PvpArenaConfig.Load();
		PlayerJewels.Load();
		PlayerLegendaries.Load();
		CaptureThePancakeConfig.Load();
		DominationConfig.Load();
		PrisonConfig.Load();
        TradersConfig.Load();
		DodgeballConfig.Load();

		matchmaking1V1DataRepository = new PlayerMatchmaking1v1DataStorage(PvpArenaConfig.Config.ServerDatabase);
		pointsDataRepository = new PlayerPointsStorage(PvpArenaConfig.Config.ServerDatabase);
		banDataRepository = new PlayerBanInfoStorage(PvpArenaConfig.Config.ServerDatabase);
		muteDataRepository = new PlayerMuteInfoStorage(PvpArenaConfig.Config.ServerDatabase);
		imprisonDataRepository = new PlayerImprisonInfoStorage(PvpArenaConfig.Config.ServerDatabase);
		playerConfigOptionsRepository = new PlayerConfigOptionsStorage(PvpArenaConfig.Config.ServerDatabase);
		playerBulletHellDataRepository = new PlayerBulletHellDataStorage(PvpArenaConfig.Config.ServerDatabase);
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
				new ComponentType(Il2CppType.Of<CastleHeartConnection>(), ComponentType.AccessMode.ReadWrite),
				new ComponentType(Il2CppType.Of<CanFly>(), ComponentType.AccessMode.ReadWrite)
			},
			Options = options
		};
		var query = VWorld.Server.EntityManager.CreateEntityQuery(queryDesc);
		Listener.AddListener(query, new ManuallySpawnedStructureListener());

		/*discordBot = new DiscordBot();
		discordBot.InitializeAsync();*/

		matchmaking1V1DataRepository.LoadDataAsync();
		pointsDataRepository.LoadDataAsync();
		banDataRepository.LoadDataAsync();
		muteDataRepository.LoadDataAsync();
		imprisonDataRepository.LoadDataAsync();
		playerConfigOptionsRepository.LoadDataAsync();
		playerBulletHellDataRepository.LoadDataAsync();

		defaultGameMode = new DefaultGameMode();
		defaultGameMode.Initialize();

		matchmaking1v1GameMode = new Matchmaking1v1GameMode();
		matchmaking1v1GameMode.Initialize();

		spectatingGameMode = new SpectatingGameMode();
		spectatingGameMode.Initialize();

		captureThePancakeGameMode = new CaptureThePancakeGameMode(); //wait until a match has started to initialize

		dominationGameMode = new DominationGameMode();

		BulletHellManager.Initialize();

		dodgeballGameMode = new DodgeballGameMode();

		prisonGameMode = new PrisonGameMode();
		prisonGameMode.Initialize();

		if (PvpArenaConfig.Config.PointSystemEnabled)
		{
			LoginPointsService.SetTimersForOnlinePlayers();
		}

		HasInitialized = true;
	}

	public static void Dispose()
	{
		HasInitialized = false;
		BulletHellManager.Dispose();
        defaultGameMode.Dispose();
        matchmaking1v1GameMode.Dispose();
        if (captureThePancakeGameMode != null)
        {
            CaptureThePancakeHelper.EndMatch();
        }
		if (dominationGameMode != null)
		{
			DominationHelper.EndMatch();
		}
		if (dodgeballGameMode != null)
		{
			DodgeballHelper.EndMatch();
		}
		
        spectatingGameMode.Dispose();
		prisonGameMode.Dispose();
		LoginPointsService.DisposeTimersForOnlinePlayers();
		PersistPlayerSubData();
		Listener.Dispose();
	}

	public static void PersistPlayerSubData()
	{
		try
		{
			var matchmakingData = PlayerService.GetPlayerSubData<PlayerMatchmaking1v1Data>();
			matchmaking1V1DataRepository.SaveDataAsync(matchmakingData); //saved on autosave and on plugin reload

			var pointsData = PlayerService.GetPlayerSubData<PlayerPoints>();
			Unity.Debug.Log($"{pointsData[0].SteamID} {pointsData[0].TotalPoints}");
			pointsDataRepository.SaveDataAsync(pointsData);

			var banInfo = PlayerService.GetPlayerSubData<PlayerBanInfo>();
			banDataRepository.SaveDataAsync(banInfo);

			var muteInfo = PlayerService.GetPlayerSubData<PlayerMuteInfo>();
			muteDataRepository.SaveDataAsync(muteInfo);

			var playerConfigOptions = PlayerService.GetPlayerSubData<PlayerConfigOptions>();
			playerConfigOptionsRepository.SaveDataAsync(playerConfigOptions);
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogError(e.ToString());
		}
	}
}

