using PlayGen.Unity.Utilities.Extensions;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages menu state changes when escape key is pressed
/// </summary>
public class EscapeAction : MonoBehaviour {
	private void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//if settings panel is open, close settings panel
			if (UIManagement.Settings.gameObject.activeInHierarchy)
			{
			    UIManagement.Settings.transform.Parent().Active(false);
				return;
			}
			//if tutorial quitting pop-up is open, close this pop-up
			if (UIManagement.Tutorial.gameObject.activeInHierarchy)
			{
				var popUp = UIManagement.Tutorial.transform.parent.FindObject("Quit Tutorial Pop-Up");
				if (popUp.activeInHierarchy)
				{
					popUp.Active(false);
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
				    UIManagement.PositionDisplay.ClosePositionPopUp(TrackerTriggerSource.EscapeKey.ToString());
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
				    UIManagement.RaceResult.Close(TrackerTriggerSource.EscapeKey.ToString());
					return;
				}
			}
			//if the boat promotion pop-up is open and the current top pop-up, close this pop-up
			if (UIManagement.Promotion.gameObject.activeInHierarchy)
			{
				if (UIManagement.Promotion.transform.GetSiblingIndex() == UIManagement.Promotion.transform.parent.childCount - 1)
				{
				    UIManagement.Promotion.Close(TrackerTriggerSource.EscapeKey.ToString());
					return;
				}
			}
			//if the pre-race pop-up is open and the current top pop-up, close this pop-up
			if (UIManagement.PreRace.gameObject.activeInHierarchy)
			{
				if (UIManagement.PreRace.GetComponentInChildren<Text>().text == Localization.Get("REPEAT_CONFIRM"))
				{
				    UIManagement.PreRace.CloseRepeatWarning(TrackerTriggerSource.EscapeKey.ToString());
				}
				else
				{
				    UIManagement.PreRace.CloseConfirmPopUp(TrackerTriggerSource.EscapeKey.ToString());
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