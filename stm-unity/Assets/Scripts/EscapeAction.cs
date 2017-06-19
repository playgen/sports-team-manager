using UnityEngine;

/// <summary>
/// Manages menu state changes when escape key is pressed
/// </summary>
public class EscapeAction : MonoBehaviour {

	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			var tutorial = FindObjectOfType<TutorialController>();
			//if settings panel is open, close settings panel
			if (FindObjectOfType<SettingsUI>())
			{
				FindObjectOfType<SettingsUI>().transform.parent.gameObject.SetActive(false);
				return;
			}
			//if tutorial quitting pop-up is open, close this pop-up
			if (tutorial)
			{
				var popUp = tutorial.transform.parent.Find("Quit Tutorial Pop-Up").gameObject;
				if (popUp.activeSelf)
				{
					popUp.SetActive(false);
					return;
				}
			}
			//if recruitment pop-up is open and the player is not in the tutorial, close this pop-up
			if (FindObjectOfType<RecruitMemberUI>())
			{
				if (!tutorial)
				{
					FindObjectOfType<RecruitMemberUI>().OnEscape();
				}
				return;
			}
			//if position pop-up is open and the current top pop-up, close this pop-up
			if (FindObjectOfType<PositionDisplayUI>())
			{
				if (FindObjectOfType<PositionDisplayUI>().transform.GetSiblingIndex() == FindObjectOfType<PositionDisplayUI>().transform.parent.childCount - 1)
				{
					FindObjectOfType<PositionDisplayUI>().ClosePositionPopUp(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the meeting pop-up is open and the player isn't in the tutorial, close this pop-up
			if (FindObjectOfType<MemberMeetingUI>())
			{
				if (!tutorial)
				{
					FindObjectOfType<MemberMeetingUI>().OnEscape();
				}
				return;
			}
			//if no pop-ups are open, trigger the OnEscape method in TeamSelectionUI
			if (FindObjectOfType<TeamSelectionUI>())
			{
				FindObjectOfType<TeamSelectionUI>().OnEscape();
			}
		}
	}
}
