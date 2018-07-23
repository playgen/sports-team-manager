using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Loading;
using PlayGen.Unity.Utilities.Localization;

using UnityEngine.UI.Extensions.ColorPicker;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

/// <summary>
/// Contains all UI logic related to creating new games
/// </summary>
public class NewGameUI : MonoBehaviour
{
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
		_boatName.onValueChanged.AddListener(s => WarningDisable());
		_managerName.onValueChanged.AddListener(s => WarningDisable());
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
			Invoke(nameof(WarningDisable), 0.25f);
		}
		return newChar;
	}

	private void OnEnable()
	{
		if (GameManagement.DemoMode)
		{
			_tutorialToggle.transform.Parent().Active(false);
			_tutorialToggle.isOn = false;
			if (string.IsNullOrEmpty(_boatName.text))
			{
				_boatName.text = "Demo" + (GameManagement.GameCount + 1);
			}
			if (string.IsNullOrEmpty(_managerName.text))
			{
				_managerName.text = "Demo" + (GameManagement.GameCount + 1);
			}
		}
		else
		{
			_tutorialToggle.enabled = GameManagement.GameCount != 0;
			_tutorialToggle.isOn = true;
		}
		BestFit.ResolutionChange += DoBestFit;
		DoBestFit();
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
		_colorSliderSecondary.GetComponentInParent<ColorPickerControl>().AssignColor(ColorValues.Hue, _colorSliderSecondary.value);
		UpdatePrimaryColor(_colorSliderPrimary.GetComponentInParent<ColorPickerControl>().CurrentColor);
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
		if (string.IsNullOrEmpty(_boatName.text) || _boatName.text.Trim().Length == 0)
		{
			_boatName.transform.FindObject("Required Warning").Active(true);
			return;
		}
		if (string.IsNullOrEmpty(_managerName.text) || _managerName.text.Trim().Length == 0)
		{
			_managerName.transform.FindObject("Required Warning").Active(true);
			return;
		}
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

	/// <summary>
	/// Check if information provided is valid and creates new game if so
	/// </summary>
	public void NewGame()
	{
		var colorsPri = new PlayGen.RAGE.SportsTeamManager.Simulation.Color((int)(_colorImagePrimary.color.r * 255), (int)(_colorImagePrimary.color.g * 255), (int)(_colorImagePrimary.color.b * 255));
		var colorsSec = new PlayGen.RAGE.SportsTeamManager.Simulation.Color((int)(_colorImageSecondary.color.r * 255), (int)(_colorImageSecondary.color.g * 255), (int)(_colorImageSecondary.color.b * 255));
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
					TrackerEventSender.SendEvent(new TraceEvent("GameStarted", TrackerAsset.Verb.Initialized, new Dictionary<TrackerContextKey, object>
					{
						{ TrackerContextKey.GameName, GameManagement.TeamName },
						{ TrackerContextKey.BoatLayout, GameManagement.PositionString }
					}, CompletableTracker.Completable.Game));
					TrackerEventSender.SendEvaluationEvent(TrackerEvalautionEvent.UserProfile, new Dictionary<TrackerEvaluationKey, string> { { TrackerEvaluationKey.Event, "setupnewteam" } });
					UIStateManager.StaticGoToGame();
					Loading.Stop();
					return;
				}
			}
			_errorText.text = Localization.Get("NEW_GAME_CREATION_ERROR");
			Loading.Stop();
		});
	}

	private void DoBestFit()
	{
		var size = GetComponentsInChildren<InputField>().ToList().BestFit();
		GetComponentsInChildren<Text>().ToList().ForEach(t => t.fontSize = size);
		transform.Find("Buttons").BestFit();
		_overwritePopUp.GetComponentsInChildren<Button>().Where(t => t.gameObject != _overwritePopUp).ToList().BestFit();
	}
}