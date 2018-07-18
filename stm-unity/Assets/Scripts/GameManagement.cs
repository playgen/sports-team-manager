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
	private static Platform _platform
	{
		get
		{
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsPlayer:
					return Platform.Windows;
				case RuntimePlatform.Android:
					return Platform.Android;
				case RuntimePlatform.IPhonePlayer:
					return Platform.iOS;
				default:
					return Platform.Windows;
			}
		}
	}

	public static GameManager GameManager { get; } = new GameManager(_platform);

	public static EventController EventController => GameManager.EventController;

	public static PlatformSettings PlatformSettings { get; } = (PlatformSettings)Resources.Load("PlatformSettings", typeof(PlatformSettings));

	public static bool RageMode => PlatformSettings.Rage;

	public static bool DemoMode => PlatformSettings.DemoMode;

	public static List<string> GameNames => GameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves"));

	public static int GameCount => GameNames.Count;

	public static Team Team => GameManager.Team;

	public static string TeamName => Team.Name;

	public static Dictionary<string, CrewMember> CrewMembers => Team.CrewMembers;

	public static int CrewCount => CrewMembers.Count;

	public static List<Boat> LineUpHistory => Team.LineUpHistory;

	public static List<Boat> ReverseLineUpHistory => LineUpHistory.AsEnumerable().Reverse().ToList();

	public static Boat PreviousSession => LineUpHistory.LastOrDefault();

	public static int SessionCount => LineUpHistory.Count;

	public static List<Boat> RaceHistory => Team.RaceHistory;

	public static int RaceCount => RaceHistory.Count;

	public static List<KeyValuePair<int, int>> RaceScorePositionCountPairs => RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();

	public static Person Manager => Team.Manager;

	public static Boat Boat => Team.Boat;

	public static Dictionary<Position, CrewMember> PositionCrew => Boat.PositionCrew;

	public static List<Position> Positions => Boat.Positions;

	public static string PositionString
	{
		get { return string.Join(",", Positions.Select(pos => pos.ToString()).ToArray()); }
	}
	public static int PositionCount => Positions.Count;

	public static bool SeasonOngoing => PositionCount > 0;

	public static List<PostRaceEventState> CurrentEvent => EventController.PostRaceEvents.FirstOrDefault();

	public static bool ShowTutorial => GameManager.ShowTutorial;

	public static int TutorialStage => GameManager.TutorialStage;

	public static int CurrentRaceSession => GameManager.CurrentRaceSession + 1;

	public static int RaceSessionLength => GameManager.RaceSessionLength;

	public static int SessionsRemaining => RaceSessionLength - CurrentRaceSession;

	public static bool IsRace => CurrentRaceSession == RaceSessionLength;

	public static string CurrentSessionString => CurrentRaceSession + "/" + RaceSessionLength;

	public static int ActionAllowance => GameManager.ActionAllowance;

	public static bool ActionRemaining => ActionAllowance > 0;

	public static float ActionAllowancePercentage => ActionAllowance / (float)GameManager.GetStartingActionAllowance();

	public static bool CrewEditAllowed => GameManager.CrewEditAllowance > 0;

	public static bool CanAddToCrew => Team.CanAddToCrew();

	public static bool CanRemoveFromCrew => Team.CanRemoveFromCrew();

	public static float AverageTeamMood => Team.AverageTeamMood();

	public static float AverageTeamManagerOpinion => Team.AverageTeamManagerOpinion();

	public static float AverageTeamOpinion => Team.AverageTeamOpinion();

	public static int StartingActionAllowance => GameManager.GetStartingActionAllowance();

	public static int StartingCrewEditAllowance => GameManager.GetStartingCrewEditAllowance();

	public static bool QuestionnaireCompleted => GameManager.QuestionnaireCompleted;

	public static float Value(this ConfigKey key, CrewMember member = null)
	{
		return GameManager.GetConfigValue(key, member);
	}

	public static bool Affordable(this ConfigKey key, CrewMember member = null)
	{
		return ActionAllowance >= key.Value(member);
	}

	public static string EventString(this string key, bool localize = true)
	{
		var eventString = EventController.GetEventStrings(key).OrderBy(s => Guid.NewGuid()).First();
		return localize ? Localization.Get(eventString) : eventString;
	}

	public static string EventKeys(this string state)
	{
		return EventController.GetEventKeys().First(state.Split('_')[1].StartsWith);
	}

	public static string HelpText(this string key)
	{
		return Localization.Get(EventController.GetHelpText(key));
	}

	public static Position BoatPosition(this CrewMember member)
	{
		return Boat.GetCrewMemberPosition(member);
	}

	public static int RacesWon(this CrewMember member)
	{
		return RaceHistory.Count(boat => boat.PositionCrew.Values.Any(cm => member.Name == cm.Name) && GetRacePosition(boat.Score, boat.Positions.Count) == 1);
	}

	public static int SessionsIncluded(this CrewMember member)
	{
		return LineUpHistory.Count(boat => boat.PositionCrew.Values.ToList().Any(c => c.Name == member.Name));
	}

	public static int SessionsIncluded(this Position position)
	{
		return LineUpHistory.Sum(boat => boat.Positions.Count(pos => pos == position)) + (Positions.Contains(position) ? 1 : 0);
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
		var raceResults = RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();
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