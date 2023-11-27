using System;
using System.Reflection;
using Bloodstone.API;
using Epic.OnlineServices;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using PvpArena.Configs;
using PvpArena.GameModes;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Domination;
using PvpArena.Listeners;
using PvpArena.Models;
using PvpArena.Persistence.Json;
using PvpArena.Persistence.MySql;
using PvpArena.Services;
using Unity.Entities;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using static PvpArena.PrefabSpawnerService;

namespace PvpArena;

public static class Core
{
	public static IDataStorage<PlayerMatchmaking1v1Data> matchmaking1V1DataRepository;
	public static IDataStorage<PlayerPoints> pointsDataRepository;
	public static IDataStorage<PlayerMuteInfo> muteDataRepository;
	public static IDataStorage<PlayerBanInfo> banDataRepository;
	public static IDataStorage<PlayerConfigOptions> playerConfigOptionsRepository;
	public static DefaultGameMode defaultGameMode;
	public static CaptureThePancakeGameMode captureThePancakeGameMode;
	public static DominationGameMode dominationGameMode;
	public static BulletHellGameMode bulletHellGameMode;
	public static Matchmaking1v1GameMode matchmaking1v1GameMode;
    public static SpectatingGameMode spectatingGameMode;
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
		CaptureThePancakeConfig.Load();
		DominationConfig.Load();
		if (PvpArenaConfig.Config.Database.UseDatabaseStorage)
		{
			matchmaking1V1DataRepository = new PlayerMatchmaking1v1DataStorage();
			pointsDataRepository = new PlayerPointsStorage();
			banDataRepository = new PlayerBanInfoStorage();
			muteDataRepository = new PlayerMuteInfoStorage();
			playerConfigOptionsRepository = new PlayerConfigOptionsStorage();
		}
		else
		{
			matchmaking1V1DataRepository = new PlayerJsonDataStorage<PlayerMatchmaking1v1Data>("BepInEx/config/PvpArena/matchmaking_1v1_data.json");
			pointsDataRepository = new PlayerJsonDataStorage<PlayerPoints>("BepInEx/config/PvpArena/login_points.json");
			banDataRepository = new PlayerJsonDataStorage<PlayerBanInfo>("Bepinex/config/PvpArena/banned_players.json");
			muteDataRepository = new PlayerJsonDataStorage<PlayerMuteInfo>("Bepinex/config/PvpArena/muted_steam_ids.json");
			playerConfigOptionsRepository = new PlayerJsonDataStorage<PlayerConfigOptions>("Bepinex/config/PvpArena/player_config_options.json");
		}
		

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
		
		/*sqlHandler = new SQLHandler();
		sqlHandler.InitializeAsync();*/

		matchmaking1V1DataRepository.LoadDataAsync();
		pointsDataRepository.LoadDataAsync();
		banDataRepository.LoadDataAsync();
		muteDataRepository.LoadDataAsync();
		playerConfigOptionsRepository.LoadDataAsync();

		defaultGameMode = new DefaultGameMode();
		defaultGameMode.Initialize();

		matchmaking1v1GameMode = new Matchmaking1v1GameMode();
		matchmaking1v1GameMode.Initialize();

		spectatingGameMode = new SpectatingGameMode();
		spectatingGameMode.Initialize();

		captureThePancakeGameMode = new CaptureThePancakeGameMode(); //wait until a match has started to initialize
		dominationGameMode = new DominationGameMode();
		bulletHellGameMode = new BulletHellGameMode();

		if (PvpArenaConfig.Config.PointSystemEnabled)
		{
			LoginPointsService.SetTimersForOnlinePlayers();
		}

		HasInitialized = true;
	}

	public static void Dispose()
	{
        defaultGameMode.Dispose();
        matchmaking1v1GameMode.Dispose();
        if (captureThePancakeGameMode != null)
        {
            BulletHellhelper.EndMatch();
        }
        spectatingGameMode.Dispose();
		PersistPlayerSubData();
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

