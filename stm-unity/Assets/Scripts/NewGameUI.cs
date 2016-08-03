using UnityEngine;
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

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_newGame = GetComponent<NewGame>();
		_overwritePopUp.SetActive(false);
	}

	void OnEnable()
	{
		WarningDisable();
		_boatName.text = "";
		_managerName.text = "";
		_managerAge.text = "";
	}

	void WarningDisable()
	{
		_errorText.text = "";
		_boatName.transform.Find("Required Warning").gameObject.SetActive(false);
		_managerName.transform.Find("Required Warning").gameObject.SetActive(false);
		_managerAge.transform.Find("Required Warning").gameObject.SetActive(false);
	}

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

	public void NewGame()
	{
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
			bool success = _newGame.CreateNewGame(_boatName.text, _managerName.text, _managerAge.text, _managerGender.options[_managerGender.value].text);
			if (success)
			{
				print("Game created");
			} else
			{
				_errorText.text = "Game not created. Please try again.";
			}
		}
	}

	public void BackToMenu()
	{
		_stateManager.BackToMenu(gameObject);
	}
}
