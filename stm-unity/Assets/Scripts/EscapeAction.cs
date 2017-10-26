using System.Linq;

using PlayGen.Unity.Utilities.Localization;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages menu state changes when escape key is pressed
/// </summary>
public class EscapeAction : MonoBehaviour {

	private TutorialController _tutorial;
	private SettingsUI _settings;
	private RecruitMemberUI _recruitment;
	private PositionDisplayUI _position;
	private MemberMeetingUI _meeting;
	private TeamSelectionUI _teamSelection;
	private RaceResultUI _raceResult;
	private CupResultUI _cupResult;
	private BoatPromotionUI _boatPromotion;
	private PreRaceConfirmUI _preRace;

	private void Start()
	{
		_tutorial = transform.root.GetComponentsInChildren<TutorialController>(true).First();
		_settings = transform.root.GetComponentsInChildren<SettingsUI>(true).Last();
		_recruitment = transform.root.GetComponentsInChildren<RecruitMemberUI>(true).First();
		_position = transform.root.GetComponentsInChildren<PositionDisplayUI>(true).First();
		_meeting = transform.root.GetComponentsInChildren<MemberMeetingUI>(true).First();
		_teamSelection = transform.root.GetComponentsInChildren<TeamSelectionUI>(true).First();
		_raceResult = transform.root.GetComponentsInChildren<RaceResultUI>(true).First();
		_cupResult = transform.root.GetComponentsInChildren<CupResultUI>(true).First();
		_boatPromotion = transform.root.GetComponentsInChildren<BoatPromotionUI>(true).First();
		_preRace = transform.root.GetComponentsInChildren<PreRaceConfirmUI>(true).First();
	}

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
			//if the race result pop-up is open and the current top pop-up, close this pop-up
			if (_raceResult.gameObject.activeInHierarchy)
			{
				if (_raceResult.transform.GetSiblingIndex() == _raceResult.transform.parent.childCount - 1)
				{
					_raceResult.Close(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the cup result pop-up is open and the current top pop-up, close this pop-up
			if (_cupResult.gameObject.activeInHierarchy)
			{
				if (_cupResult.transform.GetSiblingIndex() == _cupResult.transform.parent.childCount - 1)
				{
					_cupResult.Close(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the boat promotion pop-up is open and the current top pop-up, close this pop-up
			if (_boatPromotion.gameObject.activeInHierarchy)
			{
				if (_boatPromotion.transform.GetSiblingIndex() == _boatPromotion.transform.parent.childCount - 1)
				{
					_boatPromotion.Close(TrackerTriggerSources.EscapeKey.ToString());
					return;
				}
			}
			//if the pre-race pop-up is open and the current top pop-up, close this pop-up
			if (_preRace.gameObject.activeInHierarchy)
			{
				if (_preRace.GetComponentInChildren<Text>().text == Localization.Get("REPEAT_CONFIRM"))
				{
					_preRace.CloseRepeatWarning(TrackerTriggerSources.EscapeKey.ToString());
				}
				else
				{
					_preRace.CloseConfirmPopUp(TrackerTriggerSources.EscapeKey.ToString());
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