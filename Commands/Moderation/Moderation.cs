using System.Text;

public static class Moderation 
{
	public static string BanMuteMessage (bool _recipientIsTheCriminal, bool _isMute, string _banishedName, string _steamID, int _days = -1, string _reason = "")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_recipientIsTheCriminal ? ("You have been " + (_isMute ? "muted" : "banned") + ".").Error() : (_isMute ? "muted " : "banned ") + _banishedName + ".");
		stringBuilder.Append(" - Duration: " + (_days == -1 ? "Permanently" : (_days.ToString() + " day(s)")) + ".");
		stringBuilder.Append(_recipientIsTheCriminal ? "" : " - Steam ID: " + _steamID);
		stringBuilder.Append(" - Reason: " + (_reason == "" ? "No reason given" : _reason) + ".");
		stringBuilder.Append(_isMute && _recipientIsTheCriminal ? " - Muting requires a temporary kick from the server, but you can rejoin." : "");
		
		return stringBuilder.ToString();
	}
}
