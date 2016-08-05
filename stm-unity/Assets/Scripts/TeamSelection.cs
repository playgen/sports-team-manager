using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

public class TeamSelection : MonoBehaviour {
	private GameManager _gameManager;
	private int _confirmCount;

	void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
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

	/// <summary>
	/// Confirm the line-up and get its score
	/// </summary>
	public int ConfirmLineUp(bool historical = false)
	{
		_confirmCount++;
		if (historical)
		{
			if (_confirmCount >= 3)
			{
				_confirmCount -= 3;
			}
		}
		else
		{
			if (_confirmCount >= 3)
			{
				_gameManager.ConfirmLineUp();
				_confirmCount -= 3;
			}
			_gameManager.SaveLineUp();
		}
		return _gameManager.Boat.BoatScore;
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
}
