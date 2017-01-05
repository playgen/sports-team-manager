using System.Reflection;

using PlayGen.SUGAR.Unity;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls switching between different game state panels
/// </summary>
public class UIStateManager : ObservableMonoBehaviour {
	public static bool MusicOn = true;
	public static bool SoundOn = true;
	[SerializeField]
	private GameObject _mainMenu;
	[SerializeField]
	private GameObject _newGame;
	[SerializeField]
	private GameObject _loadGame;
	[SerializeField]
	private GameObject _signIn;
	[SerializeField]
	private Text _userSignedInText;
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
	private static bool _loaded;

	void Start()
	{
		if (PlayerPrefs.HasKey("Music"))
		{
			MusicOn = PlayerPrefs.GetInt("Music") == 1;
		}
		else
		{
			PlayerPrefs.SetInt("Music", 1);
		}
		if (PlayerPrefs.HasKey("Sound"))
		{
			SoundOn = PlayerPrefs.GetInt("Sound") == 1;
		}
		else
		{
			PlayerPrefs.SetInt("Sound", 1);
		}
		BackToMenu(_mainMenu);
		if (SUGARManager.CurrentUser == null && !_loaded)
		{
			_loaded = true;
			SignIn();
		}
	}

	void Update()
	{
		if (_mainMenu.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				CloseGame();
			}
		}
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
		var gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		_mainMenu.transform.Find("Load Game").GetComponent<Button>().interactable = gameManager.GetGameNames(Application.persistentDataPath).Count != 0;
	}

	/// <summary>
	/// Reload the scene
	/// </summary>
	public void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Display top section and team selection, hide New Game/Load Game screen
	/// </summary>
	public void GoToGame(GameObject go)
	{
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(CompletableTracker).Name, "Initialized", "StartSession", CompletableTracker.Completable.Session));
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

	public void ShowAchievements()
	{
		SUGARManager.Achievement.DisplayList();
	}

	public void ShowLeaderboards()
	{
		SUGARManager.GameLeaderboard.DisplayList();
	}

	public void SignIn()
	{
		SUGARManager.Account.DisplayPanel(success =>
		{
			if (success)
			{
				_signIn.SetActive(false);
				_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + SUGARManager.CurrentUser.Name;
			}
			else
			{
				_signIn.SetActive(true);
			}
			//SUGARManager.Account.
		});
	}

	/// <summary>
	/// Close the game
	/// </summary>
	public void CloseGame()
	{
		Application.Quit();
	}
}
