using GAIPS.Rage;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;

public class NewGame : MonoBehaviour {
	private GameManager _gameManager;

	void Start () {
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	/// <summary>
	/// Check if a game with this name already exists
	/// </summary>
	public bool ExistingGameCheck(string boatName)
	{
		return _gameManager.CheckIfGameExists(Application.persistentDataPath, boatName);
	}

	/// <summary>
	/// Create a new game
	/// </summary>
	public bool CreateNewGame(string boatName, int[] colorsPri, int[] colorsSec, string managerName, string managerAge, string managerGender)
	{
		_gameManager.NewGame(LocalStorageProvider.Instance, Application.persistentDataPath, boatName, colorsPri, colorsSec, managerName, managerAge, managerGender);
		if (_gameManager.Boat != null && _gameManager.Boat.Name == boatName)
		{
			return true;
		}
		return false;
	}
}
