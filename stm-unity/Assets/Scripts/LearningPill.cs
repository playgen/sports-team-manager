using UnityEngine;
using System.Collections;

using PlayGen.RAGE.SportsTeamManager.Simulation;

public class LearningPill : MonoBehaviour {

	private GameManager _gameManager;

	public string GetHelpText(string key)
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		return _gameManager.EventController.GetHelpText(key);
	}
}
