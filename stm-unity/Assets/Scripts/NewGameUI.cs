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

	private void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_newGame = GetComponent<NewGame>();
		_overwritePopUp.SetActive(false);
		WarningDisable();
		RandomColor();
	}

	public void ExtraNameValidation(InputField inputField)
	{
		inputField.text = inputField.text.Replace("'", "");
	}

	private void Update()
	{
		int ageTest;
		if (int.TryParse(_managerAge.text, out ageTest) && ageTest < 0)
		{
			_managerAge.text = (ageTest * -1).ToString();
		}
		//code for tabbing between input fields
		if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject != null)
		{
			Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().navigation.selectOnDown;

			if (next != null)
			{
				InputField inputfield = next.GetComponent<InputField>();
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
			bool exists = _newGame.ExistingGameCheck(_boatName.text);
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
		Tracker.T.alternative.Selected("New Game", "Created Game", AlternativeTracker.Alternative.Menu);
		int[] colorsPri = new int[]
			{
				(int)(_colorImagePrimary.color.r * 255),
				(int)(_colorImagePrimary.color.g * 255),
				(int)(_colorImagePrimary.color.b * 255)
			};
		int[] colorsSec = new int[]
		{
				(int)(_colorImageSecondary.color.r * 255),
				(int)(_colorImageSecondary.color.g * 255),
				(int)(_colorImageSecondary.color.b * 255)
		};
		bool success = _newGame.CreateNewGame(_boatName.text, colorsPri, colorsSec, _managerName.text, _managerAge.text, _managerGender.options[_managerGender.value].text);
		if (success)
		{
			_stateManager.GoToGame(gameObject);
			Tracker.T.completable.Initialized("Created New Game", CompletableTracker.Completable.Game);
		}
		else
		{
			_errorText.text = "Game not created. Please try again.";
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
