using System;

namespace PvpArena.GameModes.Matchmaking1v1;

public static class MmrCalculator
{
	private const int K = 32; // You can adjust this value to make rating changes more or less aggressive
	public static int StartingMmr = 1500;

	public static int CalculateNewMmr(int currentMmr, int opponentMmr, bool isWin)
	{
		double expectedScore = 1 / (1 + Math.Pow(10, (opponentMmr - currentMmr) / 400.0));
		double actualScore = isWin ? 1 : 0;

		int newMmr = currentMmr + (int)(K * (actualScore - expectedScore));
		return newMmr;
	}
}
