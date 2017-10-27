using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages menu state changes when escape key is pressed
/// </summary>
public class EscapeAction : MonoBehaviour {
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//if settings panel is open, close settings panel
			if (UIManagement.Settings.gameObject.activeInHierarchy)
			{
			    UIManagement.Settings.transform.parent.gameObject.SetActive(false);
				return;
			}
			//if tutorial quitting pop-up is open, close this pop-up
			if (UIManagement.Tutorial.gameObject.activeInHierarchy)
			{
				var popUp = UIManagement.Tutorial.transform.parent.Find("Quit Tutorial Pop-Up").gameObject;
				if (popUp.activeInHierarchy)
				{
					popUp.SetActive(false);
					return;
				}
			}
			//if recruitment pop-up is open and the player is not in the tutorial, close this pop-up
			if (UIManagement.Recruitment.gameObject.activeInHierarchy)
			{
				if (!UIManagement.Tutorial.gameObject.activeInHierarchy)
				{
				    UIManagement.Recruitment.OnEscape();
				}
				return;
			}
			//if position pop-up is open and the current top pop-up, close this pop-up
			if (UIManagement.PositionDisplay.gameObject.activeInHierarchy)
			{
				if (UIManagement.PositionDisplay.transform.GetSiblingIndex() == UIManagement.PositionDisplay.transform.parent.childCount - 1)
				{
				    UIManagement.PositionDisplay.ClosePositionPopUp(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the meeting pop-up is open and the player isn't in the tutorial, close this pop-up
			if (UIManagement.MemberMeeting.gameObject.activeInHierarchy)
			{
				if (!UIManagement.Tutorial.gameObject.activeInHierarchy)
				{
				    UIManagement.MemberMeeting.OnEscape();
				}
				return;
			}
			//if the race result pop-up is open and the current top pop-up, close this pop-up
			if (UIManagement.RaceResult.gameObject.activeInHierarchy)
			{
				if (UIManagement.RaceResult.transform.GetSiblingIndex() == UIManagement.RaceResult.transform.parent.childCount - 1)
				{
				    UIManagement.RaceResult.Close(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the boat promotion pop-up is open and the current top pop-up, close this pop-up
			if (UIManagement.Promotion.gameObject.activeInHierarchy)
			{
				if (UIManagement.Promotion.transform.GetSiblingIndex() == UIManagement.Promotion.transform.parent.childCount - 1)
				{
				    UIManagement.Promotion.Close(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the pre-race pop-up is open and the current top pop-up, close this pop-up
			if (UIManagement.PreRace.gameObject.activeInHierarchy)
			{
				if (UIManagement.PreRace.GetComponentInChildren<Text>().text == Localization.Get("REPEAT_CONFIRM"))
				{
				    UIManagement.PreRace.CloseRepeatWarning(TrackerTriggerSources.EscapeKey.ToString());
				}
				else
				{
				    UIManagement.PreRace.CloseConfirmPopUp(TrackerTriggerSources.EscapeKey.ToString());
				}
				return;
			}
			//if no pop-ups are open, trigger the OnEscape method in TeamSelectionUI
			if (UIManagement.TeamSelection.gameObject.activeInHierarchy)
			{
			    UIManagement.TeamSelection.OnEscape();
			}
		}
	}
}