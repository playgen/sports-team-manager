using System.Collections.Generic;
using System.Globalization;
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
	private Button _notesButton;
	[SerializeField]
	private GameObject _fireWarningPopUp;
	[SerializeField]
	private Button _fireButton;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;
	private List<string> _lastReply;
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
		_currentMember = crewMember;

		//adjust the crew member scroll rect position to ensure this crew member is shown
		var memberTransform = _crewContainer.GetComponentsInChildren<CrewMemberUI>().First(c => c.CrewMember == crewMember && !c.Usable).RectTransform();
		if (!memberTransform.IsRectTransformVisible(memberTransform.parent.parent.RectTransform()))
		{
			_crewContainer.horizontalNormalizedPosition = 0;
			if (!memberTransform.IsRectTransformVisible(memberTransform.parent.parent.RectTransform()))
			{
				_crewContainer.horizontalNormalizedPosition = 1;
			}
		}

		//make pop-up visible and firing warning not visible
		gameObject.Active(true);
		_fireWarningPopUp.Active(false);
		//disable opinion images on CrewMember UI objects
		foreach (var cmui in UIManagement.CrewMemberUI)
		{
			if (cmui.Current)
			{
				cmui.transform.FindImage("Opinion").enabled = false;
			}
		}
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
		//TODO try to simplify this as much as possible
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		//ActionAllowance display
		_allowanceBar.fillAmount = GameManagement.ActionAllowancePercentage;
		_allowanceText.text = GameManagement.ActionAllowance.ToString();
		//CrewMember avatar
		_avatarDisplay.SetAvatar(_currentMember.Avatar, _currentMember.GetMood());
		_avatarDisplay.GetComponent<Image>().color = new UnityEngine.Color(0, 1, 1);
		_avatarDisplay.transform.parent.GetComponent<Image>().color = AvatarDisplay.MoodColor(_currentMember.GetMood());
		//CrewMember information
		var currentRole = _currentMember.BoatPosition();
		_textList[0].text = _currentMember.Name;
		_textList[1].text = _currentMember.Age.ToString();
		_textList[2].text = currentRole == Position.Null ? Localization.Get("NO_ROLE") : string.Empty;
		_textList[3].text = Localization.Get("NAME") + ":";
		_textList[4].text = Localization.Get("AGE") + ":";
		_textList[5].text = Localization.Get("ROLE") + ":";
		_notesButton.onClick.RemoveAllListeners();
		_notesButton.onClick.AddListener(() => UIManagement.Notes.Display(_currentMember.Name));
		_roleButton.onClick.RemoveAllListeners();
		//set up button onclick if CrewMember is positioned
		if (currentRole != Position.Null)
		{
			_roleButton.gameObject.Active(true);
			_roleButton.onClick.AddListener(() => UIManagement.PositionDisplay.SetUpDisplay(currentRole, TrackerTriggerSource.TeamManagementScreen.ToString()));
			_roleButton.GetComponentInChildren<Text>().text = Localization.Get(currentRole.ToString());
			_roleButton.transform.FindImage("Image").sprite = UIManagement.TeamSelection.RoleLogos.First(mo => mo.Name == currentRole.ToString()).Image;
		}
		//hide if not positioned
		else
		{
			_roleButton.gameObject.Active(false);
		}
		//set stat bar fill amount (foreground) and sprite (background)
		for (var i = 0; i < _barForegrounds.Length; i++)
		{
			_barForegrounds[i].fillAmount = _currentMember.RevealedSkills[(Skill)Mathf.Pow(2, i)] * 0.1f;
			_barForegrounds[i].transform.parent.FindImage("Hidden Image").enabled = _currentMember.RevealedSkills[(Skill)Mathf.Pow(2, i)] == 0;
			_barForegrounds[i].transform.parent.FindImage("Skill Image").enabled = _currentMember.RevealedSkills[(Skill)Mathf.Pow(2, i)] != 0;
		}
		//set default starting dialogue
		_dialogueText.text = Localization.Get("MEETING_INTRO_" + _currentMember.GetSocialImportanceRating(GameManagement.ManagerName));
		//set question text for the player
		_statQuestion.text = "StatReveal".EventString();
		_roleQuestion.text = "RoleReveal".EventString();
		_opinionPositiveQuestion.text = "OpinionRevealPositive".EventString();
		_opinionNegativeQuestion.text = "OpinionRevealNegative".EventString();
		//set the cost shown for each question and for firing
		_statQuestion.transform.parent.FindText("Image/Text").text = ConfigKey.StatRevealCost.Value(_currentMember).ToString(Localization.SpecificSelectedLanguage);
		_roleQuestion.transform.parent.FindText("Image/Text").text = ConfigKey.RoleRevealCost.Value().ToString(Localization.SpecificSelectedLanguage);
		_opinionPositiveQuestion.transform.parent.FindText("Image/Text").text = ConfigKey.OpinionRevealPositiveCost.Value().ToString(Localization.SpecificSelectedLanguage);
		_opinionNegativeQuestion.transform.parent.FindText("Image/Text").text = ConfigKey.OpinionRevealNegativeCost.Value().ToString(Localization.SpecificSelectedLanguage);
		_fireButton.transform.FindText("Image/Text").text = ConfigKey.FiringCost.Value().ToString(Localization.SpecificSelectedLanguage);
		//set if each button is interactable according to if the player has enough allowance
		_fireButton.interactable = ConfigKey.FiringCost.Affordable() && GameManagement.CrewEditAllowed && GameManagement.CanRemoveFromCrew && !GameManagement.ShowTutorial;
		_statQuestion.GetComponentInParent<Button>().interactable = ConfigKey.StatRevealCost.Affordable(_currentMember);
		_roleQuestion.GetComponentInParent<Button>().interactable = ConfigKey.RoleRevealCost.Affordable();
		_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = ConfigKey.OpinionRevealPositiveCost.Affordable();
		_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = ConfigKey.OpinionRevealNegativeCost.Affordable();
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
			else if (GameManagement.CanRemoveFromCrew)
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

		//set closing text
		_closeText.text = Localization.Get("MEETING_EARLY_EXIT");
		if (_lastReply != null)
		{
			_closeText.text = Localization.Get("MEETING_EXIT");
		}
		//display revealed opinions for each other active CrewMember
		ForcedOpinionChange("accurate");
		var managerOpinionImage = transform.FindComponentInChildren<Image>("Manager Opinion");
		managerOpinionImage.enabled = true;
		managerOpinionImage.sprite = null;
		var managerOpinion = _currentMember.RevealedCrewOpinions[GameManagement.ManagerName];
		managerOpinionImage.sprite = _opinionIcons[(managerOpinion > 0 ? Mathf.CeilToInt(managerOpinion / 3f) : Mathf.FloorToInt(managerOpinion / 3f)) + 2];
		DoBestFit();
	}

	public void ForcedOpinionChange(string change)
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
					opinionImage.sprite = null;
					var opinion = _currentMember.RevealedCrewOpinions[crewName];
					if (change == "accurate")
					{
						opinionImage.sprite = _opinionIcons[(opinion > 0 ? Mathf.CeilToInt(opinion / 3f) : Mathf.FloorToInt(opinion / 3f)) + 2];
					}
					else
					{
						opinionImage.sprite = _opinionIcons[forceCycle % 5];
						forceCycle++;
					}
				}
			}
		}
	}

	/// <summary>
	/// Triggered by button. Sends provided question to Simulation, gets and displays reply from NPC in response.
	/// </summary>
	public void AskQuestion(string questionType)
	{
		var allowanceBefore = GameManagement.ActionAllowance;
		var reply = GameManagement.GameManager.SendMeetingEvent(questionType, _currentMember);
		_lastReply = reply;
		Display();
		var replyExtras = reply.Count > 0 ? reply.Where(r => r != reply.First()).Select(r => Localization.Get(r)).ToArray() : new string[0];
		_dialogueText.text = reply.Count > 0 ? Localization.GetAndFormat(reply.First(), false, replyExtras) : string.Empty;
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
		SUGARManager.GameData.Send("Meeting Question Directed At", _currentMember.Name);
		SUGARManager.GameData.Send("Meeting Question Asked", questionType);
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
			{ TrackerContextKey.FiringCost, ConfigKey.FiringCost.Value().ToString(CultureInfo.InvariantCulture) },
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
			foreach (var crewMember in UIManagement.CrewMemberUI)
			{
				if (crewMember.Current)
				{
					crewMember.transform.FindImage("Opinion").enabled = false;
				}
			}
			_lastReply = null;
		}
		UIManagement.PositionDisplay.ChangeBlockerOrder();
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
	/// When langauge is changed, redraw UI elements set in code
	/// </summary>
	private void OnLanguageChange()
	{
		var currentRole = _currentMember.BoatPosition();
		_textList[2].text = currentRole == Position.Null ? Localization.Get("NO_ROLE") : string.Empty;
		if (currentRole != Position.Null)
		{
			_roleButton.GetComponentInChildren<Text>().text = Localization.Get(currentRole.ToString());
		}
		_closeText.text = Localization.Get("MEETING_EARLY_EXIT");
		if (_lastReply != null)
		{
			_closeText.text = Localization.Get("MEETING_EXIT");
		}
		_statQuestion.text = "StatReveal".EventString();
		_roleQuestion.text = "RoleReveal".EventString();
		_opinionPositiveQuestion.text = "OpinionRevealPositive".EventString();
		_opinionNegativeQuestion.text = "OpinionRevealNegative".EventString();
		_dialogueText.text = _lastReply != null ? Localization.GetAndFormat(_lastReply.First(), false, _lastReply.Where(r => r != _lastReply.First()).ToArray()) : Localization.Get("MEETING_INTRO_" + _currentMember.GetSocialImportanceRating(GameManagement.ManagerName));
		DoBestFit();
	}

	/// <summary>
	/// Set up pointer enter and exit events for created objects that can be hovered over
	/// </summary>
	private void FeedbackHoverOver(string text = "")
	{
		_fireButton.transform.GetComponent<HoverObject>().SetHoverText(text);
	}

	private void DoBestFit()
	{
		_textList.BestFit();
		new Component[] { _dialogueText, _statQuestion, _roleQuestion, _opinionPositiveQuestion, _opinionNegativeQuestion, _closeText }.BestFit();
		_barForegrounds.Select(b => b.transform.Parent()).BestFit();
		_fireWarningPopUp.GetComponentsInChildren<Button>().ToList().BestFit();
	}
}