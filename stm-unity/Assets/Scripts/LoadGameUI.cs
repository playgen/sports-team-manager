﻿using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.Unity.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Loading;
using PlayGen.Unity.Utilities.Localization;
using TrackerAssetPackage;

/// <summary>
/// Contains all UI logic related to loading saved games
/// </summary>
public class LoadGameUI : MonoBehaviour
{
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

	private string _selectedName;

	/// <summary>
	/// Get available games and wipe error text
	/// </summary>
	private void OnEnable()
	{
		_selectedName = string.Empty;
		_loadButton.interactable = false;
		_selectedIcon.Active(false);
		GetGames();
		_errorText.text = string.Empty;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Update the position of the selected icon and if the load button should be enabled according to whether a game is selected or not
	/// </summary>
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			GoToMainMenu();
		}
	}

	/// <summary>
	/// Get a list of the current games and instantiate a new button for each one
	/// </summary>
	private void GetGames()
	{
		_selectedIcon.transform.SetParent(transform, false);
		//destroy old buttons
		foreach (Transform child in _gameContainer.transform)
		{
			Destroy(child.gameObject);
		}
		foreach (var game in GameManagement.GameNames)
		{
			var gameButton = Instantiate(_gameButtonPrefab, _gameContainer.transform, false);
			gameButton.GetComponentInChildren<Text>().text = game;
			gameButton.GetComponent<Button>().onClick.AddListener(() => SelectGame(gameButton.GetComponentInChildren<Text>()));
			gameButton.name = game;
		}
		DoBestFit();
	}

	/// <summary>
	/// Triggered by button click. Set clicked to be selected game
	/// </summary>
	public void SelectGame(Text nameText)
	{
		_errorText.text = string.Empty;
		_selectedName = nameText.text;
		_loadButton.interactable = true;
		_selectedIcon.Active(true);
		_selectedIcon.transform.SetParent(nameText.transform.parent, false);
		_selectedIcon.RectTransform().anchoredPosition = Vector2.zero;
		_selectedIcon.RectTransform().sizeDelta = Vector2.zero;
	}

	/// <summary>
	/// Triggered by button click. Load currently selected game.
	/// </summary>
	public void LoadGame()
	{
		if (_selectedName != null)
		{
			_errorText.text = string.Empty;
			//check if the game exists
			Loading.Start();
			GameManagement.GameManager.LoadGameTask(GameManagement.GameSavePath, _selectedName, success =>
			{
				if (success)
				{
					if (GameManagement.Team != null && string.Equals(GameManagement.TeamName, _selectedName, StringComparison.CurrentCultureIgnoreCase))
					{
						var newString = GameManagement.PositionString;
						TrackerEventSender.SendEvent(new TraceEvent("GameStarted", TrackerAsset.Verb.Initialized, new Dictionary<TrackerContextKey, object>
						{
							{ TrackerContextKey.GameName, GameManagement.TeamName },
							{ TrackerContextKey.BoatLayout, string.IsNullOrEmpty(newString) ? "NullAsGameFinished" : newString }
						}, CompletableTracker.Completable.Game));
						TrackerEventSender.SendEvaluationEvent(TrackerEvalautionEvent.UserProfile, new Dictionary<TrackerEvaluationKey, string> { { TrackerEvaluationKey.Event, "loadedoldteam" } });
						UIManagement.StateManager.GoToState(State.TeamManagement);
					}
					else
					{
						//display error and remove game from the list if the game could not be found
						_errorText.text = Localization.Get("LOAD_GAME_MISSING_FILES");
						_selectedIcon.transform.SetParent(_gameContainer.transform, true);
						_selectedIcon.Active(false);
						Destroy(_gameContainer.transform.FindObject(_selectedName));
						_selectedName = string.Empty;
						_loadButton.interactable = false;
					}
				}
				Loading.Stop();
			});
		}
	}

	/// <summary>
	/// Triggered by button click. Return back to showing the Main Menu UI
	/// </summary>
	public void GoToMainMenu()
	{
		UIManagement.StateManager.GoToState(State.MainMenu);
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Button>().ToList().BestFit();
	}
}