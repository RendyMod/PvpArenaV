using System;
using ProjectM;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Services.Core;

namespace PvpArena.Frameworks.CommandFramework;

public interface IArgumentConverter
{
	bool TryConvert(string input, Type targetType, out object result);
}

public interface IArgumentConverter<T> : IArgumentConverter
{
	bool TryConvert(string input, out T result);
}

public class IntegerConverter : IArgumentConverter<int>
{
	public bool TryConvert(string input, out int result)
	{
		return int.TryParse(input, out result);
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out int intResult);
		result = intResult;
		return success;
	}
}

public class BooleanConverter : IArgumentConverter<bool>
{
	public bool TryConvert(string input, out bool result)
	{
		return bool.TryParse(input, out result);
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out bool boolResult);
		result = boolResult;
		return success;
	}
}


public class FloatConverter : IArgumentConverter<float>
{
	public bool TryConvert(string input, out float result)
	{
		return float.TryParse(input, out result);
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out float floatResult);
		result = floatResult;
		return success;
	}
}


public class PlayerConverter : IArgumentConverter<Player>
{
	public bool TryConvert(string input, out Player result)
	{
		return PlayerService.TryGetPlayerFromString(input, out result);
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out Player playerResult);
		result = playerResult;
		return success;
	}
}

public class PrefabGUIDConverter : IArgumentConverter<PrefabGUID>
{
	public bool TryConvert(string input, out PrefabGUID result)
	{
		return Helper.TryGetPrefabGUIDFromString(input, out result);
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out PrefabGUID prefabGUIDResult);
		result = prefabGUIDResult;
		return success;
	}
}

public class ItemPrefabDataConverter : IArgumentConverter<ItemPrefabData>
{
	public bool TryConvert(string input, out ItemPrefabData result)
	{
		if (Helper.TryGetPrefabDataFromString(input, Data.Items.GiveableItemsPrefabData, out var prefabData))
		{
			result = new ItemPrefabData(prefabData.PrefabGUID, prefabData.FormalPrefabName, prefabData.OverrideName);
			return true;
		}

		result = null;
		return false;
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out ItemPrefabData itemPrefabDataResult);
		result = itemPrefabDataResult;
		return success;
	}
}

public class BloodPrefabDataConverter : IArgumentConverter<BloodPrefabData>
{
	public bool TryConvert(string input, out BloodPrefabData result)
	{
		if (Helper.TryGetPrefabDataFromString(input, Data.BloodData.BloodPrefabData, out var prefabData))
		{
			result = new BloodPrefabData(prefabData.PrefabGUID, prefabData.FormalPrefabName, prefabData.OverrideName);
			return true;
		}

		result = null;
		return false;
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out BloodPrefabData bloodPrefabDataResult);
		result = bloodPrefabDataResult;
		return success;
	}
}

public class JewelPrefabDataConverter : IArgumentConverter<JewelPrefabData>
{
	public bool TryConvert(string input, out JewelPrefabData result)
	{
		if (Helper.TryGetPrefabDataFromString(input, Data.JewelData.JewelPrefabData, out var prefabData))
		{
			result = new JewelPrefabData(prefabData.PrefabGUID, prefabData.FormalPrefabName, prefabData.OverrideName);
			return true;
		}

		result = null;
		return false;
	}

	public bool TryConvert(string input, Type targetType, out object result)
	{
		bool success = TryConvert(input, out JewelPrefabData jewelPrefabDataResult);
		result = jewelPrefabDataResult;
		return success;
	}
}
