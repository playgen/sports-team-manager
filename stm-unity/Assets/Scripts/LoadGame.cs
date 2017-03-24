using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

/// <summary>
/// Contains all logic to communicate between LoadGameUI and GameManager
/// </summary>
public class LoadGame : MonoBehaviour
{
	private GameManager _gameManager;
	private string _selectedName;

	private void Start()
	{
		_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
	}

	/// <summary>
	/// Get a list of the current games available to load
	/// </summary>
	public List<string> GetGames()
	{
		_selectedName = null;
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves"));
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
		return _selectedName != null && _gameManager.CheckIfGameExists(Path.Combine(Application.persistentDataPath, "GameSaves"), _selectedName);
	}

	/// <summary>
	/// Load the selected game
	/// </summary>
	public bool LoadSelectedGame()
	{
		if (_selectedName != null)
		{
			_gameManager.LoadGame(Path.Combine(Application.persistentDataPath, "GameSaves"), _selectedName);
			if (_gameManager.Team != null && _gameManager.Team.Name.ToLower() == _selectedName.ToLower())
			{
				var newString = string.Join(",", _gameManager.Team.Boat.Positions.Select(pos => pos.ToString()).ToArray());
				TrackerEventSender.SendEvent(new TraceEvent("GameStarted", new Dictionary<string, string>
				{
					{ TrackerContextKeys.GameName.ToString(), _gameManager.Team.Name },
					{ TrackerContextKeys.BoatLayout.ToString(), newString },
				}));
				return true;
			}
		}
		return false;
	}
}