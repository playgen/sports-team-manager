using UnityEngine;

public class EscapeAction : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			var tutorial = FindObjectOfType<TutorialController>();
			if (FindObjectOfType<SettingsUI>())
			{
				FindObjectOfType<SettingsUI>().transform.parent.gameObject.SetActive(false);
				return;
			}
			if (tutorial)
			{
				var popUp = tutorial.transform.parent.Find("Quit Tutorial Pop-Up").gameObject;
				if (popUp.activeSelf)
				{
					popUp.SetActive(false);
					return;
				}
			}
			if (FindObjectOfType<RecruitMemberUI>())
			{
				if (!tutorial)
				{
					FindObjectOfType<RecruitMemberUI>().OnEscape();
				}
				return;
			}
			if (FindObjectOfType<PositionDisplayUI>())
			{
				if (FindObjectOfType<PositionDisplayUI>().transform.GetSiblingIndex() == FindObjectOfType<PositionDisplayUI>().transform.parent.childCount - 1)
				{
					FindObjectOfType<PositionDisplayUI>().ClosePositionPopUp();
					return;
				}
			}
			if (FindObjectOfType<MemberMeetingUI>())
			{
				if (!tutorial)
				{
					FindObjectOfType<MemberMeetingUI>().OnEscape();
				}
				return;
			}
			if (FindObjectOfType<TeamSelectionUI>())
			{
				FindObjectOfType<TeamSelectionUI>().OnEscape();
			}
		}
	}
}
