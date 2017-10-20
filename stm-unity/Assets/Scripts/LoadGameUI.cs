using UnityEngine;
using UnityEngine.UI;

using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;

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

	/// <summary>
	/// Get available games and wipe error text
	/// </summary>
	private void OnEnable()
	{
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
		if (string.IsNullOrEmpty(GameManagement.LoadGame.GetSelected()) && _loadButton.interactable)
		{
			_loadButton.interactable = false;
			_selectedIcon.SetActive(false);
		}
		else if(!string.IsNullOrEmpty(GameManagement.LoadGame.GetSelected()) && !_loadButton.interactable)
		{
			_loadButton.interactable = true;
			_selectedIcon.SetActive(true);
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
            UIStateManager.StaticBackToMenu();
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
		var gameNames = GameManagement.LoadGame.GetGames();
		foreach (var game in gameNames)
		{
			var gameButton = Instantiate(_gameButtonPrefab);
			gameButton.transform.SetParent(_gameContainer.transform, false);
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
		GameManagement.LoadGame.SetSelected(nameText.text);
		_selectedIcon.transform.SetParent(nameText.transform, false);
		_selectedIcon.transform.position = nameText.transform.position;
	}

	/// <summary>
	/// Triggered by button click. Load currently selected game.
	/// </summary>
	public void LoadGame()
	{
		_errorText.text = string.Empty;
		//check if the game exists
		var exists = GameManagement.LoadGame.ExistingGameCheck();
		if (exists)
		{
			var success = GameManagement.LoadGame.LoadSelectedGame();
			if (success)
			{
                UIStateManager.StaticGoToGame();
			}
			else
			{
				_errorText.text = Localization.Get("LOAD_GAME_NOT_LOADED");
			}
		}
		//display error and remove game from the list if the game could not be found
		else
		{
			_errorText.text = Localization.Get("LOAD_GAME_MISSING_FILES");
			_selectedIcon.transform.SetParent(_gameContainer.transform, true);
			_selectedIcon.SetActive(false);
			Destroy(_gameContainer.transform.Find(GameManagement.LoadGame.GetSelected()).gameObject);
			GameManagement.LoadGame.SetSelected(string.Empty);
		}
		
	}

	private void DoBestFit()
	{
	    _gameContainer.BestFit();
	}
}