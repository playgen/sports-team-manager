using UnityEngine;
using UnityEngine.SceneManagement;
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

	void Start()
	{
		BackToMenu(_mainMenu);
	}

	/// <summary>
	/// Hide Main Menu, display New Game screen
	/// </summary>
	public void MenuToNewGame()
	{
		_mainMenu.SetActive(false);
		_newGame.SetActive(true);
	}

	/// <summary>
	/// Hide Main Menu, display Load Game screen
	/// </summary>
	public void MenuToLoadGame()
	{
		_mainMenu.SetActive(false);
		_loadGame.SetActive(true);
	}

	/// <summary>
	/// Hide all other screens, display Main Menu
	/// </summary>
	public void BackToMenu(GameObject go)
	{
		go.SetActive(false);
		_mainMenu.SetActive(true);
	}

	public void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Display top section and team selection, hide New Game/Load Game screen
	/// </summary>
	public void GoToGame(GameObject go)
	{
		go.SetActive(false);
		_topDetails.SetActive(true);
		_sideMenu.SetActive(true);
		GoToTeamManagement();
	}

	/// <summary>
	/// Hide other screens, display team management screen
	/// </summary>
	public void GoToTeamManagement()
	{
		_teamManagement.SetActive(true);
		_seasonStandings.SetActive(false);
		_helpPages.SetActive(false);
		(FindObjectOfType(typeof(ScreenSideUI)) as ScreenSideUI).ChangeSelected(0);
	}

	/// <summary>
	/// Hide other screens, display season standings screen
	/// </summary>
	public void GoToSeasonStandings()
	{
		_teamManagement.SetActive(false);
		_seasonStandings.SetActive(true);
		_helpPages.SetActive(false);
		(FindObjectOfType(typeof(ScreenSideUI)) as ScreenSideUI).ChangeSelected(1);
	}

	/// <summary>
	/// Hide other screens, display help screen
	/// </summary>
	public void GoToHelpPages()
	{
		_teamManagement.SetActive(false);
		_seasonStandings.SetActive(false);
		_helpPages.SetActive(true);
		(FindObjectOfType(typeof(ScreenSideUI)) as ScreenSideUI).ChangeSelected(2);
	}
}
