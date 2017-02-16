using System.Collections;
using System.Collections.Generic;

using PlayGen.RAGE.SportsTeamManager.Simulation;

using UnityEngine;

public class Feedback : MonoBehaviour {

	private GameManager _gameManager;

	public Dictionary<string, int> GatherManagementStyles()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.GatherManagementStyles();
	}

	public string[] GetPrevalentLeadershipStyle()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.GetPrevalentLeadershipStyle();
	}
}
