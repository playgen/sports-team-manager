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
	public void AssignCrew(string crewMember, string position)
	{
		_gameManager.AssignCrew(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position on the active boat
	/// </summary>
	public void RemoveCrew(string crewMember)
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
			if (_confirmCount >= _sessionLength)
			{
				_gameManager.ConfirmLineUp();
				_confirmCount -= _sessionLength;
			}
		}
		return _gameManager.Boat.BoatScore;
	}

	public void PostRaceEvent()
	{
		if (_confirmCount == 0)
		{
			_postRaceEvent.GetEvent();
		}
	}

	/// <summary>
	/// Get the current position (if any) of a CrewMember
	/// </summary>
	public string GetCrewMemberPosition(CrewMember crewMember)
	{
		return crewMember.GetPosition(_gameManager.Boat);
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
	public int GetPositionScore(string positionName)
	{
		var boatPosition = _gameManager.Boat.BoatPositions.SingleOrDefault(bp => bp.Position.Name == positionName);
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

	public void FireCrewMember(CrewMember crewMember)
	{
		_gameManager.RetireCrewMember(crewMember);
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

	public bool CanRemoveCheck()
	{
		return _gameManager.CanRemoveFromCrew();
	}
}
