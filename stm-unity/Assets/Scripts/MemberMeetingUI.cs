using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.SUGAR.Unity;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

/// <summary>
/// Contains all logic related to the CrewMember Meeting pop-up
/// </summary>
public class MemberMeetingUI : MonoBehaviour
{
	private CrewMember _currentMember;
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private Text _ageText;
	[SerializeField]
	private Text _roleText;
	[SerializeField]
	private Button _roleButton;
	[SerializeField]
	private List<Transform> _skills;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private List<Transform> _questions;
	[SerializeField]
	private Sprite[] _opinionIcons;
	private Dictionary<string, Sprite> _opinionIconDict;
	[SerializeField]
	private Text _closeText;
	[SerializeField]
	private GameObject _fireWarningPopUp;
	[SerializeField]
	private Button _fireButton;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;
	private List<string> _lastReply;

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
		_fireWarningPopUp.Active(false);
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _currentMember.Name);
	}

	/// <summary>
	/// Set-up the pop-up for displaying the given CrewMember
	/// </summary>
	public void SetUpDisplay(CrewMember crewMember, string source)
	{
		if (!GameManagement.SeasonOngoing)
		{
			return;
		}
		if (_opinionIconDict == null)
		{
			_opinionIconDict = _opinionIcons.ToDictionary(o => o.name.Replace("Icon_Box_", string.Empty), o => o);
		}
		_currentMember = crewMember;
		EnsureVisible();
		//make pop-up visible and firing warning not visible
		gameObject.Active(true);
		_fireWarningPopUp.Active(false);
		//disable opinion images on CrewMember UI objects
		ResetOpinionIcons();
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CrewMemberName, crewMember.Name },
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.CurrentSession, GameManagement.CurrentSessionString },
			{ TrackerContextKey.CrewMemberPosition, crewMember.BoatPosition() },
			{ TrackerContextKey.SizeOfTeam, GameManagement.CrewCount },
			{ TrackerContextKey.TriggerUI, source },
			{ TrackerContextKey.CrewMemberSessionsInTeam, crewMember.SessionsIncluded() }
		}, AccessibleTracker.Accessible.Screen));
		SUGARManager.GameData.Send("View Crew Member Screen", crewMember.Name);
		Display();
		//set the order of the pop-ups and pop-up blockers and set-up the click event for the blocker
		transform.EnableSmallBlocker(() => CloseCrewMemberPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	public void Display()
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		//ActionAllowance display
		_allowanceBar.fillAmount = GameManagement.ActionAllowancePercentage;
		_allowanceText.text = GameManagement.ActionAllowance.ToString();
		//CrewMember avatar
		var mood = _currentMember.GetMood();
		_avatarDisplay.SetAvatar(_currentMember.Avatar, mood);
		_avatarDisplay.GetComponent<Image>().color = new UnityEngine.Color(0, 1, 1);
		_avatarDisplay.Parent().GetComponent<Image>().color = AvatarDisplay.MoodColor(mood);
		//CrewMember information
		var currentRole = _currentMember.BoatPosition();
		_nameText.text = _currentMember.Name;
		_ageText.text = _currentMember.Age.ToString();
		//set up button if CrewMember is positioned, hide if not
		_roleButton.gameObject.Active(currentRole != Position.Null);
		if (currentRole != Position.Null)
		{
			_roleButton.onClick.RemoveAllListeners();
			_roleButton.onClick.AddListener(() => UIManagement.PositionDisplay.SetUpDisplay(currentRole, TrackerTriggerSource.TeamManagementScreen.ToString()));
			_roleButton.transform.FindImage("Image").sprite = UIManagement.TeamSelection.RoleLogos[currentRole.ToString()];
		}
		//set stat bar fill amount (foreground) and sprite (background)
		foreach (var skill in _skills)
		{
			var skillvalue = _currentMember.RevealedSkills[(Skill)Enum.Parse(typeof(Skill), skill.name)];
			skill.FindImage("Foreground Bar").fillAmount = skillvalue * 0.1f;
			skill.FindImage("Hidden Image").enabled = skillvalue == 0;
			skill.FindImage("Skill Image").enabled = skillvalue != 0;
		}
		foreach (var question in _questions)
		{
			var configKey = (ConfigKey)Enum.Parse(typeof(ConfigKey), $"{question.name}Cost");
			question.FindText("Cost/Amount").text = configKey.ValueString(true, _currentMember);
			question.GetComponent<Button>().interactable = configKey.Affordable(_currentMember);
		}
		_fireButton.transform.FindText("Cost/Amount").text = ConfigKey.FiringCost.ValueString();
		_fireButton.interactable = GameManagement.CanRemoveFromCrew;
		if (!_fireButton.interactable)
		{
			if (!ConfigKey.FiringCost.Affordable())
			{
				FeedbackHoverOver("FIRE_BUTTON_HOVER_ALLOWANCE");
			}
			else if (!GameManagement.CrewEditAllowed)
			{
				FeedbackHoverOver(Localization.GetAndFormat("FIRE_BUTTON_HOVER_LIMIT", false, GameManagement.StartingCrewEditAllowance));
			}
			else if (GameManagement.Team.CanRemoveFromCrew())
			{
				FeedbackHoverOver("FIRE_BUTTON_HOVER_CREW_LIMIT");
			}
			else if (!GameManagement.ShowTutorial)
			{
				FeedbackHoverOver("FIRE_BUTTON_HOVER_TUTORIAL");
			}
		}
		else
		{
			FeedbackHoverOver();
		}
		//display revealed opinions for each other active CrewMember
		DisplayOpinions();
		var managerOpinionImage = transform.FindComponentInChildren<Image>("Manager Opinion");
		var managerOpinion = _currentMember.RevealedCrewOpinions[GameManagement.ManagerName];
		managerOpinionImage.sprite = GetOpinionIcon(managerOpinion);
		OnLanguageChange();
	}

	public void DisplayOpinions(bool accurate = true)
	{
		var forceCycle = 0;
		foreach (var crewMember in UIManagement.CrewMemberUI)
		{
			if (crewMember.Current)
			{
				var crewName = crewMember.CrewMember.Name;
				var opinionImage = crewMember.transform.FindImage("Opinion");
				if (crewName != _currentMember.Name)
				{
					opinionImage.enabled = true;
					opinionImage.sprite = accurate ? GetOpinionIcon(_currentMember.RevealedCrewOpinions[crewName]) : _opinionIcons[forceCycle % 5];
					forceCycle++;
				}
			}
		}
	}

	private Sprite GetOpinionIcon(int opinion)
	{
		if (opinion >= ConfigKey.OpinionStrongLike.Value())
		{
			return _opinionIconDict["Strongly_Agree"];
		}
		if (opinion >= ConfigKey.OpinionLike.Value())
		{
			return _opinionIconDict["Agree"];
		}
		if (opinion <= ConfigKey.OpinionDislike.Value())
		{
			return _opinionIconDict["Disagree"];
		}
		return opinion <= ConfigKey.OpinionStrongDislike.Value() ? _opinionIconDict["Strongly_Disagree"] : _opinionIconDict["Neutral"];
	}

	/// <summary>
	/// Triggered by button. Sends provided question to Simulation, gets and displays reply from NPC in response.
	/// </summary>
	public void AskQuestion(GameObject clicked)
	{
		var questionType = clicked.name;
		var allowanceBefore = GameManagement.ActionAllowance;
		var reply = GameManagement.GameManager.SendMeetingEvent(questionType, _currentMember);
		_lastReply = reply;
		Display();
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		TrackerEventSender.SendEvent(new TraceEvent("MeetingQuestionAsked", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.QuestionAsked, questionType },
			{ TrackerContextKey.QuestionCost, allowanceBefore - GameManagement.ActionAllowance },
			{ TrackerContextKey.CrewMemberName, _currentMember.Name },
			{ TrackerContextKey.CurrentTalkTime, allowanceBefore },
			{ TrackerContextKey.SizeOfTeam, GameManagement.CrewCount }
		}, questionType, AlternativeTracker.Alternative.Question));
		UIManagement.TeamSelection.SortCrew();
		EnsureVisible();
		SUGARManager.GameData.Send("Meeting Question Directed At", _currentMember.Name);
		SUGARManager.GameData.Send("Meeting Question Asked", questionType);
	}

	private void EnsureVisible()
	{
		//adjust the crew member scroll rect position to ensure this crew member is shown
		var memberTransform = UIManagement.TeamSelection.CrewContainer.GetComponentsInChildren<CrewMemberUI>().First(c => c.CrewMember == _currentMember && !c.Usable).RectTransform();
		if (!memberTransform.IsRectTransformVisible(memberTransform.parent.parent.RectTransform()))
		{
			UIManagement.TeamSelection.CrewContainerPaging(0);
		}
		if (!memberTransform.IsRectTransformVisible(memberTransform.parent.parent.RectTransform()))
		{
			UIManagement.TeamSelection.CrewContainerPaging(1);
		}
	}

	/// <summary>
	/// Triggered by button. Displays the Notes UI for the currently selected crew member.
	/// </summary>
	public void DisplayNotes()
	{
		UIManagement.Notes.Display(_currentMember.Name);
	}

	/// <summary>
	/// Triggered by button. Display warning to player that they are about to fire a character.
	/// </summary>
	public void FireCrewWarning()
	{
		TrackerEventSender.SendEvent(new TraceEvent("FirePopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CrewMemberName, _currentMember.Name },
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.CurrentSession, GameManagement.CurrentSessionString },
			{ TrackerContextKey.CrewMemberPosition, _currentMember.BoatPosition() },
			{ TrackerContextKey.SizeOfTeam, GameManagement.CrewCount },
			{ TrackerContextKey.CrewMemberSessionsInTeam, _currentMember.SessionsIncluded() }
		}, AccessibleTracker.Accessible.Screen));
		_fireWarningPopUp.Active(true);
		_fireWarningPopUp.transform.EnableSmallBlocker(() => CloseFireCrewWarning(TrackerTriggerSource.PopUpBlocker.ToString()));
		DoBestFit();
	}

	/// <summary>
	/// Triggered by button. Close fire warning pop-up.
	/// </summary>
	public void CloseFireCrewWarning(string source)
	{
		TrackerEventSender.SendEvent(new TraceEvent("FirePopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.PositionName, _currentMember.Name },
			{ TrackerContextKey.TriggerUI, source }
		}, AccessibleTracker.Accessible.Screen));
		_fireWarningPopUp.Active(false);
		if (gameObject.activeInHierarchy)
		{
			transform.EnableSmallBlocker(() => CloseCrewMemberPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
		}
		else
		{
			UIManagement.DisableSmallBlocker();
		}
	}

	/// <summary>
	/// Triggered by button. Fires a character from the team.
	/// </summary>
	public void FireCrew()
	{
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberFired", TrackerAsset.Verb.Interacted, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CrewMemberName, _currentMember.Name },
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.CurrentSession, GameManagement.CurrentSessionString },
			{ TrackerContextKey.CrewMemberPosition, _currentMember.BoatPosition() },
			{ TrackerContextKey.SizeOfTeam, GameManagement.CrewCount },
			{ TrackerContextKey.FiringCost, ConfigKey.FiringCost.ValueString(false) },
			{ TrackerContextKey.CrewMemberSessionsInTeam, _currentMember.SessionsIncluded() }
		}, GameObjectTracker.TrackedGameObject.Npc));
		SUGARManager.GameData.Send("Crew Member Fired", true);
		GameManagement.GameManager.RetireCrewMember(_currentMember);
		UIManagement.TeamSelection.ResetCrew();
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
			TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.PositionName, _currentMember.Name },
				{ TrackerContextKey.TriggerUI, source }
			}, AccessibleTracker.Accessible.Screen));
			gameObject.Active(false);
			ResetOpinionIcons();
			_lastReply = null;
		}
		UIManagement.PositionDisplay.ChangeBlockerOrder();
	}

	private static void ResetOpinionIcons()
	{
		foreach (var crewMember in UIManagement.CrewMemberUI)
		{
			if (crewMember.Current)
			{
				crewMember.transform.FindImage("Opinion").enabled = false;
			}
		}
	}

	/// <summary>
	/// On the escape key being pressed, close pop-up or fire warning pop-up if open
	/// </summary>
	public void OnEscape()
	{
		if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
		{
			CloseCrewMemberPopUp(TrackerTriggerSource.EscapeKey.ToString());
		}
		else if (_fireWarningPopUp.activeInHierarchy)
		{
			CloseFireCrewWarning(TrackerTriggerSource.EscapeKey.ToString());
		}
	}

	/// <summary>
	/// Set up pointer enter and exit events for created objects that can be hovered over
	/// </summary>
	private void FeedbackHoverOver(string text = "")
	{
		_fireButton.transform.GetComponent<HoverObject>().SetHoverText(text);
	}

	/// <summary>
	/// When langauge is changed, redraw UI elements set in code
	/// </summary>
	private void OnLanguageChange()
	{
		var currentRole = _currentMember.BoatPosition();
		_roleText.text = currentRole == Position.Null ? Localization.Get("NO_ROLE") : string.Empty;
		if (currentRole != Position.Null)
		{
			_roleButton.GetComponentInChildren<Text>().text = Localization.Get(currentRole.ToString());
		}
		_closeText.text = _lastReply == null ? Localization.Get("MEETING_EXIT") : Localization.Get("MEETING_EARLY_EXIT");
		foreach (var question in _questions)
		{
			question.FindText("Question").text = question.name.EventString();
		}
		_dialogueText.text = _lastReply != null ? Localization.GetAndFormat(_lastReply.First(), false, _lastReply.Where(r => r != _lastReply.First()).Select(l => Localization.Get(l)).ToArray()) : Localization.Get("MEETING_INTRO_" + _currentMember.GetSocialImportanceRating(GameManagement.ManagerName));
		DoBestFit();
	}

	private void DoBestFit()
	{
		new Component[] { _nameText, _ageText, _roleText }.BestFit();
		_questions.Select(q => q.FindText("Question")).Concat(new Component[] { _dialogueText, _closeText }).BestFit();
		_skills.BestFit();
		_fireWarningPopUp.GetComponentsInChildren<Button>().ToList().BestFit();
	}
}