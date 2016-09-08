using System;

using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

using IntegratedAuthoringTool.DTOs;

public class TeamSelection : MonoBehaviour {
	private GameManager _gameManager;
	[SerializeField]
	private PostRaceEvent _postRaceEvent;
	private int _sessionLength;
	private int _confirmCount;

	void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		_sessionLength = _gameManager.GetRaceSessionLength();
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public List<Boat> GetLineUpHistory()
	{
		return _gameManager.LineUpHistory;
	}

	/// <summary>
	/// Get the currently available crew for the active Boat
	/// </summary>
	public Boat LoadCrew()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		_gameManager.RemoveAllCrew();
		return _gameManager.Boat;
	}

	public Boat GetBoat()
	{
		return _gameManager.Boat;
	}

	/// <summary>
	/// Assign a CrewMember to a Position on the active boat
	/// </summary>
	public void AssignCrew(CrewMember crewMember, Position position)
	{
		_gameManager.AssignCrew(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position on the active boat
	/// </summary>
	public void RemoveCrew(CrewMember crewMember)
	{
		_gameManager.AssignCrew(null, crewMember);
	}

	/// <summary>
	/// Get the current assigning stage
	/// </summary>
	public int GetStage()
	{
		return _confirmCount + 1;
	}

	public int GetSessionLength()
	{
		return _sessionLength;
	}

	/// <summary>
	/// Confirm the line-up and get its score
	/// </summary>
	public int ConfirmLineUp(bool historical = false)
	{
		int score = 0;
		_confirmCount++;
		if (historical)
		{
			if (_confirmCount >= _sessionLength)
			{
				_confirmCount -= _sessionLength;
			}
		}
		else
		{
			_gameManager.SaveLineUp();
			score = _gameManager.Boat.BoatScore;
			if (_confirmCount >= _sessionLength)
			{
				_gameManager.ConfirmLineUp();
				_confirmCount -= _sessionLength;
			}
		}
		return score;
	}

	public bool IsRace()
	{
		if (_confirmCount == 0)
		{
			return true;
		}
		return false;
	}

	public void PostRaceEvent()
	{
		if (_confirmCount == _sessionLength - 1)
		{
			_postRaceEvent.GetEvent();
		}
	}

	/// <summary>
	/// Get the current CrewMember (if any) of a position
	/// </summary>
	public string GetPositionCrewMember(Position position)
	{
		return position.GetCrewMember(_gameManager.Boat);
	}

	/// <summary>
	/// Get the current score of a Position on the active boat by name
	/// </summary>
	public int GetPositionScore(Position position)
	{
		var boatPosition = _gameManager.Boat.BoatPositions.SingleOrDefault(bp => bp.Position == position);
		if (boatPosition != null)
		{
			return boatPosition.PositionScore;
		}
		return 0;
	}

	public float IdealCheck()
	{
		return _gameManager.Boat.IdealMatchScore;
	}

	public CrewMember PersonToCrewMember(Person person)
	{
		return _gameManager.Boat.GetAllCrewMembers().Where(cm => cm.Name == person.Name).FirstOrDefault();
	}

	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}

	public int CrewEditAllowance()
	{
		return _gameManager.CrewEditAllowance;
	}

	public bool CanAddCheck()
	{
		return _gameManager.CanAddToCrew();
	}
}
