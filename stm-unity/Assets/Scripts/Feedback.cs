using System.Collections.Generic;

using PlayGen.RAGE.SportsTeamManager.Simulation;

using UnityEngine;

/// <summary>
/// Connecting class between GameManager in logic and the Feedback UI
/// </summary>
public class Feedback : MonoBehaviour {

	private GameManager _gameManager;

	/// <summary>
	/// Get a dictionary of management styles and the percentage of their use
	/// </summary>
	public Dictionary<string, float> GatherManagementStyles()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.GatherManagementStyles();
	}

	/// <summary>
	/// Get a dictionary of leaderboard styles and the percentage of their use
	/// </summary>
	public Dictionary<string, float> GatherLeadershipStyles()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.GatherLeadershipStyles();
	}

	/// <summary>
	/// Get an array of the most used leaderboard styles
	/// </summary>
	public string[] GetPrevalentLeadershipStyle()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.GetPrevalentLeadershipStyle();
	}
}
