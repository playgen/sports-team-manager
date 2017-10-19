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
	private GameObject _teamManagement;
	[SerializeField]
	private GameObject _questionnaire;
	[SerializeField]
	private GameObject _feedback;
	private static bool _loaded;
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
		BackToMenu(_mainMenu);
		if (SUGARManager.CurrentUser == null && !_loaded)
		{
			_loaded = true;
			SignIn();
		}
		else
		{
			_signIn.SetActive(SUGARManager.CurrentUser == null);
			if (SUGARManager.CurrentUser != null)
			{
				_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + " " + SUGARManager.CurrentUser.Name;
			}
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
		_teamManagement.SetActive(false);
		_questionnaire.SetActive(false);
		_feedback.SetActive(false);
		_mainMenu.SetActive(true);
		DoBestFit();
		_mainMenu.transform.Find("Load Game").GetComponent<Button>().interactable = GameManagement.GameManager.GetGameNames(Path.Combine(Application.persistentDataPath, "GameSaves")).Count != 0;
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
		go.SetActive(false);
		_teamManagement.SetActive(true);
		_questionnaire.SetActive(false);
		_feedback.SetActive(false);
		((ScreenSideUI)FindObjectOfType(typeof(ScreenSideUI))).ChangeSelected(0);
	}

	/// <summary>
	/// Hide other UI states, show questionnaire
	/// </summary>
	public void GoToQuestionnaire()
	{
		_teamManagement.SetActive(false);
		_questionnaire.SetActive(true);
		_feedback.SetActive(false);
	}

	/// <summary>
	/// Hide other UI states, show feedback
	/// </summary>
	public void GoToFeedback()
	{
		_teamManagement.SetActive(false);
		_questionnaire.SetActive(false);
		_feedback.SetActive(true);
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
				_signIn.SetActive(false);
				_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + " " + SUGARManager.CurrentUser.Name;
				DoBestFit();
			}
			else
			{
				_signIn.SetActive(true);
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

    public static void StaticGoToGame(GameObject go)
    {
        _instance.GoToGame(go);
    }

    public static void StaticBackToMenu(GameObject go)
    {
        _instance.BackToMenu(go);
    }
}
