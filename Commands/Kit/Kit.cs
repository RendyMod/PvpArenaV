using System.Collections.Generic;
using ProjectM;

namespace PvpArena.Data;

public static class Kit
{
    public static List<PrefabGUID> servantGear = new List<PrefabGUID>
    {
        Prefabs.Item_Weapon_Spear_Legendary_T08,
        Prefabs.Item_Boots_T08_Shadowmoon,
        Prefabs.Item_Chest_T08_Shadowmoon,
        Prefabs.Item_Gloves_T08_Shadowmoon,
        Prefabs.Item_Legs_T08_Shadowmoon,
        Prefabs.Item_MagicSource_General_T08_Delusion,
    };

	public static List<PrefabGUID> StartingGear = new List<PrefabGUID>
	{
		Prefabs.Item_Boots_T08_Shadowmoon,
		Prefabs.Item_Chest_T08_Shadowmoon,
		Prefabs.Item_Gloves_T08_Shadowmoon,
		Prefabs.Item_Legs_T08_Shadowmoon,
		Prefabs.Item_Cloak_Main_T03_Phantom,
	};

	public static List<PrefabGUID> Necks = new List<PrefabGUID>
	{
		Prefabs.Item_MagicSource_General_T08_Delusion,
		Prefabs.Item_MagicSource_General_T08_FrozenCrypt,
		Prefabs.Item_MagicSource_General_T08_Beast,
		Prefabs.Item_MagicSource_General_T08_CrimsonSky,
		Prefabs.Item_MagicSource_General_T08_Madness,
		Prefabs.Item_MagicSource_General_T08_WickedProphet,
	};

	public static List<PrefabGUID> SanguineWeapons = new List<PrefabGUID>
	{
		Prefabs.Item_Weapon_Slashers_T08_Sanguine,
		Prefabs.Item_Weapon_Spear_T08_Sanguine,
		Prefabs.Item_Weapon_Axe_T08_Sanguine,
		Prefabs.Item_Weapon_GreatSword_T08_Sanguine,
		Prefabs.Item_Weapon_Crossbow_T08_Sanguine,
		Prefabs.Item_Weapon_Pistols_T08_Sanguine,
		Prefabs.Item_Weapon_Reaper_T08_Sanguine,
		Prefabs.Item_Weapon_Sword_T08_Sanguine,
		Prefabs.Item_Weapon_Mace_T08_Sanguine,
	};
}
