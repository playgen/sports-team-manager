using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

using PlayGen.SUGAR.Unity;

using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.BestFit;

using RAGE.Analytics.Formats;

/// <summary>
/// Contains all UI logic related to the Recruitment pop-up
/// </summary>
[RequireComponent(typeof(RecruitMember))]
public class RecruitMemberUI : ObservableMonoBehaviour
{
	private RecruitMember _recruitMember;
	[SerializeField]
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private GameObject[] _recruitUI;
	[SerializeField]
	private Button[] _questionButtons;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Text _hireWarningText;
	[SerializeField]
	private GameObject _hireWarningPopUp;
	[SerializeField]
	private Button _hireWarningAccept;
	[SerializeField]
	private GameObject _hireWarningReject;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;
	[SerializeField]
	private Icon[] _opinionSprites;

	private string _lastQuestion;
	private Dictionary<CrewMember, string> _lastAnswers;
	private string _currentSelected;

	private void Awake()
	{
		_recruitMember = GetComponent<RecruitMember>();
	}

	private void OnEnable()
	{
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "RecruitmentPopUpOpened", AlternativeTracker.Alternative.Menu));
		_lastQuestion = null;
		_lastAnswers = null;
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "RecruitmentPopUpClosed", AlternativeTracker.Alternative.Menu));
		_popUpBlocker.gameObject.SetActive(false);
		_popUpBlocker.transform.SetAsFirstSibling();
		transform.SetAsFirstSibling();
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	private void ResetDisplay()
	{
		//ActionAllowance display
		_allowanceBar.fillAmount = _recruitMember.QuestionAllowance() / (float)_recruitMember.StartingQuestionAllowance();
		_allowanceText.text = _recruitMember.QuestionAllowance().ToString();
		//set initial text displayed in center of pop-up
		SetDialogueText("RECRUITMENT_INTRO");
		//get recruits
		var recruits = _recruitMember.GetRecruits().OrderBy(r => Guid.NewGuid()).ToList();
		//for each recruitUI element
		for (var i = 0; i < _recruitUI.Length; i++)
		{
			//hide display if not needed
			if (recruits.Count <= i)
			{
				_recruitUI[i].SetActive(false);
				continue;
			}
			//make UI element active
			var thisRecruit = recruits[i];
			_recruitUI[i].SetActive(true);
			//set-up displayed name
			var splitName = thisRecruit.Name.Split(' ');
			var firstName =  ",\n" + splitName.First();
			var lastName = string.Empty;
			foreach (var split in splitName)
			{
				if (split != splitName.First())
				{
					lastName += split;
					if (split != splitName.Last())
					{
						lastName += " ";
					}
				}
			}
			var formattedName = lastName + firstName;
			_recruitUI[i].transform.Find("Name").GetComponent<Text>().text = formattedName;
			//set-up avatar for this recruit
			_recruitUI[i].transform.Find("Image").GetComponentInChildren<AvatarDisplay>().SetAvatar(thisRecruit.Avatar, 0);
			//flip direction they are facing for every other recruit
			if (i % 2 != 0)
			{
				_recruitUI[i].transform.Find("Image").localScale = new Vector3(-1, 1, 1);
			}
			//set-up button onclick handler
			_recruitUI[i].transform.Find("Button").GetComponent<Button>().interactable = true;
			_recruitUI[i].transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();
			_recruitUI[i].transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { HireCrewWarning(thisRecruit); });
			var rand = UnityEngine.Random.Range(0, 8);
			//set initial greeting dialogue
			_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = Localization.Get("RECRUIT_GREETING_" + (rand % 4));
			if (rand / 4 > 0)
			{
				_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text += Localization.Get("EXCLAIMATION_MARK");
			}
			_recruitUI[i].transform.Find("Dialogue Box/Image").GetComponent<Image>().enabled = false;
			_recruitUI[i].transform.Find("Cost Image/Text").GetComponent<Text>().text = _recruitMember.GetConfigValue(ConfigKeys.RecruitmentCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
			_recruitUI[i].name = recruits[i].Name;
		}
		//set-up question text and click handlers
		var skills = (CrewMemberSkill[])Enum.GetValues(typeof(CrewMemberSkill));
		for (var i = 0; i < _questionButtons.Length; i++)
		{
			if (skills.Length <= i)
			{
				_questionButtons[i].gameObject.SetActive(false);
				continue;
			}
			var selected = skills[i];
			_questionButtons[i].gameObject.SetActive(true);
			_questionButtons[i].interactable = true;
			var questionText = _recruitMember.GetQuestionText("Recruit" + selected).OrderBy(s => Guid.NewGuid()).First();
			_questionButtons[i].transform.Find("Text").GetComponent<Text>().text = Localization.Get(questionText);
			_questionButtons[i].transform.Find("Image/Text").GetComponent<Text>().text = _recruitMember.GetConfigValue(ConfigKeys.SendRecruitmentQuestionCost).ToString(Localization.SelectedLanguage.GetSpecificCulture());
			_questionButtons[i].onClick.RemoveAllListeners();
			_questionButtons[i].onClick.AddListener(delegate { AskQuestion(selected, questionText); });
		}
		DoBestFit();
		CostCheck();
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Update the allowance bar and disable all questions if they cost too much to ask
	/// </summary>
	private void CostCheck()
	{
		var allowance = _recruitMember.QuestionAllowance();
		_allowanceBar.fillAmount = allowance / (float)_recruitMember.StartingQuestionAllowance();
		_allowanceText.text = allowance.ToString();
		if (allowance < _recruitMember.GetConfigValue(ConfigKeys.SendRecruitmentQuestionCost))
		{
			_questionButtons.ToList().ForEach(qb => qb.interactable = false);
		}
	}

	/// <summary>
	/// Send a question to all recruits and get their replies in response
	/// </summary>
	public void AskQuestion(CrewMemberSkill skill, string questionText)
	{
		SetDialogueText(questionText);
		_lastQuestion = questionText;
		var replies = _recruitMember.AskQuestion(skill);
		_lastAnswers = replies;
		foreach (var recruit in _recruitUI)
		{
			var reply = replies.FirstOrDefault(r => r.Key.Name == recruit.name);
			if (reply.Key != null)
			{
				recruit.transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = Localization.Get(reply.Value);
				recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().enabled = true;
				var opinionSprite = _opinionSprites.FirstOrDefault(o => o.Name == reply.Value);
				if (opinionSprite != null)
				{
					recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().sprite = opinionSprite.Image;
				}
				recruit.transform.Find("Image").GetComponentInChildren<AvatarDisplay>().UpdateMood(reply.Key.Avatar, reply.Value);
			}
		}
		DoBestFit();
		CostCheck();
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, skill, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", skill + "Question", AlternativeTracker.Alternative.Question));
		SUGARManager.GameData.Send("Recruitment Question Asked", skill.ToString());
	}

	/// <summary>
	/// Set the previously asked question to be displayed
	/// </summary>
	private void SetDialogueText(string text)
	{
		_dialogueText.text = Localization.Get(text);
	}

	/// <summary>
	/// Display a warning before hiring a recruit
	/// </summary>
	public void HireCrewWarning(CrewMember recruit)
	{
		_currentSelected = recruit.Name;
		//pop-up and blocker reordering
		_hireWarningPopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_hireWarningPopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		//update of blocker click handling
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(CloseHireCrewWarning);
		//adjust text, button text and button positioning based on context
		if (_recruitMember.QuestionAllowance() < _recruitMember.GetConfigValue(ConfigKeys.RecruitmentCost))
		{
			_hireWarningText.text = Localization.Get("HIRE_WARNING_NOT_POSSIBLE");
			_hireWarningAccept.gameObject.SetActive(false);
			((RectTransform)_hireWarningReject.transform).anchorMin = new Vector2(0.375f, 0.1f);
			((RectTransform)_hireWarningReject.transform).anchorMax = new Vector2(0.625f, 0.35f);
			((RectTransform)_hireWarningReject.transform).anchoredPosition = Vector2.zero;
			_hireWarningReject.GetComponentInChildren<Text>().text = Localization.Get("OK", true);
		}
		else
		{
			_hireWarningAccept.onClick.RemoveAllListeners();
			_hireWarningAccept.onClick.AddListener(delegate { Recruit(recruit); });
			_hireWarningAccept.gameObject.SetActive(true);
			_hireWarningText.text = Localization.GetAndFormat("HIRE_WARNING_POSSIBLE", false, recruit.Name);
			((RectTransform)_hireWarningReject.transform).anchorMin = new Vector2(0.55f, 0.1f);
			((RectTransform)_hireWarningReject.transform).anchorMax = new Vector2(0.8f, 0.35f);
			((RectTransform)_hireWarningReject.transform).anchoredPosition = Vector2.zero;
			_hireWarningReject.GetComponentInChildren<Text>().text = Localization.Get("NO", true);
		}
		DoBestFit();
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "HireWarning", AlternativeTracker.Alternative.Menu));
	}

	/// <summary>
	/// Close the hire warning
	/// </summary>
	public void CloseHireCrewWarning()
	{
		_currentSelected = string.Empty;
		_hireWarningPopUp.SetActive(false);
		if (gameObject.activeInHierarchy)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "HireWarningClosed", AlternativeTracker.Alternative.Menu));
	}

	/// <summary>
	/// Hire the selected recruit onto the player's crew
	/// </summary>
	public void Recruit(CrewMember crewMember)
	{
		_recruitMember.Recruit(crewMember);
		_teamSelectionUI.ResetCrew();
		gameObject.SetActive(false);
		CloseHireCrewWarning();
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(GameObjectTracker).Name, "Interacted", "Recruitment", "HiredCrewMember", GameObjectTracker.TrackedGameObject.Npc));
		SUGARManager.GameData.Send("Crew Member Hired", true);
	}

	public void OnEscape()
	{
		if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
		{
			gameObject.SetActive(false);
		}
		else if (_hireWarningPopUp.activeInHierarchy)
		{
			CloseHireCrewWarning();
		}
	}

	private void OnLanguageChange()
	{
		SetDialogueText(_lastQuestion ?? "RECRUITMENT_INTRO");
		if (_recruitMember.QuestionAllowance() < _recruitMember.GetConfigValue(ConfigKeys.RecruitmentCost))
		{
			_hireWarningText.text = Localization.Get("HIRE_WARNING_NOT_POSSIBLE");
			_hireWarningReject.GetComponentInChildren<Text>().text = Localization.Get("OK", true);
		}
		else
		{
			_hireWarningText.text = Localization.GetAndFormat("HIRE_WARNING_POSSIBLE", false, _currentSelected);
			_hireWarningReject.GetComponentInChildren<Text>().text = Localization.Get("NO", true);
		}
		var skills = (CrewMemberSkill[])Enum.GetValues(typeof(CrewMemberSkill));
		for (var i = 0; i < _questionButtons.Length; i++)
		{
			var selected = skills[i];
			var questionText = _recruitMember.GetQuestionText("Recruit" + selected).OrderBy(s => Guid.NewGuid()).First();
			_questionButtons[i].transform.Find("Text").GetComponent<Text>().text = Localization.Get(questionText);
		}
		if (_lastAnswers != null)
		{
			foreach (var recruit in _recruitUI)
			{
				var reply = _lastAnswers.FirstOrDefault(r => r.Key.Name == recruit.name);
				if (reply.Key != null)
				{
					recruit.transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = Localization.Get(reply.Value);
					recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().enabled = true;
					var opinionSprite = _opinionSprites.FirstOrDefault(o => o.Name == reply.Value);
					if (opinionSprite != null)
					{
						recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().sprite = opinionSprite.Image;
					}
					recruit.transform.Find("Image").GetComponentInChildren<AvatarDisplay>().UpdateMood(reply.Key.Avatar, reply.Value);
				}
			}
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		_recruitUI.Select(r => r.transform.Find("Name").gameObject).BestFit();
		_recruitUI.Select(r => r.transform.Find("Dialogue Box/Dialogue").gameObject).BestFit(false);
		var questionList = _questionButtons.Select(q => q.gameObject).ToList();
		questionList.Add(transform.Find("Close").gameObject);
		questionList.BestFit();
		new[] { _hireWarningAccept.gameObject, _hireWarningReject.gameObject }.BestFit();
	}
}
