using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

/// <summary>
/// Contains all logic to communicate between PositionDisplayUI and GameManager
/// </summary>
public class PositionDisplay : MonoBehaviour
{
	private GameManager _gameManager;

	void Awake()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

    /// <summary>
    /// Get the current boat being used throughout the game
    /// </summary>
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
}