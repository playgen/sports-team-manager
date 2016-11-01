using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;

/// <summary>
/// Contains all logic to communicate between NewGameUI and GameManager
/// </summary>
public class NewGame : MonoBehaviour {
	private GameManager _gameManager;

	private void Awake () {
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
	public bool CreateNewGame(string boatName, byte[] colorsPri, byte[] colorsSec, string managerName, string managerAge, string managerGender, bool showTutorial)
	{
		_gameManager.NewGame(Application.persistentDataPath, boatName, colorsPri, colorsSec, managerName, managerAge, managerGender, showTutorial, Localization.SelectedLanguage.ToString());
		return _gameManager.Team != null && _gameManager.Team.Name == boatName;
	}

	public bool ExistingSaves()
	{
		return _gameManager.GetGameNames(Application.persistentDataPath).Count != 0;
	}
}
