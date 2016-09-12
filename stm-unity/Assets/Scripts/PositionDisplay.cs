using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

public class PositionDisplay : MonoBehaviour
{
	private GameManager _gameManager;

	void Awake()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	public Boat GetBoat()
	{
		return _gameManager.Boat;
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public List<Boat> GetLineUpHistory()
	{
		return _gameManager.LineUpHistory;
	}

	public int GetSessionLength()
	{
		return _gameManager.GetRaceSessionLength();
	}
}