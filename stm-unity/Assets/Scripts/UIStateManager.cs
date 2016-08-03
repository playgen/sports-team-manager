using UnityEngine;
using UnityEngine.UI;

public class UIStateManager : MonoBehaviour {
	[SerializeField]
	private GameObject _mainMenu;
	[SerializeField]
	private GameObject _newGame;
	[SerializeField]
	private GameObject _loadGame;
	[SerializeField]
	private GameObject _teamSelection;

	public void MenuToNewGame()
	{
		_mainMenu.SetActive(false);
		_newGame.SetActive(true);
	}

	public void MenuToLoadGame()
	{
		_mainMenu.SetActive(false);
		_loadGame.SetActive(true);
	}

	public void BackToMenu(GameObject go)
	{
		go.SetActive(false);
		_mainMenu.SetActive(true);
	}

	public void GoToGame(GameObject go)
	{
		go.SetActive(false);
		_teamSelection.SetActive(true);
	}
}
