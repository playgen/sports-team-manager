using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Loading;
using PlayGen.Unity.Utilities.Localization;

using UnityEngine.UI.Extensions.ColorPicker;

/// <summary>
/// Contains all UI logic related to creating new games
/// </summary>
public class NewGameUI : MonoBehaviour {

	[SerializeField]
	private InputField _boatName;
	[SerializeField]
	private InputField _managerName;
	[SerializeField]
	private GameObject _overwritePopUp;
	[SerializeField]
	private Text _errorText;
	[SerializeField]
	private Slider _colorSliderPrimary;
	[SerializeField]
	private Slider _colorSliderSecondary;
	[SerializeField]
	private Image _colorImagePrimary;
	[SerializeField]
	private Image _colorImageSecondary;
	[SerializeField]
	private Toggle _tutorialToggle;

	private void Awake()
	{
		_overwritePopUp.Active(false);
		WarningDisable();
		RandomColor();
		_boatName.onValidateInput += (input, charIndex, addedChar) => InvalidFlash(addedChar, _boatName);
		_managerName.onValidateInput += (input, charIndex, addedChar) => InvalidFlash(addedChar, _managerName);
	}

	/// <summary>
	/// Display a flash on the text field if an invalid character is typed
	/// </summary>
	private char InvalidFlash(char newChar, InputField inputField)
	{
		if (!char.IsLetterOrDigit(newChar) && newChar != ' ')
		{
			newChar = '\0';
			inputField.transform.FindObject("Warning").Active(true);
			Invoke("WarningDisable", 0.02f);
		}
		return newChar;
	}

	private void OnEnable()
	{
		if (GameManagement.PlatformSettings.DemoMode)
		{
			_tutorialToggle.transform.parent.gameObject.Active(false);
			_tutorialToggle.isOn = false;
			if (string.IsNullOrEmpty(_boatName.text))
			{
				_boatName.text = "Demo" + (GameManagement.GameNames.Count + 1);
			}
			if (string.IsNullOrEmpty(_managerName.text))
			{
				_managerName.text = "Demo" + (GameManagement.GameNames.Count + 1);
			}
		}
		else
		{
			_tutorialToggle.enabled = GameManagement.GameNames.Count != 0;
			_tutorialToggle.isOn = true;
		}
		BestFit.ResolutionChange += DoBestFit;
		Invoke("DoBestFit", 0f);
	}

	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Set the color sliders to random values
	/// </summary>
	public void RandomColor()
	{
		_colorSliderPrimary.value = Random.Range(0, 1f);
		_colorSliderSecondary.value = Random.Range(0, 1f);
		_colorSliderPrimary.GetComponentInParent<ColorPickerControl>().AssignColor(ColorValues.Hue, _colorSliderPrimary.value);
		UpdatePrimaryColor(_colorSliderPrimary.GetComponentInParent<ColorPickerControl>().CurrentColor);
		_colorSliderSecondary.GetComponentInParent<ColorPickerControl>().AssignColor(ColorValues.Hue, _colorSliderSecondary.value);
		UpdateSecondaryColor(_colorSliderSecondary.GetComponentInParent<ColorPickerControl>().CurrentColor);
	}

	/// <summary>
	/// Update the displayed color to match what has been selected using the sliders
	/// </summary>
	public void UpdatePrimaryColor(Color color)
	{
		_colorImagePrimary.color = color;
	}

	public void UpdateSecondaryColor(Color color)
	{
		_colorImageSecondary.color = color;
	}

	/// <summary>
	/// Hide all warnings for missing information
	/// </summary>
	private void WarningDisable()
	{
		_errorText.text = string.Empty;
		_boatName.transform.FindObject("Required Warning").Active(false);
		_managerName.transform.FindObject("Required Warning").Active(false);
		_boatName.transform.FindObject("Warning").Active(false);
		_managerName.transform.FindObject("Warning").Active(false);
	}

	/// <summary>
	/// Triggered by button click. Check if game with name provided already exists, display overwrite pop-up if one does, create new game if not
	/// </summary>
	public void ExistingGameCheck()
	{
		WarningDisable();
		var valid = true;
		if (string.IsNullOrEmpty(_boatName.text) || _boatName.text.Trim().Length == 0)
		{
			valid = false;
			_boatName.transform.FindObject("Required Warning").Active(true);
		}
		if (string.IsNullOrEmpty(_managerName.text) || _managerName.text.Trim().Length == 0)
		{
			valid = false;
			_managerName.transform.FindObject("Required Warning").Active(true);
		}
		if (valid)
		{
			GameManagement.GameManager.CheckIfGameExistsTask(Path.Combine(Application.persistentDataPath, "GameSaves"), _boatName.text, (completed, exists) =>
			{
				if (completed)
				{
					if (exists)
					{
						_overwritePopUp.Active(true);
						DoBestFit();
					}
					else
					{
						NewGame();
					}
				}
			});
		}
	}

	/// <summary>
	/// Check if information provided is valid and creates new game if so
	/// </summary>
	public void NewGame()
	{
		//convert selected colors to bytes
		var colorsPri = new []
			{
				(byte)(_colorImagePrimary.color.r * 255),
				(byte)(_colorImagePrimary.color.g * 255),
				(byte)(_colorImagePrimary.color.b * 255)
			};
		var colorsSec = new []
		{
				(byte)(_colorImageSecondary.color.r * 255),
				(byte)(_colorImageSecondary.color.g * 255),
				(byte)(_colorImageSecondary.color.b * 255)
		};
		_boatName.text = _boatName.text.TrimEnd();
		_managerName.text = _managerName.text.TrimEnd();
		var language = string.IsNullOrEmpty(Localization.SelectedLanguage.Parent.Name) ? Localization.SelectedLanguage.EnglishName : Localization.SelectedLanguage.Parent.EnglishName;
		Loading.Start();
		GameManagement.GameManager.NewGameTask(Path.Combine(Application.persistentDataPath, "GameSaves"), _boatName.text, colorsPri, colorsSec, _managerName.text, _tutorialToggle.isOn, language, success =>
		{
			if (success)
			{
				if (GameManagement.Team != null && GameManagement.TeamName == _boatName.text)
				{
					var newString = GameManagement.PositionString;
					TrackerEventSender.SendEvent(new TraceEvent("GameStarted", TrackerVerbs.Initialized, new Dictionary<string, string>
					{
						{ TrackerContextKeys.GameName.ToString(), GameManagement.TeamName },
						{ TrackerContextKeys.BoatLayout.ToString(), newString }
					}, CompletableTracker.Completable.Game));
					TrackerEventSender.SendEvaluationEvent(TrackerEvalautionEvents.UserProfile, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, "setupnewteam" } });
					UIStateManager.StaticGoToGame();
				}
				else
				{
					_errorText.text = Localization.Get("NEW_GAME_CREATION_ERROR");
				}
			}
			else
			{
				_errorText.text = Localization.Get("NEW_GAME_CREATION_ERROR");
			}
			Loading.Stop();
		});
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Text>().Where(t => t.gameObject.activeInHierarchy && (t.text.Length > 0 || t.GetComponentInParent<InputField>())).BestFit();
		_overwritePopUp.GetComponentsInChildren<Button>().Where(t => t.gameObject != _overwritePopUp).Select(t => t.gameObject).BestFit();
	}
}