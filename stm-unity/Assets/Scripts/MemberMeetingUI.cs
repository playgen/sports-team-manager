﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PlayGen.RAGE.SportsTeamManager.Simulation;

using PlayGen.SUGAR.Unity;

using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.BestFit;

using RAGE.Analytics.Formats;

/// <summary>
/// Contains all logic related to the CrewMember Meeting pop-up
/// </summary>
[RequireComponent(typeof(MemberMeeting))]
public class MemberMeetingUI : ObservableMonoBehaviour
{
	private MemberMeeting _memberMeeting;
	private CrewMember _currentMember;
	[SerializeField]
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private PositionDisplayUI _positionUI;
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
	[SerializeField]
	private Text[] _textList;
	[SerializeField]
	private Button _roleButton;
	[SerializeField]
	private Image[] _barForegrounds;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Text _statQuestion;
	[SerializeField]
	private Text _roleQuestion;
	[SerializeField]
	private Text _opinionPositiveQuestion;
	[SerializeField]
	private Text _opinionNegativeQuestion;
	[SerializeField]
	private Sprite[] _opinionIcons;
	[SerializeField]
	private Text _closeText;
	[SerializeField]
	private GameObject _fireWarningPopUp;
	[SerializeField]
	private Button _fireButton;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;
	private List<string> _lastReply;
	[SerializeField]
	private HoverPopUpUI _hoverPopUp;

