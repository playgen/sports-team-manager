using UnityEngine;

/// <summary>
/// Manages menu state changes when escape key is pressed
/// </summary>
public class EscapeAction : MonoBehaviour {

    [SerializeField]
    private TutorialController _tutorial;
    [SerializeField]
    private SettingsUI _settings;
    [SerializeField]
    private RecruitMemberUI _recruitment;
    [SerializeField]
    private PositionDisplayUI _position;
    [SerializeField]
    private MemberMeetingUI _meeting;
    [SerializeField]
    private TeamSelectionUI _teamSelection;
    [SerializeField]
    private ScreenSideUI _side;

    void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//if settings panel is open, close settings panel
			if (_settings.gameObject.activeInHierarchy)
			{
                _settings.transform.parent.gameObject.SetActive(false);
				return;
			}
			//if tutorial quitting pop-up is open, close this pop-up
			if (_tutorial.gameObject.activeInHierarchy)
			{
				var popUp = _tutorial.transform.parent.Find("Quit Tutorial Pop-Up").gameObject;
				if (popUp.activeInHierarchy)
				{
					popUp.SetActive(false);
					return;
				}
			}
			//if recruitment pop-up is open and the player is not in the tutorial, close this pop-up
			if (_recruitment.gameObject.activeInHierarchy)
			{
				if (!_tutorial.gameObject.activeInHierarchy)
				{
                    _recruitment.OnEscape();
				}
				return;
			}
			//if position pop-up is open and the current top pop-up, close this pop-up
			if (_position.gameObject.activeInHierarchy)
			{
				if (_position.transform.GetSiblingIndex() == _position.transform.parent.childCount - 1)
				{
                    _position.ClosePositionPopUp(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the meeting pop-up is open and the player isn't in the tutorial, close this pop-up
			if (_meeting.gameObject.activeInHierarchy)
			{
				if (!_tutorial.gameObject.activeInHierarchy)
				{
                    _meeting.OnEscape();
				}
				return;
			}
			//if no pop-ups are open, trigger the OnEscape method in TeamSelectionUI
			if (_teamSelection.gameObject.activeInHierarchy)
			{
                _teamSelection.OnEscape();
			}
		}
	}
}
