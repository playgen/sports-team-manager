using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.SUGAR.Unity;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

/// <summary>
/// Contains all UI logic related to the Recruitment pop-up
/// </summary>
public class RecruitMemberUI : MonoBehaviour
{
	[SerializeField]
	private Transform[] _recruitUI;
	[SerializeField]
	private List<Button> _questionButtons;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Text _hireWarningText;
	[SerializeField]
	private GameObject _hireWarningPopUp;
	[SerializeField]
	private Button _hireWarningAccept;
	[SerializeField]
	private Button _hireWarningReject;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;
	[SerializeField]
	private Icon[] _opinionSprites;

	private string _lastQuestion;
	private Dictionary<CrewMember, string> _lastAnswers;
	private string _currentSelected;

	private void OnEnable()
	{
		var history = GameManagement.ReverseLineUpHistory;
		var sessionsSinceLastChange = Mathf.Max(0, history.FindIndex(b => b.Type != GameManagement.BoatType));
		TrackerEventSender.SendEvent(new TraceEvent("RecruitmentPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.CurrentSession, GameManagement.CurrentSessionString },
			{ TrackerContextKey.SizeOfTeam, GameManagement.CrewCount },
			{ TrackerContextKey.SessionsSinceBoatLayoutChange, sessionsSinceLastChange }
		}, AccessibleTracker.Accessible.Screen));
		_lastQuestion = null;
		_lastAnswers = null;
		ResetDisplay();
		transform.EnableBlocker(() => CloseRecruitmentPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		UIManagement.DisableBlocker();
		UIManagement.Blocker.transform.SetAsFirstSibling();
		transform.SetAsFirstSibling();
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	private void ResetDisplay()
	{
		//get recruits
		var recruits = GameManagement.Team.Recruits.Values.OrderBy(r => Guid.NewGuid()).ToList();
		//for each recruitUI element
		for (var i = 0; i < _recruitUI.Length; i++)
		{
			//hide display if not needed
			if (recruits.Count <= i)
			{
				_recruitUI[i].gameObject.Active(false);
				continue;
			}
			//make UI element active
			var thisRecruit = recruits[i];
			_recruitUI[i].gameObject.Active(true);
			//set-up displayed name
			_recruitUI[i].FindText("Name").text = thisRecruit.SplitName();
			//set-up avatar for this recruit
			_recruitUI[i].GetComponentInChildren<AvatarDisplay>().SetAvatar(thisRecruit.Avatar, 0f);
			//flip direction they are facing for every other recruit
			if (i % 2 != 0)
			{
				_recruitUI[i].Find("Image").localScale = new Vector3(-1, 1, 1);
			}
			//set-up button onclick handler
			var button = _recruitUI[i].FindButton("Button");
			button.interactable = true;
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => HireCrewWarning(thisRecruit));
			var rand = UnityEngine.Random.Range(0, 8);
			//set initial greeting dialogue
			_recruitUI[i].FindText("Dialogue Box/Dialogue").text = Localization.Get("RECRUIT_GREETING_" + (rand % 4)) + (rand / 4 > 0 ? Localization.Get("EXCLAIMATION_MARK") : string.Empty);
			_recruitUI[i].FindImage("Dialogue Box/Image").enabled = false;
			_recruitUI[i].FindText("Cost Image/Text").text = ConfigKey.RecruitmentCost.ValueString();
			_recruitUI[i].name = recruits[i].Name;
		}
		OnLanguageChange();
		CostCheck();
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Update the allowance bar and disable all questions if they cost too much to ask
	/// </summary>
	private void CostCheck()
	{
		_allowanceBar.fillAmount = GameManagement.ActionAllowancePercentage;
		_allowanceText.text = GameManagement.ActionAllowance.ToString();
		_questionButtons.ForEach(qb => qb.transform.FindText("Image/Text").text = ConfigKey.SendRecruitmentQuestionCost.ValueString());
		_questionButtons.ForEach(qb => qb.interactable = ConfigKey.SendRecruitmentQuestionCost.Affordable());
	}

	/// <summary>
	/// Send a question to all recruits and get their replies in response
	/// </summary>
	public void AskQuestion(string skillName)
	{
		var skill = (Skill)Enum.Parse(typeof(Skill), skillName);
		_lastQuestion = skillName;
		var replies = GameManagement.GameManager.SendRecruitmentEvent(skill);
		_lastAnswers = replies;
		OnLanguageChange();
		CostCheck();
		TrackerEventSender.SendEvent(new TraceEvent("RecruitmentQuestionAsked", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.CurrentSession, GameManagement.CurrentSessionString },
			{ TrackerContextKey.QuestionAsked, skillName },
			{ TrackerContextKey.QuestionCost, ConfigKey.SendRecruitmentQuestionCost.ValueString(false) },
			{ TrackerContextKey.RaceStartTalkTime, GameManagement.StartingActionAllowance }
		}, skill.ToString(), AlternativeTracker.Alternative.Question));
		SUGARManager.GameData.Send("Recruitment Question Asked", skill.ToString());
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, skill.ToString());
	}

	/// <summary>
	/// Display a warning before hiring a recruit
	/// </summary>
	public void HireCrewWarning(CrewMember recruit)
	{
		_currentSelected = recruit.Name;
		//pop-up and blocker reordering
		_hireWarningPopUp.Active(true);
		_hireWarningPopUp.transform.EnableBlocker(() => CloseHireCrewWarning(TrackerTriggerSource.PopUpBlocker.ToString()));
		//adjust text, button text and button positioning based on context
		_hireWarningAccept.onClick.RemoveAllListeners();
		_hireWarningAccept.onClick.AddListener(() => Recruit(recruit, TrackerTriggerSource.YesButtonSelected.ToString()));
		_hireWarningAccept.gameObject.Active(ConfigKey.RecruitmentCost.Affordable());
		_hireWarningReject.RectTransform().anchorMin = ConfigKey.RecruitmentCost.Affordable() ? new Vector2(0.525f, 0.02f) : new Vector2(0.375f, 0.02f);
		_hireWarningReject.RectTransform().anchorMax = ConfigKey.RecruitmentCost.Affordable() ? new Vector2(0.85f, 0.27f) : new Vector2(0.625f, 0.2f);
		_hireWarningReject.RectTransform().anchoredPosition = Vector2.zero;
		_hireWarningReject.onClick.RemoveAllListeners();
		_hireWarningReject.onClick.AddListener(() => CloseHireCrewWarning(ConfigKey.RecruitmentCost.Affordable() ? TrackerTriggerSource.NoButtonSelected.ToString() : TrackerTriggerSource.OKButtonSelected.ToString()));
		OnLanguageChange();
		TrackerEventSender.SendEvent(new TraceEvent("HirePopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CrewMemberName, recruit.Name },
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.HiringCost, ConfigKey.RecruitmentCost.ValueString(false) }
		}, AccessibleTracker.Accessible.Screen));
	}

	/// <summary>
	/// Close the hire warning
	/// </summary>
	public void CloseHireCrewWarning(string source)
	{
		_hireWarningPopUp.Active(false);
		if (gameObject.activeInHierarchy)
		{
			transform.EnableBlocker(() => CloseRecruitmentPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
		}
		else
		{
			UIManagement.DisableBlocker();
		}
		TrackerEventSender.SendEvent(new TraceEvent("HirePopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CrewMemberName, _currentSelected },
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.HiringCost, ConfigKey.RecruitmentCost.ValueString(false) },
			{ TrackerContextKey.TriggerUI, source }
		}, AccessibleTracker.Accessible.Screen));
		_currentSelected = string.Empty;
	}

	/// <summary>
	/// Hire the selected recruit onto the player's crew
	/// </summary>
	public void Recruit(CrewMember crewMember, string source)
	{
		GameManagement.GameManager.AddRecruit(crewMember);
		UIManagement.TeamSelection.ResetCrew();
		CloseRecruitmentPopUp(string.Empty);
		CloseHireCrewWarning(string.Empty);
		TrackerEventSender.SendEvent(new TraceEvent("CrewMemberHired", TrackerAsset.Verb.Interacted, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CrewMemberName, crewMember.Name },
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.HiringCost, ConfigKey.RecruitmentCost.ValueString(false) },
			{ TrackerContextKey.TriggerUI, source }
		}, GameObjectTracker.TrackedGameObject.Npc));
		SUGARManager.GameData.Send("Crew Member Hired", true);
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Close this pop-up
	/// </summary>
	public void CloseRecruitmentPopUp(string source)
	{
		TrackerEventSender.SendEvent(new TraceEvent("RecruitmentPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
			{ TrackerContextKey.TriggerUI, source }
		}, AccessibleTracker.Accessible.Screen));
		UIManagement.MemberMeeting.Display();
		gameObject.Active(false);
	}

	/// <summary>
	/// Upon the escape key being pressed, close this pop-up or the hire warning pop-up if it's open
	/// </summary>
	public void OnEscape()
	{
		if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
		{
			CloseRecruitmentPopUp(TrackerTriggerSource.EscapeKey.ToString());
		}
		else if (_hireWarningPopUp.activeInHierarchy)
		{
			CloseHireCrewWarning(TrackerTriggerSource.EscapeKey.ToString());
		}
	}

	/// <summary>
	/// Upon language change redraw UI
	/// </summary>
	private void OnLanguageChange()
	{
		_dialogueText.text = _lastQuestion != null ? ("Recruit" + _lastQuestion).EventString() : Localization.Get("RECRUITMENT_INTRO");
		_hireWarningText.text = Localization.GetAndFormat(ConfigKey.RecruitmentCost.Affordable() ? "HIRE_WARNING_POSSIBLE" : "HIRE_WARNING_NOT_POSSIBLE", false, _currentSelected);
		_hireWarningReject.GetComponentInChildren<Text>().text = Localization.Get(ConfigKey.RecruitmentCost.Affordable() ? "NO" : "OK");
		_questionButtons.ForEach(b => b.transform.FindText("Text").text = ("Recruit" + b.name).EventString());
		if (_lastAnswers != null)
		{
			foreach (var recruit in _recruitUI)
			{
				var reply = _lastAnswers.FirstOrDefault(r => r.Key.Name == recruit.name);
				if (reply.Key != null)
				{
					recruit.transform.FindText("Dialogue Box/Dialogue").text = Localization.Get(reply.Value);
					recruit.transform.FindImage("Dialogue Box/Image").enabled = true;
					recruit.transform.FindImage("Dialogue Box/Image").sprite = _opinionSprites.FirstOrDefault(o => o.Name == reply.Value)?.Image;
					recruit.transform.FindComponentInChildren<AvatarDisplay>("Image").UpdateMood(reply.Key.Avatar, reply.Value);
				}
			}
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		_recruitUI.Select(r => r.transform.FindObject("Name")).BestFit(false);
		_recruitUI.Select(r => r.transform.FindObject("Dialogue Box/Dialogue")).BestFit(false);
		_questionButtons.Concat(new [] { transform.FindButton("Close") }).BestFit();
		new Component[] { _hireWarningAccept, _hireWarningReject }.BestFit();
	}
}