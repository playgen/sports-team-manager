using GAIPS.Rage;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using UnityEngine;

public class LoadGame : MonoBehaviour
{
	private GameManager _gameManager;
	private string _selectedName;

	void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	public List<string> GetGames()
	{
		_selectedName = null;
		return _gameManager.GetGameNames(Application.streamingAssetsPath);
	}

	public void SetSelected(string name)
	{
		_selectedName = name;
	}

	public bool ExistingGameCheck()
	{
		if (_selectedName != null)
		{
			return _gameManager.CheckIfGameExists(Application.streamingAssetsPath, _selectedName);
		}
		return false;
	}

	public bool LoadSelectedGame()
	{
		if (_selectedName != null)
		{
			_gameManager.LoadGame(LocalStorageProvider.Instance, Application.streamingAssetsPath, _selectedName);
			if (_gameManager.Boat != null && _gameManager.Boat.Name == _selectedName)
			{
				return true;
			}
		}
		return false;
	}
}