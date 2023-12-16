using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PvpArena.Models;

namespace PvpArena.Models
{
	public class BreakpointControl
	{
		public int[] Breakpoints = new int[4]; // Assuming 5 breakpoints

		public BreakpointControl()
		{
			Reset();
		}

		public void Reset()
		{
			for (int i = 0; i < Breakpoints.Length; i++)
			{
				Breakpoints[i] = 0; // 0 indicates neutral
			}
		}

		public void SetBreakpoint(int breakpoint, int teamId)
		{
			if (breakpoint >= 0 && breakpoint < Breakpoints.Length)
			{
				Breakpoints[breakpoint] = teamId;
			}
		}

		public bool IsFullyCapturedByTeam(int teamId)
		{
			return Breakpoints.All(bp => bp == teamId);
		}

		public override string ToString()
		{
			var message = "";
			foreach (var breakpoint in Breakpoints)
			{
				message += breakpoint + " ";
			}
			return message;
		}
	}


	public class CapturePoint
	{
		public bool IsActive = true;
		public RectangleZone Zone { get; private set; }
		public int PointIndex { get; private set; }
		private int controllingTeamId = 0; // 0 for neutral, 1 for team1, 2 for team2
		private Stopwatch timer;
		private float breakpointCaptureDelay; // Time to capture from one state to another
		private Dictionary<int, float> breakpointCaptureTimers = new Dictionary<int, float>
		{
			{ 0, 0 },
			{ 1, 0 },
			{ 2, 0 },
		};
		private float breakpointCaptureTimer = 0.0f;
		private float pointAwardTimer = 0.0f;
		private const float pointAwardInterval = 10.0f; // Interval for awarding points
		private BreakpointControl breakpointControl = new BreakpointControl();

		public delegate void PointCapturedHandler(int pointIndex, int previousControllingTeam, int newControllingTeamId);
		public event PointCapturedHandler OnPointCaptured;

		public delegate void CaptureProgressHandler(int pointIndex, int gainingTeamId, int controllingTeamId, int breakpoint);
		public event CaptureProgressHandler OnCaptureProgress;


		public CapturePoint(RectangleZone zone, int pointIndex, float timeToCompletelyCapture = 5, int numOfIntervals = 4)
		{
			Zone = zone;
			timer = new Stopwatch();
			timer.Start();
			PointIndex = pointIndex;
			breakpointCaptureDelay = (timeToCompletelyCapture / numOfIntervals);
		}

		public void ResetCapturePoint()
		{
			// Reset controlling team to neutral
			controllingTeamId = 0;

			// Reset timers
			timer.Restart();
			breakpointCaptureTimer = 0.0f;
			pointAwardTimer = 0.0f;

			// Reset breakpoint capture timers for both teams
			breakpointCaptureTimers[1] = 0;
			breakpointCaptureTimers[2] = 0;

			// Reset the BreakpointControl state
			breakpointControl.Reset();

			// Set the capture point as inactive
			IsActive = false;
		}

		public void Update(List<Player> team1Players, List<Player> team2Players, Action<int> awardPointsCallback = null)
		{
			if (!IsActive) return;

			// Calculate deltaTime
			float deltaTime = (float)timer.Elapsed.TotalSeconds;
			timer.Restart();

			// Check for players in zone
			var playersInZone = team1Players.Concat(team2Players)
								.Where(player => Zone.Contains(player) && player.IsAlive);

			// Determine team presence
			bool team1Present = playersInZone.Any(player => player.IsAlliedWith(team1Players[0]));
			bool team2Present = playersInZone.Any(player => player.IsAlliedWith(team2Players[0]));

			int attemptingTeamId = 0;
			if (team1Present && !team2Present) attemptingTeamId = 1;
			if (team2Present && !team1Present) attemptingTeamId = 2;

			bool isContested = team1Present && team2Present;

			// Handle capturing, contesting, or losing progress logic
			if (!isContested)
			{
				breakpointCaptureTimers[attemptingTeamId] += deltaTime;
				if (breakpointCaptureTimers[attemptingTeamId] >= breakpointCaptureDelay)
				{
					CheckAndUpdateBreakpoint(attemptingTeamId); // Update breakpoint
					breakpointCaptureTimers[attemptingTeamId] = 0; // Reset the timer for the next breakpoint
				}
				foreach (var kvp in breakpointCaptureTimers)
				{
					if (kvp.Key != attemptingTeamId)
					{
						breakpointCaptureTimers[kvp.Key] = 0;
					}
				}
			}
			else
			{
				foreach (var kvp in breakpointCaptureTimers)
				{
					breakpointCaptureTimers[kvp.Key] = 0;
				}
			}

			// Handle point awarding
			if (controllingTeamId != 0)
			{
				pointAwardTimer += deltaTime;
				if (pointAwardTimer >= pointAwardInterval)
				{
					awardPointsCallback?.Invoke(controllingTeamId); // Award points to controlling team
					pointAwardTimer = 0; // Reset point award timer after awarding
				}
			}
			else
			{
				pointAwardTimer = 0.0f;
			}
		}

		private void CheckAndUpdateBreakpoint(int teamId)
		{
			if (teamId == 0 && controllingTeamId != 0)
			{
				teamId = controllingTeamId;
			}
			bool breakpointCaptured = false;
			// Handle increasing capture				
			for (int i = 0; i < breakpointControl.Breakpoints.Length; i++)
			{
				if (breakpointControl.Breakpoints[i] != teamId && breakpointControl.Breakpoints[i] != 0)
				{
					// Found a breakpoint not set to the current team, set it
					breakpointControl.SetBreakpoint(i, teamId);
					OnCaptureProgress?.Invoke(PointIndex, teamId, controllingTeamId, i);
					breakpointCaptured = true;
					break; // Exit the method after setting a breakpoint
				}
			}

			if (teamId != 0 && !breakpointCaptured)
			{
				for (int i = 0; i < breakpointControl.Breakpoints.Length; i++)
				{
					if (breakpointControl.Breakpoints[i] != teamId && breakpointControl.Breakpoints[i] == 0)
					{
						// Found a breakpoint not set to the current team, set it
						breakpointControl.SetBreakpoint(i, teamId);
						OnCaptureProgress?.Invoke(PointIndex, teamId, controllingTeamId, i);
						breakpointCaptured = true;
						break;
					}
				}
			}
					
			// Check if the point is fully captured
			if (breakpointCaptured && breakpointControl.IsFullyCapturedByTeam(teamId) && teamId != 0)
			{
				ProcessCapture(teamId);
			}
		}

		private void ProcessCapture(int teamId)
		{
			var oldControllingTeam = controllingTeamId;
			controllingTeamId = teamId;

			// Trigger capture event only if control changes
			if (oldControllingTeam != controllingTeamId)
			{
				OnPointCaptured?.Invoke(PointIndex, oldControllingTeam, controllingTeamId);
				pointAwardTimer = 0; // Reset point award timer
			}

			breakpointCaptureTimer = 0; // Reset the capture timer
		}

		public int GetControllingTeamId()
		{
			return controllingTeamId;
		}
	}
}
