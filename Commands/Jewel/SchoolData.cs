using UnityEngine;

public class SchoolData
{
	public enum Type
	{
		Blood,
		Chaos,
		Frost,
		Storm,
		Illusion,
		Unholy,
	}

	public Type type;
	public Color lightColor;
	public Color color;
	
	public SchoolData ()
	{
		
	}
	
	public static SchoolData Blood = new SchoolData()
	{
		type = Type.Blood,
		lightColor = ExtendedColor.LightBlood,
		color = ExtendedColor.Blood,
	};
	
	public static SchoolData Chaos = new SchoolData()
	{
		type = Type.Chaos,
		lightColor = ExtendedColor.LightChaos,
		color = ExtendedColor.Chaos,
	};
	
	public static SchoolData Frost = new SchoolData()
	{
		type = Type.Frost,
		lightColor = ExtendedColor.LightFrost,
		color = ExtendedColor.Frost,
	};
	
	public static SchoolData Storm = new SchoolData()
	{
		type = Type.Storm,
		lightColor = ExtendedColor.LightStorm,
		color = ExtendedColor.Storm,
	};
	
	public static SchoolData Illusion = new SchoolData()
	{
		type = Type.Illusion,
		lightColor = ExtendedColor.LightIllusion,
		color = ExtendedColor.Illusion,
	};
	
	public static SchoolData Unholy = new SchoolData()
	{
		type = Type.Unholy,
		lightColor = ExtendedColor.LightUnholy,
		color = ExtendedColor.Unholy,
	};
}
