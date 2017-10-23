using System.Collections.Generic;
using System.IO;
using System.Linq;

using RAGE.Analytics.Formats;

using UnityEngine;

/// <summary>
/// Contains all logic to communicate between LoadGameUI and GameManager
/// </summary>
public class LoadGame
{
	private string _selectedName;

	/// <summary>
	/// Get a list of the current games available to load
	/// </summary>
	public List<string> GetGames()
	{
		_selectedName = null;
		return GameManagement.GameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves"));
	}

	/// <summary>
	/// Set the name of the game selected to load
	/// </summary>
	public void SetSelected(string givenName)
	{
		_selectedName = givenName;
	}

	/// <summary>
	/// Return the name of the selected game
	/// </summary>
	public string GetSelected()
	{
		return _selectedName;
	}

	/// <summary>
	/// Check if the selected game exists
	/// </summary>
	public bool ExistingGameCheck()
	{
		return _selectedName != null && GameManagement.GameManager.CheckIfGameExists(Path.Combine(Application.persistentDataPath, "GameSaves"), _selectedName);
	}

	/// <summary>
	/// Load the selected game
	/// </summary>
	public bool LoadSelectedGame()
	{
		if (_selectedName != null)
		{
			GameManagement.GameManager.LoadGame(Path.Combine(Application.persistentDataPath, "GameSaves"), _selectedName);
			if (GameManagement.Team != null && GameManagement.TeamName.ToLower() == _selectedName.ToLower())
			{
				var newString = string.Join(",", GameManagement.Positions.Select(pos => pos.ToString()).ToArray());
				TrackerEventSender.SendEvent(new TraceEvent("GameStarted", TrackerVerbs.Initialized, new Dictionary<string, string>
				{
					{ TrackerContextKeys.GameName.ToString(), GameManagement.TeamName },
					{ TrackerContextKeys.BoatLayout.ToString(), string.IsNullOrEmpty(newString) ? "NullAsGameFinished" : newString },
				}, CompletableTracker.Completable.Game));
				return true;
			}
		}
		return false;
	}
}