	private void Awake()
	{
		_memberMeeting = GetComponent<MemberMeeting>();
	}

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	/// <summary>
	/// On the GameObject being disabled, hide the fire warning pop-up, all displayed opinions and adjust the order of the pop-ups
	/// </summary>
	private void OnDisable()
	{
		_fireWarningPopUp.SetActive(false);
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _currentMember.Name);
	}

	/// <summary>
	/// Set-up the pop-up for displaying the given CrewMember
	/// </summary>
	public void SetUpDisplay(CrewMember crewMember, string source)
	{
		_currentMember = crewMember;
		//make pop-up visible and firing warning not visible
		gameObject.SetActive(true);
		_fireWarningPopUp.SetActive(false);
		//disable opinion images on CrewMember UI objects
		foreach (var cmui in (CrewMemberUI[])FindObjectsOfType(typeof(CrewMemberUI)))
		{
			if (cmui.Current)
			{
				cmui.transform.Find("Opinion").GetComponent<Image>().enabled = false;
			}
		}
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPopUpOpened", new Dictionary<string, string>
		{
			{ TrackerContextKeys.CrewMemberName.ToString(), crewMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), _memberMeeting.QuestionAllowance().ToString() },
			{ TrackerContextKeys.CurrentSession.ToString(), _memberMeeting.SessionInRace() },
			{ TrackerContextKeys.CrewMemberPosition.ToString(), _memberMeeting.GetCrewMemberPosition(crewMember).ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), _memberMeeting.TeamSize().ToString() },
			{ TrackerContextKeys.TriggerUI.ToString(), source }
		}));
		SUGARManager.GameData.Send("View Crew Member Screen", crewMember.Name);
		Display();
		//set the order of the pop-ups and pop-up blockers and set-up the click event for the blocker
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { CloseCrewMemberPopUp(TrackerTriggerSources.PopUpBlocker.ToString()); });
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	public void Display()
	{
		//ActionAllowance display
		_allowanceBar.fillAmount = _memberMeeting.QuestionAllowance() / (float)_memberMeeting.StartingQuestionAllowance();
		_allowanceText.text = _memberMeeting.QuestionAllowance().ToString();
		//CrewMember avatar
		_avatarDisplay.SetAvatar(_currentMember.Avatar, _currentMember.GetMood());
		//CrewMember information
		_textList[0].text = _currentMember.Name;
		_textList[1].text = _currentMember.Age.ToString();
		var currentRole = _memberMeeting.GetCrewMemberPosition(_currentMember);
		_textList[2].text = currentRole == Position.Null ? Localization.Get("NO_ROLE") : string.Empty;
		_roleButton.onClick.RemoveAllListeners();
		//set up button onclick if CrewMember is positioned
		if (currentRole != Position.Null)
		{
			_roleButton.gameObject.SetActive(true);
			_roleButton.onClick.AddListener(delegate { _positionUI.SetUpDisplay(currentRole, TrackerTriggerSources.TeamManagementScreen.ToString()); });
			_roleButton.GetComponentInChildren<Text>().text = Localization.Get(currentRole.ToString(), true);
			_roleButton.transform.Find("Image").GetComponent<Image>().sprite = _teamSelectionUI.RoleLogos.First(mo => mo.Name == currentRole.ToString()).Image;
		}
		//hide if not positioned
		else
		{
			_roleButton.gameObject.SetActive(false);
		}
		//set stat bar fill amount (foreground) and sprite (background)
		for (var i = 0; i < _barForegrounds.Length; i++)
		{
			_barForegrounds[i].fillAmount = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] * 0.1f;
			_barForegrounds[i].transform.parent.FindChild("Hidden Image").GetComponent<Image>().enabled = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] == 0;
			_barForegrounds[i].transform.parent.FindChild("Skill Image").GetComponent<Image>().enabled = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] != 0;
		}
		//set default starting dialogue
		_dialogueText.text = Localization.Get("MEETING_INTRO");
		//set question text for the player
		_statQuestion.text = Localization.Get(_memberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).First());
		_roleQuestion.text = Localization.Get(_memberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).First());
		_opinionPositiveQuestion.text = Localization.Get(_memberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).First());
		_opinionNegativeQuestion.text = Localization.Get(_memberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).First());
		//set the cost shown for each question and for firing
		_statQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.StatRevealCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
		_roleQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.RoleRevealCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
		_opinionPositiveQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealPositiveCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
		_opinionNegativeQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealNegativeCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
		_fireButton.transform.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.FiringCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
		var allowance = _memberMeeting.QuestionAllowance();
		//set if each button is interactable according to if the player has enough allowance
		_fireButton.interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.FiringCost) && _memberMeeting.CrewEditAllowance() != 0 && _memberMeeting.CanRemoveCheck() && !_memberMeeting.TutorialInProgress();
		_statQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.StatRevealCost);
		_roleQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.RoleRevealCost);
		_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealPositiveCost);
		_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealNegativeCost);
		if (!_fireButton.interactable)
		{
			if (allowance < _memberMeeting.GetConfigValue(ConfigKeys.FiringCost))
			{
				FeedbackHoverOver(_fireButton.transform, "FIRE_BUTTON_HOVER_ALLOWANCE");
			}
			else if (_memberMeeting.CrewEditAllowance() == 0)
			{
				FeedbackHoverOver(_fireButton.transform, Localization.GetAndFormat("FIRE_BUTTON_HOVER_LIMIT", false, _memberMeeting.StartingCrewEditAllowance()));
			}
			else if (_memberMeeting.CanRemoveCheck())
			{
				FeedbackHoverOver(_fireButton.transform, "FIRE_BUTTON_HOVER_CREW_LIMIT");
			}
			else if (!_memberMeeting.TutorialInProgress())
			{
				FeedbackHoverOver(_fireButton.transform, "FIRE_BUTTON_HOVER_TUTORIAL");
			}
		}
		else
		{
			_fireButton.GetComponent<HoverObject>().Enabled = false;
		}

		//set closing text
		_closeText.text = Localization.Get("MEETING_EARLY_EXIT");
		if (_lastReply != null)
		{
			_closeText.text = Localization.Get("MEETING_EXIT");
		}
		//display revealed opinions for each other active CrewMember
		foreach (var crewMember in (CrewMemberUI[])FindObjectsOfType(typeof(CrewMemberUI)))
		{
			if (crewMember.Current)
			{
				var crewName = crewMember.CrewMember.Name;
				var opinionImage = crewMember.transform.Find("Opinion").GetComponent<Image>();
				if (crewName != _currentMember.Name)
				{
					opinionImage.enabled = true;
					opinionImage.sprite = null;
					var opinion = _currentMember.RevealedCrewOpinions[crewName];
					var opinionColorValue = 0.25f + (0.075f*(10 + _currentMember.RevealedCrewOpinionAges[crewName]));
					opinionColorValue = opinionColorValue < 0.25f ? 0.25f : opinionColorValue;
					opinionImage.color = new UnityEngine.Color(opinionColorValue, opinionColorValue, opinionColorValue);
					opinionImage.sprite = _opinionIcons[(opinion > 0 ? Mathf.CeilToInt(opinion / 3f) : Mathf.FloorToInt(opinion / 3f)) + 2];
				}
			}
		}
		var managerOpinionImage = transform.Find("Manager Opinion").GetComponent<Image>();
		managerOpinionImage.enabled = true;
		managerOpinionImage.sprite = null;
		var managerName = _memberMeeting.GetManagerName();
		var managerOpinion = _currentMember.RevealedCrewOpinions[managerName];
		var managerOpinionColorValue = 0.25f + (0.075f * (10 + _currentMember.RevealedCrewOpinionAges[managerName]));
		managerOpinionColorValue = managerOpinionColorValue < 0.25f ? 0.25f : managerOpinionColorValue;
		managerOpinionImage.color = new UnityEngine.Color(managerOpinionColorValue, managerOpinionColorValue, managerOpinionColorValue);
		managerOpinionImage.sprite = _opinionIcons[(managerOpinion > 0 ? Mathf.CeilToInt(managerOpinion / 3f) : Mathf.FloorToInt(managerOpinion / 3f)) + 2];
		DoBestFit();
	}

	/// <summary>
	/// Triggered by button. Sends provided question to Simulation, gets and displays reply from NPC in response.
	/// </summary>
	public void AskQuestion(string questionType)
	{
		var allowanceBefore = _memberMeeting.QuestionAllowance();
		var reply = _memberMeeting.AskQuestion(questionType, _currentMember);
		_lastReply = reply;
		Display();
		var replyExtras = reply.Count > 0 ? reply.Where(r => r != reply.First()).Select(r => Localization.Get(r)).ToArray() : new string[0];
		_dialogueText.text = reply.Count > 0 ? Localization.GetAndFormat(reply.First(), false, replyExtras) : string.Empty;
		TrackerEventSender.SendEvent(new TraceEvent("MeetingQuestionAsked", new Dictionary<string, string>
		{
			{ TrackerContextKeys.QuestionAsked.ToString(), questionType },
			{ TrackerContextKeys.QuestionCost.ToString(), (allowanceBefore - _memberMeeting.QuestionAllowance()).ToString() },
			{ TrackerContextKeys.CrewMemberName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), allowanceBefore.ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), _memberMeeting.TeamSize().ToString() }
		}));
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		SUGARManager.GameData.Send("Meeting Question Directed At", _currentMember.Name);
		SUGARManager.GameData.Send("Meeting Question Asked", questionType);
	}

	/// <summary>
	/// Triggered by button. Display warning to player that they are about to fire a character.
	/// </summary>
	public void FireCrewWarning()
	{
		TrackerEventSender.SendEvent(new TraceEvent("FirePopUpOpened", new Dictionary<string, string>
		{
			{ TrackerContextKeys.CrewMemberName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), _memberMeeting.QuestionAllowance().ToString() },
			{ TrackerContextKeys.CurrentSession.ToString(), _memberMeeting.SessionInRace() },
			{ TrackerContextKeys.CrewMemberPosition.ToString(), _memberMeeting.GetCrewMemberPosition(_currentMember).ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), _memberMeeting.TeamSize().ToString() }
		}));
		_fireWarningPopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_fireWarningPopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { CloseFireCrewWarning(TrackerTriggerSources.PopUpBlocker.ToString()); });
		DoBestFit();
	}

	/// <summary>
	/// Triggered by button. Close fire warning pop-up.
	/// </summary>
	public void CloseFireCrewWarning(string source)
	{
		TrackerEventSender.SendEvent(new TraceEvent("FirePopUpClosed", new Dictionary<string, string>
		{
			{ TrackerContextKeys.PositionName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.TriggerUI.ToString(), source }
		}));
		_fireWarningPopUp.SetActive(false);
		if (gameObject.activeInHierarchy)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(delegate { CloseCrewMemberPopUp(TrackerTriggerSources.PopUpBlocker.ToString()); });
		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Triggered by button. Fires a character from the team.
	/// </summary>
	public void FireCrew()
	{
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberFired", new Dictionary<string, string>
		{
			{ TrackerContextKeys.CrewMemberName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), _memberMeeting.QuestionAllowance().ToString() },
			{ TrackerContextKeys.CurrentSession.ToString(), _memberMeeting.SessionInRace() },
			{ TrackerContextKeys.CrewMemberPosition.ToString(), _memberMeeting.GetCrewMemberPosition(_currentMember).ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), _memberMeeting.TeamSize().ToString() },
			{ TrackerContextKeys.FiringCost.ToString(), _memberMeeting.GetConfigValue(ConfigKeys.FiringCost).ToString() }
		}));
		SUGARManager.GameData.Send("Crew Member Fired", true);
		_memberMeeting.FireCrewMember(_currentMember);
		_teamSelectionUI.ResetCrew();
		CloseFireCrewWarning(string.Empty);
		CloseCrewMemberPopUp(string.Empty);
	}

	/// <summary>
	/// Hide the pop-up for Crew Member details
	/// </summary>
	public void CloseCrewMemberPopUp(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPopUpClosed", new Dictionary<string, string>
			{
				{ TrackerContextKeys.PositionName.ToString(), _currentMember.Name },
				{ TrackerContextKeys.TriggerUI.ToString(), source }
			}));
		}
		gameObject.SetActive(false);
		foreach (var crewMember in (CrewMemberUI[])FindObjectsOfType(typeof(CrewMemberUI)))
		{
			if (crewMember.Current)
			{
				crewMember.transform.Find("Opinion").GetComponent<Image>().enabled = false;
			}
		}
		_positionUI.ChangeBlockerOrder();
		_lastReply = null;
	}

	public void OnEscape()
	{
		if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
		{
			CloseCrewMemberPopUp(TrackerTriggerSources.EscapeKey.ToString());
		}
		else if (_fireWarningPopUp.activeInHierarchy)
		{
			CloseFireCrewWarning(TrackerTriggerSources.EscapeKey.ToString());
		}
	}

	private void OnLanguageChange()
	{
		var currentRole = _memberMeeting.GetCrewMemberPosition(_currentMember);
		_textList[2].text = currentRole == Position.Null ? Localization.Get("NO_ROLE") : string.Empty;
		if (currentRole != Position.Null)
		{
			_roleButton.GetComponentInChildren<Text>().text = Localization.Get(currentRole.ToString(), true);
		}
		_closeText.text = Localization.Get("MEETING_EARLY_EXIT");
		if (_lastReply != null)
		{
			_closeText.text = Localization.Get("MEETING_EXIT");
		}
		_statQuestion.text = Localization.Get(_memberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).First());
		_roleQuestion.text = Localization.Get(_memberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).First());
		_opinionPositiveQuestion.text = Localization.Get(_memberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).First());
		_opinionNegativeQuestion.text = Localization.Get(_memberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).First());
		_dialogueText.text = _lastReply != null ? Localization.GetAndFormat(_lastReply.First(), false, _lastReply.Where(r => r != _lastReply.First()).ToArray()) : Localization.Get("MEETING_INTRO");
		DoBestFit();
	}

	/// <summary>
	/// Set up pointer enter and exit events for created objects that can be hovered over
	/// </summary>
	private void FeedbackHoverOver(Transform feedback, string text)
	{
		feedback.GetComponent<HoverObject>().Enabled = true;
		feedback.GetComponent<HoverObject>().SetHoverText(text, _hoverPopUp);
	}

	private void DoBestFit()
	{
		_textList.BestFit();
		new[] { _dialogueText, _statQuestion, _roleQuestion, _opinionPositiveQuestion, _opinionNegativeQuestion, _closeText }.BestFit();
		_barForegrounds.Select(b => b.transform.parent.gameObject).BestFit();
		_fireWarningPopUp.GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
	}
}