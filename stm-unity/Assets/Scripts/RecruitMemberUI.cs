using System;
using System.Linq;
using System.Reflection;

using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

using SUGAR.Unity;

using UnityEngine.UI;

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

	private bool _questionAsked;
	private string _currentSelected;

	private void Awake()
	{
		_recruitMember = GetComponent<RecruitMember>();
	}

	private void OnEnable()
	{
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "RecruitmentPopUpOpened", AlternativeTracker.Alternative.Menu));
		_questionAsked = false;
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "RecruitmentPopUpClosed", AlternativeTracker.Alternative.Menu));
		_popUpBlocker.gameObject.SetActive(false);
		Localization.LanguageChange -= OnLanguageChange;
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
		SetDialogueText(Localization.Get("RECRUITMENT_INTRO"));
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
			var lastName = splitName.Last() + ",\n";
			foreach (var split in splitName)
			{
				if (split != splitName.Last())
				{
					lastName += split + " ";
				}
			}
			lastName = lastName.Remove(lastName.Length - 1, 1);
			_recruitUI[i].transform.Find("Name").GetComponent<Text>().text = lastName;
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
			_recruitUI[i].transform.Find("Cost Image/Text").GetComponent<Text>().text = _recruitMember.GetConfigValue(ConfigKeys.RecruitmentCost).ToString();
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
			_questionButtons[i].transform.Find("Text").GetComponent<Text>().text = questionText;
			_questionButtons[i].transform.Find("Image/Text").GetComponent<Text>().text = _recruitMember.GetConfigValue(ConfigKeys.SendRecruitmentQuestionCost).ToString();
			_questionButtons[i].onClick.RemoveAllListeners();
			_questionButtons[i].onClick.AddListener(delegate { AskQuestion(selected, questionText); });
		}
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
		_questionAsked = true;
		SetDialogueText(questionText);
		var replies = _recruitMember.AskQuestion(skill);
		foreach (var recruit in _recruitUI)
		{
			var reply = replies.FirstOrDefault(r => r.Key.Name == recruit.name);
			if (reply.Key != null)
			{
				recruit.transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = reply.Value;
				recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().enabled = true;
				recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().sprite = _opinionSprites.FirstOrDefault(o => o.Name == reply.Value).Image;
				recruit.transform.Find("Image").GetComponentInChildren<AvatarDisplay>().UpdateMood(reply.Key.Avatar, reply.Value);
			}
		}
		CostCheck();
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, skill, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", skill + "Question", AlternativeTracker.Alternative.Question));
		SUGARManager.GameData.Send("Recruitment Question Asked", skill.ToString());
	}

	/// <summary>
	/// Set the previously asked question to be displayed
	/// </summary>
	private void SetDialogueText(string text)
	{
		_dialogueText.text = text;
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
		} else
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
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "Recruitment", "HireWarning", AlternativeTracker.Alternative.Menu));
	}

	/// <summary>
	/// Close the hire warning
	/// </summary>
	public void CloseHireCrewWarning()
	{
		_currentSelected = string.Empty;
		_hireWarningPopUp.SetActive(false);
		if (gameObject.activeSelf)
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
		SUGARManager.GameData.Send("Crew Member Hired", string.Empty);
	}

	private void OnLanguageChange()
	{
		if (!_questionAsked)
		{
			SetDialogueText(Localization.Get("RECRUITMENT_INTRO"));
		}
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
	}
}
