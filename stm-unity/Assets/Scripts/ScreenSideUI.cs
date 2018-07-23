using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PlayGen.Unity.Utilities.Text;

/// <summary>
/// Controls the UI displayed at the top of the screen
/// </summary>
public class ScreenSideUI : MonoBehaviour
{
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private GameObject _settings;
	[SerializeField]
	private GameObject _teamManagement;
	[SerializeField]
	private GameObject _achievements;
	[SerializeField]
	private GameObject _leaderboards;
	[SerializeField]
	private GameObject _quitWarningPopUp;
	[SerializeField]
	private Button _popUpBlocker;

	/// <summary>
	/// Set the information displayed at the top of the screen
	/// </summary>
	private void OnEnable()
	{
		_nameText.text = GameManagement.TeamName;
		BestFit.ResolutionChange += DoBestFit;
		_settings.Active(!GameManagement.DemoMode);
		_teamManagement.Active(GameManagement.RageMode);
		_achievements.Active(GameManagement.RageMode);
		_leaderboards.Active(GameManagement.RageMode);
		DoBestFit();
	}

	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Change the position of the selected object to match the current UI screen
	/// </summary>
	public void DisplayQuitWarning()
	{
		_popUpBlocker.gameObject.Active(true);
		_quitWarningPopUp.GetComponentsInChildren<Button>().ToList().BestFit();
	}

	/// <summary>
	/// Close the pop-up displayed before quitting back to the main menu
	/// </summary>
	public void CloseQuitWarning()
	{
		_popUpBlocker.gameObject.Active(false);
	}

	private void DoBestFit()
	{
		new[] { _teamManagement, _achievements, _leaderboards }.BestFit();
	}
}