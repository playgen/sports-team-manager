using System.Collections.Generic;
using System.IO;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using PlayGen.Unity.Utilities.Localization;
using RAGE.Analytics.Formats;

/// <summary>
/// Contains all logic to communicate between NewGameUI and GameManager
/// </summary>
public class NewGame : MonoBehaviour {
	private GameManager _gameManager;

	private void Awake () {
		_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
	}

	/// <summary>
	/// Check if a game with this name already exists
	/// </summary>
	public bool ExistingGameCheck(string boatName)
	{
		return _gameManager.CheckIfGameExists(Path.Combine(Application.persistentDataPath, "GameSaves"), boatName);
	}

	/// <summary>
	/// Create a new game
	/// </summary>
	public bool CreateNewGame(string boatName, byte[] colorsPri, byte[] colorsSec, string managerName, bool showTutorial)
	{
		_gameManager.NewGame(Path.Combine(Application.persistentDataPath, "GameSaves"), boatName.TrimEnd(), colorsPri, colorsSec, managerName.TrimEnd(), showTutorial, string.IsNullOrEmpty(Localization.SelectedLanguage.Parent.Name) ? Localization.SelectedLanguage.EnglishName : Localization.SelectedLanguage.Parent.EnglishName);
		if (_gameManager.Team != null && _gameManager.Team.Name == boatName.TrimEnd())
		{
			var newString = string.Join(",", _gameManager.Team.Boat.Positions.Select(pos => pos.ToString()).ToArray());
			TrackerEventSender.SendEvent(new TraceEvent("GameStarted", TrackerVerbs.Initialized, new Dictionary<string, string>
			{
				{ TrackerContextKeys.GameName.ToString(), _gameManager.Team.Name },
				{ TrackerContextKeys.BoatLayout.ToString(), newString },
			}, CompletableTracker.Completable.Game));
		}
		return _gameManager.Team != null && _gameManager.Team.Name == boatName.TrimEnd();
	}

	public bool ExistingSaves()
	{
		return _gameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves")).Count != 0;
	}
}
