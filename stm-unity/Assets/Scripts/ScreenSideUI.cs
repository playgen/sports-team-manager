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
	private GameObject _selected;
	[SerializeField]
	private GameObject _quitWarningPopUp;
	[SerializeField]
	private Button _popUpBlocker;

	/// <summary>
	/// Set the information displayed at the top of the screen
	/// </summary>
	private void OnEnable()
	{
		_nameText.text = GameManagement.TeamName.ToUpper();
		BestFit.ResolutionChange += DoBestFit;
		Invoke("DoBestFit", 0f);
	}

	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Change the position of the selected object to match the current UI screen
	/// </summary>
	public void ChangeSelected(int position)
	{
		((RectTransform)_selected.transform).anchorMax = new Vector2(0.25f + (0.15f * position), 1);
		((RectTransform)_selected.transform).anchorMin = new Vector2(0.1f + (0.15f * position), 0);
		((RectTransform)_selected.transform).anchoredPosition = Vector2.zero;
	}

	/// <summary>
	/// Change the position of the selected object to match the current UI screen
	/// </summary>
	public void DisplayQuitWarning()
	{
		_popUpBlocker.gameObject.SetActive(true);
		_quitWarningPopUp.GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
	}

	/// <summary>
	/// Close the pop-up displayed before quitting back to the main menu
	/// </summary>
	public void CloseQuitWarning()
	{
		_popUpBlocker.gameObject.SetActive(false);
	}

	private void DoBestFit()
	{
		transform.Find("Side Menu").BestFit();
	}
}