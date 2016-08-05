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

	public List<Boat> GetLineUpHistory()
	{
		return _gameManager.LineUpHistory;
	}

	public Boat LoadCrew()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		_gameManager.RemoveAllCrew();
		return _gameManager.Boat;
	}

	public void AssignCrew(string crewMember, string position)
	{
		_gameManager.AssignCrew(position, crewMember);
	}

	public void RemoveCrew(string crewMember)
	{
		_gameManager.AssignCrew(null, crewMember);
	}

	public int GetStage()
	{
		return _confirmCount + 1;
	}

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

	public string GetCrewMemberPosition(CrewMember crewMember)
	{
		return crewMember.GetPosition(_gameManager.Boat);
	}

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
