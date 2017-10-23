using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using PlayGen.Unity.Utilities.Localization;
using RAGE.Analytics.Formats;

/// <summary>
/// Contains all logic to communicate between NewGameUI and GameManager
/// </summary>
public class NewGame {
	/// <summary>
	/// Check if a game with this name already exists
	/// </summary>
	public bool ExistingGameCheck(string boatName)
	{
		return GameManagement.GameManager.CheckIfGameExists(Path.Combine(Application.persistentDataPath, "GameSaves"), boatName);
	}

	/// <summary>
	/// Create a new game
	/// </summary>
	public bool CreateNewGame(string boatName, byte[] colorsPri, byte[] colorsSec, string managerName, bool showTutorial)
	{
		GameManagement.GameManager.NewGame(Path.Combine(Application.persistentDataPath, "GameSaves"), boatName.TrimEnd(), colorsPri, colorsSec, managerName.TrimEnd(), showTutorial, string.IsNullOrEmpty(Localization.SelectedLanguage.Parent.Name) ? Localization.SelectedLanguage.EnglishName : Localization.SelectedLanguage.Parent.EnglishName);
		if (GameManagement.Team != null && GameManagement.TeamName == boatName.TrimEnd())
		{
			var newString = string.Join(",", GameManagement.Positions.Select(pos => pos.ToString()).ToArray());
			TrackerEventSender.SendEvent(new TraceEvent("GameStarted", TrackerVerbs.Initialized, new Dictionary<string, string>
			{
				{ TrackerContextKeys.GameName.ToString(), GameManagement.TeamName },
				{ TrackerContextKeys.BoatLayout.ToString(), newString },
			}, CompletableTracker.Completable.Game));
		}
		return GameManagement.Team != null && GameManagement.TeamName == boatName.TrimEnd();
	}

	/// <summary>
	/// CHeck if the provided game name already exists
	/// </summary>
	public bool ExistingSaves()
	{
		return GameManagement.GameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves")).Count != 0;
	}
}
