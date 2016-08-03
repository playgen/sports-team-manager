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

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_newGame = GetComponent<NewGame>();
	}

	public void ExistingGameCheck()
	{
		bool exists = _newGame.ExistingGameCheck(_boatName.text);
		if (exists)
		{
			print("Game already exists");
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
			print("Team name must be provided!");
			valid = false;
		}
		if (string.IsNullOrEmpty(_managerName.text))
		{
			print("Manager name must be provided!");
			valid = false;
		}
		if (string.IsNullOrEmpty(_managerAge.text))
		{
			print("Manager age must be provided!");
			valid = false;
		}
		if (valid) {
			bool success = _newGame.CreateNewGame(_boatName.text, _managerName.text, _managerAge.text, _managerGender.options[_managerGender.value].text);
			if (success)
			{
				print("Game created");
			} else
			{
				print("Game not created");
			}
		}
	}

	public void BackToMenu()
	{
		_stateManager.BackToMenu(gameObject);
	}
}
