using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Contains all UI logic related to creating new games
/// </summary>
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
	[SerializeField]
	private Toggle _tutorialToggle;

	private void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_newGame = GetComponent<NewGame>();
		_overwritePopUp.SetActive(false);
		WarningDisable();
		RandomColor();
	}

	private void OnEnable()
	{
		_managerGender.ClearOptions();
		_managerGender.AddOptions(new List<string> { Localization.Get("MALE"), Localization.Get("FEMALE") });
		_tutorialToggle.enabled = _newGame.ExistingSaves();
		_tutorialToggle.isOn = true;
	}

	/// <summary>
	/// Remove any invalid puncuation not already blocked by each inputfield
	/// </summary>
	public void ExtraNameValidation(InputField inputField)
	{
		inputField.text = inputField.text.Replace("'", "");
	}

	private void Update()
	{
		//ensure that age does not go below 0
		int ageTest;
		if (int.TryParse(_managerAge.text, out ageTest) && ageTest < 0)
		{
			_managerAge.text = (ageTest * -1).ToString();
		}
		//code for tabbing between input fields
		if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject != null)
		{
			var next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().navigation.selectOnDown;

			if (next != null)
			{
				var inputfield = next.GetComponent<InputField>();
				if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
				inputfield.MoveTextEnd(true);
				EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
			}
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			ExistingGameCheck();
		}
	}

	/// <summary>
	/// Set the color sliders to random values
	/// </summary>
	public void RandomColor()
	{
		foreach (var s in _colorSliderPrimary)
		{
			s.value = Random.Range(0, 1f);
		}
		foreach (var s in _colorSliderSecondary)
		{
			s.value = Random.Range(0, 1f);
		}
		UpdateColor();
	}

	/// <summary>
	/// Update the displayed color to match what has been selected using the sliders
	/// </summary>
	public void UpdateColor()
	{
		_colorImagePrimary.color = new Color(_colorSliderPrimary[0].value, _colorSliderPrimary[1].value, _colorSliderPrimary[2].value);
		_colorImageSecondary.color = new Color(_colorSliderSecondary[0].value, _colorSliderSecondary[1].value, _colorSliderSecondary[2].value);
	}

	/// <summary>
	/// Hide all warnings for missing information
	/// </summary>
	private void WarningDisable()
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
		int ageTest;
		if (!int.TryParse(_managerAge.text, out ageTest))
		{
			valid = false;
			_managerAge.transform.Find("Required Warning").gameObject.SetActive(true);
		}
		if (valid)
		{
			var exists = _newGame.ExistingGameCheck(_boatName.text);
			if (exists)
			{
				_overwritePopUp.SetActive(true);
			}
			else
			{
				NewGame();
			}
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
		var success = _newGame.CreateNewGame(_boatName.text, colorsPri, colorsSec, _managerName.text, _managerAge.text, _managerGender.options[_managerGender.value].text, _tutorialToggle.isOn);
		if (success)
		{
			_stateManager.GoToGame(gameObject);
		}
		else
		{
			_errorText.text = Localization.Get("NEW_GAME_CREATION_ERROR");
		}
	}
}
