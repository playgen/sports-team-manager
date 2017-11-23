using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.Unity.Utilities.Localization;

/// <summary>
/// Container for the GameManager in the Simulation
/// </summary>
public static class GameManagement
{
	private static readonly GameManager _gameManager = new GameManager(Application.platform == RuntimePlatform.Android);
	private static readonly PlatformSettings _platformSettings = (PlatformSettings)Resources.Load("PlatformSettings", typeof(PlatformSettings));

	public static GameManager GameManager
	{
		get { return _gameManager; }
	}
	public static PlatformSettings PlatformSettings
	{
		get { return _platformSettings; }
	}

	public static List<string> GameNames
	{
		get { return _gameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves")); }
	}
	public static Team Team
	{
		get { return _gameManager.Team; }
	}
	public static string TeamName
	{
		get { return Team.Name; }
	}
	public static Dictionary<string, CrewMember> CrewMembers
	{
		get { return Team.CrewMembers; }
	}
	public static int CrewCount
	{
		get { return CrewMembers.Count; }
	}
	public static List<Boat> LineUpHistory
	{
		get { return Team.LineUpHistory; }
	}
	public static Person Manager
	{
		get { return Team.Manager; }
	}
	public static Boat Boat
	{
		get { return Team.Boat; }
	}
	public static Dictionary<Position, CrewMember> PositionCrew
	{
		get { return Boat.PositionCrew; }
	}
	public static List<Position> Positions
	{
		get { return Boat.Positions; }
	}
	public static string PositionString
	{
		get { return string.Join(",", Positions.Select(pos => pos.ToString()).ToArray()); }
	}
	public static int PositionCount
	{
		get { return Positions.Count; }
	}
	public static bool SeasonOngoing
	{
		get { return PositionCount > 0; }
	}
	public static List<PostRaceEventState> CurrentEvent
	{
		get { return _gameManager.EventController.PostRaceEvents.FirstOrDefault(); }
	}
	public static bool ShowTutorial
	{
		get { return _gameManager.ShowTutorial; }
	}
	public static int TutorialStage
	{
		get { return _gameManager.TutorialStage; }
	}
	public static int CurrentRaceSession
	{
		get { return _gameManager.CurrentRaceSession + 1; }
	}
	public static int RaceSessionLength
	{
		get { return _gameManager.RaceSessionLength; }
	}
	public static int SessionsRemaining
	{
		get { return RaceSessionLength - CurrentRaceSession; }
	}
	public static bool IsRace
	{
		get { return CurrentRaceSession == RaceSessionLength; }
	}
	public static string CurrentSessionString
	{
		get { return CurrentRaceSession + "/" + RaceSessionLength; }
	}
	public static int ActionAllowance
	{
		get { return _gameManager.ActionAllowance; }
	}
	public static bool ActionRemaining
	{
		get { return ActionAllowance > 0; }
	}
	public static float ActionAllowancePercentage
	{
		get { return ActionAllowance / (float)_gameManager.GetStartingActionAllowance(); }
	}
	public static bool CrewEditAllowed
	{
		get { return _gameManager.CrewEditAllowance > 0; }
	}
	public static int StartingActionAllowance
	{
		get { return _gameManager.GetStartingActionAllowance(); }
	}
	public static int StartingCrewEditAllowance
	{
		get { return _gameManager.GetStartingCrewEditAllowance(); }
	}

	public static float Value(this ConfigKeys key, CrewMember member = null)
	{
		return _gameManager.GetConfigValue(key, member);
	}

	public static bool Affordable(this ConfigKeys key, CrewMember member = null)
	{
		return ActionAllowance >= key.Value(member);
	}

	public static string EventString(this string key, bool localize = true)
	{
		var eventString = _gameManager.EventController.GetEventStrings(key).OrderBy(s => Guid.NewGuid()).First();
		return localize ? Localization.Get(eventString) : eventString;
	}

	public static Position BoatPosition(this CrewMember member)
	{
		return member.GetBoatPosition(PositionCrew);
	}

	/// <summary>
	/// Get the position finished in a race with the provided score and position count
	/// </summary>
	public static int GetRacePosition(int score, int positionCount)
	{
		var finishPosition = 1;
		var expected = GetExpectedScore(positionCount);
		while (score < expected && finishPosition < 10)
		{
			finishPosition++;
			expected -= positionCount;
		}
		return finishPosition;
	}

	/// <summary>
	/// Get the score expected to be able to finish first in a position
	/// </summary>
	public static float GetExpectedScore(int positionCount)
	{
		return (8f * positionCount) + 1;
	}

	/// <summary>
	/// Get the position the team finished after taking in their results over all the races
	/// </summary>
	public static int GetCupPosition()
	{
		var totalScore = 0;
		var raceResults = Team.RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();
		var racePositions = new List<int>();
		var finalPosition = 1;
		var finalPositionLocked = false;
		foreach (var result in raceResults)
		{
			var position = GetRacePosition(result.Key, result.Value);
			totalScore += position;
			racePositions.Add(position);
		}

		while (!finalPositionLocked && finalPosition < 10)
		{
			var otherTeamTotal = 0;
			foreach (var r in racePositions)
			{
				otherTeamTotal += (finalPosition < r ? finalPosition : finalPosition + 1);
			}
			if (otherTeamTotal < totalScore)
			{
				finalPosition++;
			}
			else
			{
				finalPositionLocked = true;
			}
		}
		return finalPosition;
	}
}