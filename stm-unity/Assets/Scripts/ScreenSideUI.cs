using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PlayGen.Unity.Utilities.BestFit;

/// <summary>
/// Controls the UI displayed at the top of the screen
/// </summary>
public class ScreenSideUI : MonoBehaviour {
	[SerializeField]
	private Text _nameText;
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
		transform.FindObject("Side Menu/Team Management").Active(GameManagement.PlatformSettings.Rage);
		transform.FindObject("Side Menu/Achievements").Active(GameManagement.PlatformSettings.Rage);
		transform.FindObject("Side Menu/Leaderboards").Active(GameManagement.PlatformSettings.Rage);
		Invoke("DoBestFit", 0f);
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
		_quitWarningPopUp.GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
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
		transform.Find("Side Menu").BestFit();
	}
}