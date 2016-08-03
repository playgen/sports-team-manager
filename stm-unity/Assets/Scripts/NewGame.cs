using GAIPS.Rage;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;

public class NewGame : MonoBehaviour {
	private GameManager _gameManager;

	void Start () {
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	public bool ExistingGameCheck(string boatName)
	{
		return _gameManager.CheckIfGameExists(Application.streamingAssetsPath, boatName);
	}
	
	public bool CreateNewGame(string boatName, string managerName, string managerAge, string managerGender)
	{
		_gameManager.NewGame(LocalStorageProvider.Instance, Application.streamingAssetsPath, boatName, managerName, managerAge, managerGender);
		if (_gameManager.Boat != null && _gameManager.Boat.Name == boatName)
		{
			return true;
		}
		return false;
	}
}
