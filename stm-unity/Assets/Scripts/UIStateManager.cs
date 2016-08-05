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
	private GameObject _topDetails;
	[SerializeField]
	private GameObject _sideMenu;
	[SerializeField]
	private GameObject _teamManagement;
	[SerializeField]
	private GameObject _seasonStandings;
	[SerializeField]
	private GameObject _helpPages;

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
		_topDetails.SetActive(false);
		_sideMenu.SetActive(false);
		_mainMenu.SetActive(true);
		_teamManagement.SetActive(false);
		_seasonStandings.SetActive(false);
		_helpPages.SetActive(false);
	}

	public void GoToGame(GameObject go)
	{
		go.SetActive(false);
		_topDetails.SetActive(true);
		_sideMenu.SetActive(true);
		GoToTeamManagement();
	}

	public void GoToTeamManagement()
	{
		_teamManagement.SetActive(true);
		_seasonStandings.SetActive(false);
		_helpPages.SetActive(false);
		(FindObjectOfType(typeof(ScreenSideUI)) as ScreenSideUI).ChangeSelected(0);
	}

	public void GoToSeasonStandings()
	{
		_teamManagement.SetActive(false);
		_seasonStandings.SetActive(true);
		_helpPages.SetActive(false);
		(FindObjectOfType(typeof(ScreenSideUI)) as ScreenSideUI).ChangeSelected(1);
	}

	public void GoToHelpPages()
	{
		_teamManagement.SetActive(false);
		_seasonStandings.SetActive(false);
		_helpPages.SetActive(true);
		(FindObjectOfType(typeof(ScreenSideUI)) as ScreenSideUI).ChangeSelected(2);
	}
}
