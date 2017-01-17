using UnityEngine;

using PlayGen.RAGE.SportsTeamManager.Simulation;

public class LearningPill : MonoBehaviour {

	private GameManager _gameManager;

	public string GetHelpText(string key)
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.EventController.GetHelpText(key);
	}
}
