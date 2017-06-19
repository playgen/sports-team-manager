using UnityEngine;

using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Connecting class between GameManager in logic and the Learning Pill UI
/// </summary>
public class LearningPill : MonoBehaviour {

	private GameManager _gameManager;

	/// <summary>
	/// Return the learning pill text for the provided key
	/// </summary>
	public string GetHelpText(string key)
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.EventController.GetHelpText(key);
	}
}
