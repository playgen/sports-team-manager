using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(LoadGame))]
public class LoadGameUI : MonoBehaviour
{
	private LoadGame _loadGame;
	private UIStateManager _stateManager;
	[SerializeField]
	private GameObject _gameButtonPrefab;
	[SerializeField]
	private GameObject _gameContainer;

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_loadGame = GetComponent<LoadGame>();
	}

	void Start()
	{
		GetGames();
	}

	void GetGames()
	{
		foreach (Transform child in _gameContainer.transform)
		{
			Destroy(child.gameObject);
		}
		var gameNames = _loadGame.GetGames();
		for (int i = 0; i < gameNames.Count; i++)
		{
			GameObject gameButton = Instantiate(_gameButtonPrefab);
			gameButton.transform.SetParent(_gameContainer.transform, false);
			gameButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -gameButton.GetComponent<RectTransform>().sizeDelta.y * (i + 0.5f));
			gameButton.GetComponentInChildren<Text>().text = gameNames[i];
			var gameName = gameNames[i];
			gameButton.GetComponent<Button>().onClick.AddListener(() => SelectGame(gameName));
			gameButton.name = _gameButtonPrefab.name;
		}
	}

	public void SelectGame(string name)
	{
		_loadGame.SetSelected(name);
	}

	public void LoadGame()
	{
		bool exists = _loadGame.ExistingGameCheck();
		if (exists)
		{
			bool success = _loadGame.LoadSelectedGame();
			if (success)
			{
				print("Game loaded");
			}
			else
			{
				print("Game not loaded");
			}
		}
		else
		{
			print("Game does not exist");
			GetGames();
		}
		
	}

	public void BackToMenu()
	{
		_stateManager.BackToMenu(gameObject);
	}
}