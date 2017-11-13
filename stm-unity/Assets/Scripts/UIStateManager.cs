using System.IO;

using PlayGen.SUGAR.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.BestFit;

/// <summary>
/// Controls switching between different game state panels
/// </summary>
public class UIStateManager : MonoBehaviour {
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
	private GameObject _teamManagement;
	[SerializeField]
	private GameObject _questionnaire;
	[SerializeField]
	private GameObject _feedback;
	private static bool _loaded;
	private static bool _reload;
	private static UIStateManager _instance;

	private void Awake()
	{
		_instance = this;
	}

	/// <summary>
	/// Load Music and Sound settings, trigger SUGAR sign-in on first load
	/// </summary>
	void Start()
	{
		UIManagement.Initialize();
		AvatarDisplay.LoadSprites();
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
		BackToMenu();
		if (SUGARManager.CurrentUser == null && !_loaded)
		{
			_loaded = true;
			SignIn();
		}
		else
		{
			_signIn.Active(SUGARManager.CurrentUser == null);
			if (SUGARManager.CurrentUser != null)
			{
				_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + " " + SUGARManager.CurrentUser.Name;
			}
		}
		if (_reload)
		{
			GameManagement.GameManager.LoadGame(Path.Combine(Application.persistentDataPath, "GameSaves"), GameManagement.Team.Name);
			GoToGame();
			_reload = false;
		}
	}

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Close game when escape is pressed on the main menu
	/// </summary>
	void Update()
	{
		if (_mainMenu.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				CloseGame();
			}
		}
#if UNITY_EDITOR
		//takes a screenshot whenever down arrow is pressed
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			Application.CaptureScreenshot(System.DateTime.UtcNow.ToFileTimeUtc() + ".png");
		}
#endif
	}

	/// <summary>
	/// Hide Main Menu, display New Game screen
	/// </summary>
	public void MenuToNewGame()
	{
		_mainMenu.Active(false);
		_newGame.Active(true);
	}

	/// <summary>
	/// Hide Main Menu, display Load Game screen
	/// </summary>
	public void MenuToLoadGame()
	{
		_mainMenu.Active(false);
		_loadGame.Active(true);
	}

	/// <summary>
	/// Hide all other screens, display Main Menu
	/// </summary>
	public void BackToMenu()
	{
		_newGame.Active(false);
		_loadGame.Active(false);
		_teamManagement.Active(false);
		_questionnaire.Active(false);
		_feedback.Active(false);
		_mainMenu.Active(true);
		DoBestFit();
		_mainMenu.transform.Find("Load Game").GetComponent<Button>().interactable = GameManagement.GameNames.Count != 0;
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
	public void GoToGame()
	{
		_newGame.Active(false);
		_loadGame.Active(false);
		_teamManagement.Active(true);
		_questionnaire.Active(false);
		_feedback.Active(false);
		((ScreenSideUI)FindObjectOfType(typeof(ScreenSideUI))).ChangeSelected(0);
	}

	/// <summary>
	/// Hide other UI states, show questionnaire
	/// </summary>
	public void GoToQuestionnaire()
	{
		_teamManagement.Active(false);
		_questionnaire.Active(true);
		_feedback.Active(false);
	}

	/// <summary>
	/// Hide other UI states, show feedback
	/// </summary>
	public void GoToFeedback()
	{
		_teamManagement.Active(false);
		_questionnaire.Active(false);
		_feedback.Active(true);
	}

	/// <summary>
	/// Trigger showing SUGAR achievements
	/// </summary>
	public void ShowAchievements()
	{
		SUGARManager.Achievement.DisplayList();
	}

	/// <summary>
	/// Trigger showing SUGAR leaderboards
	/// </summary>
	public void ShowLeaderboards()
	{
		SUGARManager.GameLeaderboard.DisplayList();
	}

	/// <summary>
	/// Trigger showing SUGAR sign-in screen or run auto sign-in if setting is active
	/// </summary>
	public void SignIn()
	{
		SUGARManager.Account.DisplayPanel(success =>
		{
			if (success)
			{
				_signIn.Active(false);
				_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + " " + SUGARManager.CurrentUser.Name;
				DoBestFit();
			}
			else
			{
				_signIn.Active(true);
			}
		});
	}

	/// <summary>
	/// Close the game
	/// </summary>
	public void CloseGame()
	{
		Application.Quit();
	}

	private void OnLanguageChange()
	{
		if (SUGARManager.CurrentUser != null)
		{
			_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + " " + SUGARManager.CurrentUser.Name;
		}
	}

	private void DoBestFit()
	{
		_mainMenu.GetComponentsInChildren<Text>().Where(t => t.transform.parent == _mainMenu.transform || t.transform.parent.parent == _mainMenu.transform).BestFit();
	}

	public static void StaticGoToQuestionnaire()
	{
		_instance.GoToQuestionnaire();
	}

	public static void StaticGoToFeedback()
	{
		_instance.GoToFeedback();
	}

	public static void StaticGoToGame()
	{
		_instance.GoToGame();
	}

	public static void StaticBackToMenu()
	{
		_instance.BackToMenu();
	}

	public static void ReloadGame()
	{
		_reload = true;
		_instance.ReloadScene();
	}
}