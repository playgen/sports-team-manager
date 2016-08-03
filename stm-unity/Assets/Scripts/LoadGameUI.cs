using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(LoadGame))]
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
	private Scrollbar _scrollbar;
	[SerializeField]
	private Text _errorText;

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_loadGame = GetComponent<LoadGame>();
	}

	void OnEnable()
	{
		GetGames();
		_errorText.text = "";
	}

	void Update()
	{
		if (_loadGame.GetSelected() == null && _loadButton.interactable)
		{
			_loadButton.interactable = false;
			_selectedIcon.SetActive(false);
		}
		else if(_loadGame.GetSelected() != null && !_loadButton.interactable)
		{
			_loadButton.interactable = true;
			_selectedIcon.SetActive(true);
		}
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
			gameButton.GetComponent<Button>().onClick.AddListener(() => SelectGame(gameButton.GetComponentInChildren<Text>()));
			gameButton.name = _gameButtonPrefab.name;
		}
		_gameContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(_gameContainer.GetComponent<RectTransform>().sizeDelta.x, gameNames.Count * _gameButtonPrefab.GetComponent<RectTransform>().sizeDelta.y);
		_gameContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -_gameContainer.GetComponent<RectTransform>().sizeDelta.y * 0.5f);
		var scrollSize = _gameContainer.GetComponent<RectTransform>().sizeDelta.y != 0 ? _gameContainer.GetComponent<RectTransform>().sizeDelta.y : 1;
		_scrollbar.size = Mathf.Abs(_gameContainer.transform.parent.GetComponent<RectTransform>().rect.height)/scrollSize;
	}

	public void Scroll()
	{
		var scrollAmount = (_gameContainer.GetComponent<RectTransform>().sizeDelta.y - Mathf.Abs(_gameContainer.transform.parent.GetComponent<RectTransform>().rect.height));
		_gameContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -_gameContainer.GetComponent<RectTransform>().sizeDelta.y * 0.5f + (scrollAmount * _scrollbar.value));
	}

	public void SelectGame(Text name)
	{
		_errorText.text = "";
		_loadGame.SetSelected(name.text);
		_selectedIcon.transform.position = name.transform.position;
	}

	public void LoadGame()
	{
		_errorText.text = "";
		bool exists = _loadGame.ExistingGameCheck();
		if (exists)
		{
			bool success = _loadGame.LoadSelectedGame();
			if (success)
			{
				_stateManager.GoToGame(gameObject);
			}
			else
			{
				_errorText.text = "Game was not loaded. Please try again.";
			}
		}
		else
		{
			_errorText.text = "Game does not exist. Please try loading a different game.";
			GetGames();
		}
		
	}

	public void BackToMenu()
	{
		_stateManager.BackToMenu(gameObject);
	}
}