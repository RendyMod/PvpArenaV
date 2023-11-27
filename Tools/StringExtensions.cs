using System.Text;
using UnityEngine;

public static class StringExtensions
{
	private static StringBuilder m_stringBuilder = new StringBuilder();

	public static string Bold (this string _string)
	{
		m_stringBuilder.Clear();
		m_stringBuilder.Append("<b>");
		m_stringBuilder.Append(_string);
		m_stringBuilder.Append("</b>");
		return m_stringBuilder.ToString();
	}

	public static string Italic (this string _string)
	{
		m_stringBuilder.Clear();
		m_stringBuilder.Append("<i>");
		m_stringBuilder.Append(_string);
		m_stringBuilder.Append("</i>");
		return m_stringBuilder.ToString();
	}

	public static string Colorify (this string _string, Color _color)
	{
		m_stringBuilder.Clear();
		m_stringBuilder.Append("<color=#");
		m_stringBuilder.Append(_color.ToHexString());
		m_stringBuilder.Append('>');
		m_stringBuilder.Append(_string);
		m_stringBuilder.Append("</color>");
		return m_stringBuilder.ToString();
	}

	public static string Emphasize  (this string _string)
	{
		return _string.Colorify(ExtendedColor.AntiqueWhite);
	}
	
	public static string White  (this string _string)
	{
		return _string.Colorify(ExtendedColor.LightGray);
	}
	
	public static string Error (this string _string)
	{
		return (_string).Colorify(ExtendedColor.Crimson);
		return (_string + " ㄨ").Colorify(ExtendedColor.Crimson);
	}
	
	public static string Warning (this string _string)
	{
		return ( _string).Colorify(ExtendedColor.ReunoYellow);
		return ( _string + " ⚠").Colorify(ExtendedColor.ReunoYellow);
	}
	
	public static string Success (this string _string)
	{
		return (_string).Colorify(ExtendedColor.MediumSeaGreen);
		return (_string + " ✓" ).Colorify(ExtendedColor.LimeGreen);
	}

	public static string FriendlyTeam(this string _string)
	{
		return Success(_string);
	}

	public static string EnemyTeam(this string _string)
	{
		return Error(_string);
	}

	public static string NeutralTeam(this string _string)
	{
		return Warning(_string);
	}
}
