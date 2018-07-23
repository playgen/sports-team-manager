using System.Collections.Generic;

using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Extensions;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.Text;

using RAGE.EvaluationAsset;

using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField]
	private Image _logo;
	[SerializeField]
	private GameObject _newGameButton;
	[SerializeField]
	private GameObject _loadGameButton;
	[SerializeField]
	private GameObject _settingsButton;
	[SerializeField]
	private GameObject _signInButton;
	[SerializeField]
	private Text _userSignedInText;
	[SerializeField]
	private GameObject _exitButton;
	private static bool _loaded;

	private void Start()
	{
		_logo.enabled = true;
		transform.FindObject("Buttons").Active(true);
		if (GameManagement.DemoMode)
		{
			_loadGameButton.Active(false);
			_settingsButton.Active(false);
			_signInButton.Active(false);
			_userSignedInText.gameObject.Active(false);
		}
		else if (GameManagement.RageMode)
		{
			if (SUGARManager.CurrentUser == null && !_loaded)
			{
				_loaded = true;
				SignIn();
			}
			else
			{
				_signInButton.Active(false);
				OnLanguageChange();
			}
		}
		else
		{
			_signInButton.Active(false);
		}
		if (GameManagement.DemoMode || Application.platform != RuntimePlatform.WindowsPlayer)
		{
			_exitButton.Active(false);
		}
		_loadGameButton.GetComponent<Button>().interactable = GameManagement.GameCount != 0;
	}

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		DoBestFit();
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Close game when escape is pressed on the main menu
	/// </summary>
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CloseGame();
		}
	}

	public void GoToNewGame()
	{
		UIManagement.StateManager.GoToState(State.NewGame);
	}

	public void GoToLoadGame()
	{
		UIManagement.StateManager.GoToState(State.LoadGame);
	}

	/// <summary>
	/// Close the game
	/// </summary>
	public void CloseGame()
	{
		Application.Quit();
	}

	/// <summary>
	/// Trigger showing SUGAR sign-in screen or run auto sign-in if setting is active
	/// </summary>
	public void SignIn()
	{
		_signInButton.Active(true);
		_userSignedInText.gameObject.SetActive(false);
		SUGARManager.Account.DisplayPanel(success =>
		{
			if (success)
			{
				_signInButton.Active(false);
				OnLanguageChange();
				DoBestFit();
				var settings = new EvaluationAssetSettings { PlayerId = SUGARManager.CurrentUser.Name };
				EvaluationAsset.Instance.Settings = settings;
				TrackerEventSender.SendEvaluationEvent(TrackerEvalautionEvent.UserProfile, new Dictionary<TrackerEvaluationKey, string> { { TrackerEvaluationKey.Event, "sugarsignin" } });
			}
			else
			{
				_signInButton.Active(true);
			}
		});
	}

	private void OnLanguageChange()
	{
		if (GameManagement.RageMode && SUGARManager.CurrentUser != null)
		{
			_userSignedInText.gameObject.Active(true);
			_userSignedInText.text = Localization.Get("SIGNED_IN_AS") + " " + SUGARManager.CurrentUser.Name;
		}
	}

	private void DoBestFit()
	{
		new[] { _newGameButton , _loadGameButton, _settingsButton, _signInButton, _userSignedInText.gameObject, _exitButton}.BestFit(false);
	}
}
