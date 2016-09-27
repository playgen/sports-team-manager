﻿using GAIPS.Rage;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all logic to communicate between LoadGameUI and GameManager
/// </summary>
public class LoadGame : MonoBehaviour
{
	private GameManager _gameManager;
	private string _selectedName;

	void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	/// <summary>
	/// Get a list of the current games available to load
	/// </summary>
	public List<string> GetGames()
	{
		_selectedName = null;
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		return _gameManager.GetGameNames(Application.persistentDataPath);
	}

	/// <summary>
	/// Set the name of the game selected to load
	/// </summary>
	public void SetSelected(string name)
	{
		_selectedName = name;
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
		if (_selectedName != null)
		{
			return _gameManager.CheckIfGameExists(Application.persistentDataPath, _selectedName);
		}
		return false;
	}

	/// <summary>
	/// Load the selected game
	/// </summary>
	public bool LoadSelectedGame()
	{
		if (_selectedName != null)
		{
			_gameManager.LoadGame(LocalStorageProvider.Instance, Application.persistentDataPath, _selectedName);
			if (_gameManager.Boat != null && _gameManager.Boat.Name == _selectedName)
			{
				return true;
			}
		}
		return false;
	}
}