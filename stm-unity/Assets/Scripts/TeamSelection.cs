﻿using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains all logic to communicate between TeamSelectionUI and GameManager
/// </summary>
public class TeamSelection : MonoBehaviour {
	private GameManager _gameManager;
	[SerializeField]
	private PostRaceEvent _postRaceEvent;
	private int _sessionLength;
	private int _confirmCount;

	private void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		_sessionLength = _gameManager.RaceSessionLength;
		_confirmCount = _gameManager.Team.LineUpHistory.Count;
		_postRaceEvent.GetEvent();
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public List<KeyValuePair<Boat, int>> GetLineUpHistory(int skipAmount, int takeAmount)
	{
		var boats = _gameManager.Team.LineUpHistory.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var offsets = _gameManager.Team.HistoricTimeOffset.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var boatOffsets = new List<KeyValuePair<Boat, int>>();
		for (var i = 0; i < boats.Count; i++)
		{
			if (i < offsets.Count)
			{
				boatOffsets.Add(new KeyValuePair<Boat, int>(boats[i], offsets[i]));
			}
		}
		return boatOffsets;
	}

	/// <summary>
	/// Get the currently available crew
	/// </summary>
	public List<CrewMember> LoadCrew()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		return _gameManager.Team.CrewMembers.Values.ToList();
	}

	/// <summary>
	/// Get the current team
	/// </summary>
	public Team GetTeam()
	{
		return _gameManager.Team;
	}

	/// <summary>
	/// Assign a CrewMember to a Position
	/// </summary>
	public void AssignCrew(CrewMember crewMember, Position position)
	{
		_gameManager.Team.Boat.AssignCrewMember(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position
	/// </summary>
	public void RemoveCrew(CrewMember crewMember)
	{
		_gameManager.Team.Boat.AssignCrewMember(0, crewMember);
	}

	/// <summary>
	/// Get the current session in the race
	/// </summary>
	public int GetStage()
	{
		return _confirmCount + 1;
	}

	/// <summary>
	/// Get the amount of sessions in this race
	/// </summary>
	public int GetSessionLength()
	{
		return _sessionLength;
	}

	/// <summary>
	/// Confirm the line-up and get its score
	/// </summary>
	public Boat ConfirmLineUp(int offset = 0)
	{
		_confirmCount++;
		_gameManager.SaveLineUp(offset);
		_postRaceEvent.GetEvent();
		if (_confirmCount % _sessionLength == 0)
		{
			_gameManager.ConfirmLineUp();
		}
		return _gameManager.Team.LineUpHistory.Last();
	}

	/// <summary>
	/// Get the amount of ActionAllowance remaining
	/// </summary>
	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}

	/// <summary>
	/// Get the amount of starting ActionAllowance
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return _gameManager.GetStartingActionAllowance();
	}

	/// <summary>
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return _gameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Get the amount of hire/fire actions remaining for this race
	/// </summary>
	public int CrewEditAllowance()
	{
		return _gameManager.CrewEditAllowance;
	}

	/// <summary>
	/// Check if the player is able to hire another character onto the team
	/// </summary>
	public bool CanAddCheck()
	{
		return _gameManager.Team.CanAddToCrew();
	}

	/// <summary>
	/// Check the amount of players below the crew limit this boat currently is
	/// </summary>
	public int CanAddAmount()
	{
		return _gameManager.Team.CrewLimitLeft();
	}

	/// <summary>
	/// Get the top amount of current mistakes in crew assignment the player is making
	/// </summary>
	public List<string> GetAssignmentMistakes(int amount)
	{
		return _gameManager.Team.Boat.GetAssignmentMistakes(amount);
	}

	/// <summary>
	/// Get the average team mood
	/// </summary>
	public float GetTeamAverageMood()
	{
		return _gameManager.Team.AverageTeamMood();
	}

	/// <summary>
	/// Get the average team manager opinion
	/// </summary>
	public float GetTeamAverageManagerOpinion()
	{
		return _gameManager.Team.AverageTeamManagerOpinion();
	}

	/// <summary>
	/// Get the average team opinion
	/// </summary>
	public float GetTeamAverageOpinion()
	{
		return _gameManager.Team.AverageTeamOpinion();
	}

	/// <summary>
	/// Get the average boat mood
	/// </summary>
	public float GetBoatAverageMood()
	{
		return _gameManager.Team.Boat.AverageBoatMood();
	}

	/// <summary>
	/// Get the average boat manager opinion
	/// </summary>
	public float GetBoatAverageManagerOpinion()
	{
		return _gameManager.Team.Boat.AverageBoatManagerOpinion(GetTeam().Manager.Name);
	}

	/// <summary>
	/// Get the average boat opinion
	/// </summary>
	public float GetBoatAverageOpinion()
	{
		return _gameManager.Team.Boat.AverageBoatOpinion();
	}
}
