using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;

public class TeamSelection : MonoBehaviour {
	private GameManager _gameManager;
	private int _confirmCount;

	void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
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

	public int ConfirmLineUp()
	{
		_confirmCount++;
		if (_confirmCount >= 5)
		{

			_confirmCount = 0;
		}
		return _gameManager.Boat.BoatScore;
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
