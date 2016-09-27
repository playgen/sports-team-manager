﻿using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LoadGame))]
/// <summary>
/// Contains all UI logic related to loading saved games
/// </summary>
public class LoadGameUI : MonoBehaviour
{
	private LoadGame _loadGame;
	private UIStateManager _stateManager;
	[SerializeField]
	private Button _loadButton;
	[SerializeField]
	private GameObject _selectedIcon;
	[SerializeField]
	private GameObject _gameButtonPrefab;
	[SerializeField]
	private GameObject _gameContainer;
	[SerializeField]
	private Text _errorText;

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_loadGame = GetComponent<LoadGame>();
	}

    /// <summary>
    /// Get available games and wipe error text
    /// </summary>
    void OnEnable()
	{
		GetGames();
		_errorText.text = "";
	}

	/// <summary>
	/// Update the position of the selected icon and if the load button should be enabled according to whether a game is selected or not
	/// </summary>
	void Update()
	{
		if (string.IsNullOrEmpty(_loadGame.GetSelected()) && _loadButton.interactable)
		{
			_loadButton.interactable = false;
			_selectedIcon.SetActive(false);
		}
		else if(!string.IsNullOrEmpty(_loadGame.GetSelected()) && !_loadButton.interactable)
		{
			_loadButton.interactable = true;
			_selectedIcon.SetActive(true);
		}
	}

	/// <summary>
	/// Get a list of the current game and instantiate a new button for each one
	/// </summary>
	void GetGames()
	{
		_selectedIcon.transform.SetParent(transform, false);
        //destroy old buttons
		foreach (Transform child in _gameContainer.transform)
		{
			Destroy(child.gameObject);
		}
		var gameNames = _loadGame.GetGames();
		for (int i = 0; i < gameNames.Count; i++)
		{
			GameObject gameButton = Instantiate(_gameButtonPrefab);
			gameButton.transform.SetParent(_gameContainer.transform, false);
			gameButton.GetComponentInChildren<Text>().text = gameNames[i];
			gameButton.GetComponent<Button>().onClick.AddListener(() => SelectGame(gameButton.GetComponentInChildren<Text>()));
			gameButton.name = gameNames[i];
		}
	}

	/// <summary>
	/// Triggered by button click. Set clicked to be selected game
	/// </summary>
	public void SelectGame(Text name)
	{
		_errorText.text = "";
		_loadGame.SetSelected(name.text);
		_selectedIcon.transform.SetParent(name.transform, false);
		_selectedIcon.transform.position = name.transform.position;
		Tracker.T.alternative.Selected("Load Game", "Selected Game", AlternativeTracker.Alternative.Menu);
	}

	/// <summary>
	/// Triggered by button click. Load currently selected game.
	/// </summary>
	public void LoadGame()
	{
		Tracker.T.alternative.Selected("Load Game", "Loaded Game", AlternativeTracker.Alternative.Menu);
		_errorText.text = "";
        //check if the game exists
		bool exists = _loadGame.ExistingGameCheck();
		if (exists)
		{
			bool success = _loadGame.LoadSelectedGame();
			if (success)
			{
				_stateManager.GoToGame(gameObject);
				Tracker.T.completable.Initialized("Loaded Game", CompletableTracker.Completable.Game);
			}
			else
			{
				_errorText.text = "Game was not loaded. Please try again.";
			}
		}
        //display error and remove game from the list if the game could not be found
		else
		{
			_errorText.text = "Game does not exist. Please try loading a different game.";
			_selectedIcon.transform.SetParent(_gameContainer.transform, true);
			_selectedIcon.SetActive(false);
			Destroy(_gameContainer.transform.Find(_loadGame.GetSelected()).gameObject);
			_loadGame.SetSelected("");
		}
		
	}

	/// <summary>
	/// Send message to UIStateManager to reset UI panels back to the Main Menu
	/// </summary>
	public void BackToMenu()
	{
		_stateManager.BackToMenu(gameObject);
	}
}