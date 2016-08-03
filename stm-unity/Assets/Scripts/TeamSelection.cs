using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;

public class TeamSelection : MonoBehaviour {
	private GameManager _gameManager;

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
		print(_gameManager.Boat.BoatScore);
	}

	public void RemoveCrew(string crewMember)
	{
		_gameManager.AssignCrew(null, crewMember);
		print(_gameManager.Boat.BoatScore);
	}
}
