﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
public class MemberMeetingUI : MonoBehaviour
{
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
	[SerializeField]
	private ScrollRect _crewContainer;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	/// <summary>
	/// On the GameObject being disabled, hide the fire warning pop-up and remove event listeners
	/// </summary>
	private void OnDisable()
	{
		_fireWarningPopUp.SetActive(false);
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
		TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _currentMember.Name);
	}

	/// <summary>
	/// Set-up the pop-up for displaying the given CrewMember
	/// </summary>
	public void SetUpDisplay(CrewMember crewMember, string source)
	{
		_currentMember = crewMember;

		//adjust the crew member scroll rect position to ensure this crew member is shown
		var memberTransform = _crewContainer.GetComponentsInChildren<CrewMemberUI>().First(c => c.CrewMember == crewMember && !c.Usable).GetComponent<RectTransform>();
		if (!memberTransform.IsRectTransformVisible((RectTransform)memberTransform.parent.parent))
		{
			_crewContainer.horizontalNormalizedPosition = 0;
			if (!memberTransform.IsRectTransformVisible((RectTransform)memberTransform.parent.parent))
			{
				_crewContainer.horizontalNormalizedPosition = 1;
			}
		}

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
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.CrewMemberName.ToString(), crewMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
			{ TrackerContextKeys.CurrentSession.ToString(), GameManagement.CurrentSessionString },
			{ TrackerContextKeys.CrewMemberPosition.ToString(), crewMember.GetBoatPosition(GameManagement.PositionCrew).ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), GameManagement.CrewMembers.Count.ToString() },
			{ TrackerContextKeys.TriggerUI.ToString(), source },
			{ TrackerContextKeys.CrewMemberSessionsInTeam.ToString(), GameManagement.MemberMeeting.GetTimeInTeam(crewMember).ToString() },
		}, AccessibleTracker.Accessible.Screen));
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
		_allowanceBar.fillAmount = GameManagement.ActionAllowance / (float)GameManagement.StartingActionAllowance;
		_allowanceText.text = GameManagement.ActionAllowance.ToString();
		//CrewMember avatar
		_avatarDisplay.SetAvatar(_currentMember.Avatar, _currentMember.GetMood());
		//CrewMember information
		_textList[0].text = _currentMember.Name;
		_textList[1].text = _currentMember.Age.ToString();
		var currentRole = _currentMember.GetBoatPosition(GameManagement.PositionCrew);
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
			_barForegrounds[i].transform.parent.Find("Hidden Image").GetComponent<Image>().enabled = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] == 0;
			_barForegrounds[i].transform.parent.Find("Skill Image").GetComponent<Image>().enabled = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] != 0;
		}
		//set default starting dialogue
		_dialogueText.text = Localization.Get("MEETING_INTRO_" + _currentMember.GetSocialImportanceRating());
		//set question text for the player
		_statQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).First());
		_roleQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).First());
		_opinionPositiveQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).First());
		_opinionNegativeQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).First());
		//set the cost shown for each question and for firing
		_statQuestion.transform.parent.Find("Image/Text").GetComponent<Text>().text = GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.StatRevealCost, _currentMember).ToString(Localization.SpecificSelectedLanguage);
		_roleQuestion.transform.parent.Find("Image/Text").GetComponent<Text>().text = GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.RoleRevealCost).ToString(Localization.SpecificSelectedLanguage);
		_opinionPositiveQuestion.transform.parent.Find("Image/Text").GetComponent<Text>().text = GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.OpinionRevealPositiveCost).ToString(Localization.SpecificSelectedLanguage);
		_opinionNegativeQuestion.transform.parent.Find("Image/Text").GetComponent<Text>().text = GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.OpinionRevealNegativeCost).ToString(Localization.SpecificSelectedLanguage);
		_fireButton.transform.Find("Image/Text").GetComponent<Text>().text = GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.FiringCost).ToString(Localization.SpecificSelectedLanguage);
		//set if each button is interactable according to if the player has enough allowance
		_fireButton.interactable = GameManagement.ActionAllowance >= GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.FiringCost) && GameManagement.CrewEditAllowance != 0 && GameManagement.Team.CanRemoveFromCrew() && !GameManagement.ShowTutorial;
		_statQuestion.GetComponentInParent<Button>().interactable = GameManagement.ActionAllowance >= GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.StatRevealCost, _currentMember);
		_roleQuestion.GetComponentInParent<Button>().interactable = GameManagement.ActionAllowance >= GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.RoleRevealCost);
		_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = GameManagement.ActionAllowance >= GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.OpinionRevealPositiveCost);
		_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = GameManagement.ActionAllowance >= GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.OpinionRevealNegativeCost);
		if (!_fireButton.interactable)
		{
			if (GameManagement.ActionAllowance < GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.FiringCost))
			{
				FeedbackHoverOver(_fireButton.transform, "FIRE_BUTTON_HOVER_ALLOWANCE");
			}
			else if (GameManagement.CrewEditAllowance == 0)
			{
				FeedbackHoverOver(_fireButton.transform, Localization.GetAndFormat("FIRE_BUTTON_HOVER_LIMIT", false, GameManagement.StartingCrewEditAllowance));
			}
			else if (GameManagement.Team.CanRemoveFromCrew())
			{
				FeedbackHoverOver(_fireButton.transform, "FIRE_BUTTON_HOVER_CREW_LIMIT");
			}
			else if (!GameManagement.ShowTutorial)
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
					opinionImage.sprite = _opinionIcons[(opinion > 0 ? Mathf.CeilToInt(opinion / 3f) : Mathf.FloorToInt(opinion / 3f)) + 2];
				}
			}
		}
		var managerOpinionImage = transform.Find("Manager Opinion").GetComponent<Image>();
		managerOpinionImage.enabled = true;
		managerOpinionImage.sprite = null;
		var managerOpinion = _currentMember.RevealedCrewOpinions[GameManagement.Manager.Name];
		managerOpinionImage.color = UnityEngine.Color.cyan;
		managerOpinionImage.sprite = _opinionIcons[(managerOpinion > 0 ? Mathf.CeilToInt(managerOpinion / 3f) : Mathf.FloorToInt(managerOpinion / 3f)) + 2];
		DoBestFit();
	}

	/// <summary>
	/// Triggered by button. Sends provided question to Simulation, gets and displays reply from NPC in response.
	/// </summary>
	public void AskQuestion(string questionType)
	{
		var allowanceBefore = GameManagement.ActionAllowance;
		var reply = GameManagement.MemberMeeting.AskQuestion(questionType, _currentMember);
		_lastReply = reply;
		Display();
		var replyExtras = reply.Count > 0 ? reply.Where(r => r != reply.First()).Select(r => Localization.Get(r)).ToArray() : new string[0];
		_dialogueText.text = reply.Count > 0 ? Localization.GetAndFormat(reply.First(), false, replyExtras) : string.Empty;
		TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		TrackerEventSender.SendEvent(new TraceEvent("MeetingQuestionAsked", TrackerVerbs.Selected, new Dictionary<string, string>
		{
			{ TrackerContextKeys.QuestionAsked.ToString(), questionType },
			{ TrackerContextKeys.QuestionCost.ToString(), (allowanceBefore - GameManagement.ActionAllowance).ToString() },
			{ TrackerContextKeys.CrewMemberName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), allowanceBefore.ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), GameManagement.CrewMembers.Count.ToString() }
		}, questionType, AlternativeTracker.Alternative.Question));
		SUGARManager.GameData.Send("Meeting Question Directed At", _currentMember.Name);
		SUGARManager.GameData.Send("Meeting Question Asked", questionType);
	}

	/// <summary>
	/// Triggered by button. Display warning to player that they are about to fire a character.
	/// </summary>
	public void FireCrewWarning()
	{
		TrackerEventSender.SendEvent(new TraceEvent("FirePopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.CrewMemberName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
			{ TrackerContextKeys.CurrentSession.ToString(), GameManagement.CurrentSessionString },
			{ TrackerContextKeys.CrewMemberPosition.ToString(), _currentMember.GetBoatPosition(GameManagement.PositionCrew).ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), GameManagement.CrewMembers.Count.ToString() },
			{ TrackerContextKeys.CrewMemberSessionsInTeam.ToString(), GameManagement.MemberMeeting.GetTimeInTeam(_currentMember).ToString() },
		}, AccessibleTracker.Accessible.Screen));
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
		TrackerEventSender.SendEvent(new TraceEvent("FirePopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
		{
			{ TrackerContextKeys.PositionName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.TriggerUI.ToString(), source }
		}, AccessibleTracker.Accessible.Screen));
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
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberFired", TrackerVerbs.Interacted, new Dictionary<string, string>
		{
			{ TrackerContextKeys.CrewMemberName.ToString(), _currentMember.Name },
			{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
			{ TrackerContextKeys.CurrentSession.ToString(), GameManagement.CurrentSessionString },
			{ TrackerContextKeys.CrewMemberPosition.ToString(), _currentMember.GetBoatPosition(GameManagement.PositionCrew).ToString() },
			{ TrackerContextKeys.SizeOfTeam.ToString(), GameManagement.CrewMembers.Count.ToString() },
			{ TrackerContextKeys.FiringCost.ToString(), GameManagement.MemberMeeting.GetConfigValue(ConfigKeys.FiringCost).ToString(CultureInfo.InvariantCulture) },
			{ TrackerContextKeys.CrewMemberSessionsInTeam.ToString(), GameManagement.MemberMeeting.GetTimeInTeam(_currentMember).ToString() },
		}, GameObjectTracker.TrackedGameObject.Npc));
		SUGARManager.GameData.Send("Crew Member Fired", true);
		GameManagement.MemberMeeting.FireCrewMember(_currentMember);
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
			TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.PositionName.ToString(), _currentMember.Name },
				{ TrackerContextKeys.TriggerUI.ToString(), source }
			}, AccessibleTracker.Accessible.Screen));
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

	/// <summary>
	/// On the escape key being pressed, close pop-up or fire warning pop-up if open
	/// </summary>
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

	/// <summary>
	/// When langauge is changed, redraw UI elements set in code
	/// </summary>
	private void OnLanguageChange()
	{
		var currentRole = _currentMember.GetBoatPosition(GameManagement.PositionCrew);
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
		_statQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).First());
		_roleQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).First());
		_opinionPositiveQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).First());
		_opinionNegativeQuestion.text = Localization.Get(GameManagement.MemberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).First());
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