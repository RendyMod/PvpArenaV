using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;

namespace PvpArena.Data;
public class ShardData
{
	public string ShardName { get; private set; }
	public PrefabGUID BuffPrefabGUID { get; private set; }
	public PrefabGUID ItemPrefabGUID { get; private set; }

	public ShardData(string shardName, PrefabGUID buffPrefabGUID, PrefabGUID itemPrefabGUID)
	{
		ShardName = shardName;
		BuffPrefabGUID = buffPrefabGUID;
		ItemPrefabGUID = itemPrefabGUID;
	}
}

public static class Shards
{
	public static ShardData Monster = new ShardData("Monster Shard", Prefabs.AB_Interact_UseRelic_Monster_Buff, Prefabs.Item_Building_Relic_Monster);
	public static ShardData WingedHorror = new ShardData("Winged Horror Shard", Prefabs.AB_Interact_UseRelic_Manticore_Buff, Prefabs.Item_Building_Relic_Manticore);
	public static ShardData Behemoth = new ShardData("Behemoth Shard", Prefabs.AB_Interact_UseRelic_Behemoth_Buff, Prefabs.Item_Building_Relic_Behemoth);
	public static ShardData Solarus = new ShardData("Solarus Shard", Prefabs.AB_Interact_UseRelic_Paladin_Buff, Prefabs.Item_Building_Relic_Paladin);
	public static List<ShardData> AllShards = new List<ShardData>
	{
		Shards.Monster, Shards.WingedHorror, Shards.Behemoth, Shards.Solarus
	};
}
