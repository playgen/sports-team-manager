﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(NewGame))]
public class NewGameUI : MonoBehaviour {

	private NewGame _newGame;
	private UIStateManager _stateManager;
	[SerializeField]
	private InputField _boatName;
	[SerializeField]
	private InputField _managerName;
	[SerializeField]
	private InputField _managerAge;
	[SerializeField]
	private Dropdown _managerGender;
	[SerializeField]
	private GameObject _overwritePopUp;
	[SerializeField]
	private Text _errorText;
	[SerializeField]
	private Slider[] _colorSliderPrimary;
	[SerializeField]
	private Slider[] _colorSliderSecondary;
	[SerializeField]
	private Image _colorImagePrimary;
	[SerializeField]
	private Image _colorImageSecondary;

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_newGame = GetComponent<NewGame>();
		_overwritePopUp.SetActive(false);
	}

	/// <summary>
	/// Wipe all errors and provided information
	/// </summary>
	void OnEnable()
	{
		WarningDisable();
		_boatName.text = "";
		_managerName.text = "";
		_managerAge.text = "";
		RandomColor();
	}

	public void RandomColor()
	{
		foreach (Slider s in _colorSliderPrimary)
		{
			s.value = Random.Range(0, 1f);
		}
		foreach (Slider s in _colorSliderSecondary)
		{
			s.value = Random.Range(0, 1f);
		}
		UpdateColor();
	}

	public void UpdateColor()
	{
		_colorImagePrimary.color = new Color(_colorSliderPrimary[0].value, _colorSliderPrimary[1].value, _colorSliderPrimary[2].value);
		_colorImageSecondary.color = new Color(_colorSliderSecondary[0].value, _colorSliderSecondary[1].value, _colorSliderSecondary[2].value);
	}

	/// <summary>
	/// Hide all warnings for missing information
	/// </summary>
	void WarningDisable()
	{
		_errorText.text = "";
		_boatName.transform.Find("Required Warning").gameObject.SetActive(false);
		_managerName.transform.Find("Required Warning").gameObject.SetActive(false);
		_managerAge.transform.Find("Required Warning").gameObject.SetActive(false);
	}

	/// <summary>
	/// Triggered by button click. Check if game with name provided already exists, display overwrite pop-up if one does, create new game if not
	/// </summary>
	public void ExistingGameCheck()
	{
		WarningDisable();
		if (string.IsNullOrEmpty(_boatName.text))
		{
			_boatName.transform.Find("Required Warning").gameObject.SetActive(true);
			return;
		} 
		bool exists = _newGame.ExistingGameCheck(_boatName.text);
		if (exists)
		{
			_overwritePopUp.SetActive(true);
		} else
		{
			NewGame();
		}
	}

	/// <summary>
	/// Check if information provided is valid
	/// </summary>
	public void NewGame()
	{
		Tracker.T.alternative.Selected("New Game", "Created Game", AlternativeTracker.Alternative.Menu);
		var valid = true;
		if (string.IsNullOrEmpty(_boatName.text))
		{
			valid = false;
			_boatName.transform.Find("Required Warning").gameObject.SetActive(true);
		}
		if (string.IsNullOrEmpty(_managerName.text))
		{
			valid = false;
			_managerName.transform.Find("Required Warning").gameObject.SetActive(true);
		}
		if (string.IsNullOrEmpty(_managerAge.text))
		{
			valid = false;
			_managerAge.transform.Find("Required Warning").gameObject.SetActive(true);
		}
		if (valid) {
			float[] colorsPri = new float[]
			{
				_colorImagePrimary.color.r,
				_colorImagePrimary.color.g,
				_colorImagePrimary.color.b
			};
			float[] colorsSec = new float[]
			{
				_colorImageSecondary.color.r,
				_colorImageSecondary.color.g,
				_colorImageSecondary.color.b
			};
			bool success = _newGame.CreateNewGame(_boatName.text, colorsPri, colorsSec, _managerName.text, _managerAge.text, _managerGender.options[_managerGender.value].text);
			if (success)
			{
				_stateManager.GoToGame(gameObject);
				Tracker.T.completable.Initialized("Created New Game", CompletableTracker.Completable.Game);
			} else
			{
				_errorText.text = "Game not created. Please try again.";
			}
		}
	}

	/// <summary>
	/// Send message to UIStateManager to reset UI panels back to the Main Menu
	/// </summary>
	public void BackToMenu()
	{
		_stateManager.BackToMenu(gameObject);
	}
}
