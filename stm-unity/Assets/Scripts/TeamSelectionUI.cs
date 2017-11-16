using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.BestFit;
using RAGE.Analytics.Formats;

/// <summary>
/// A class for grouping together a sprite with a name
/// </summary>
[Serializable]
public class Icon
{
	public string Name;
	public Sprite Image;
}

/// <summary>
/// Contains all UI logic related to the Team Management screen
/// </summary>
public class TeamSelectionUI : MonoBehaviour, IScrollHandler, IDragHandler {
	[SerializeField]
	private GameObject _boatContainer;
	[SerializeField]
	private GameObject _boatMain;
	[SerializeField]
	private List<GameObject> _boatPool;
	[SerializeField]
	private Scrollbar _boatContainerScroll;
	private int _previousScrollValue;
	[SerializeField]
	private GameObject _crewContainer;
	[SerializeField]
	private Dropdown _crewSort;
	[SerializeField]
	private Icon[] _mistakeIcons;
	[SerializeField]
	private Icon[] _roleIcons;
	public Icon[] RoleIcons
	{
		get { return _roleIcons; }
	}
	[SerializeField]
	public Icon[] _roleLogos;
	public Icon[] RoleLogos
	{
		get { return _roleLogos; }
	}
	[SerializeField]
	private GameObject _crewPrefab;
	[SerializeField]
	private GameObject _recruitPrefab;
	[SerializeField]
	private Button _raceButton;
	[SerializeField]
	private Button _skipToRaceButton;
	[SerializeField]
	private GameObject _endRace;
	[SerializeField]
	private Button _feedbackButton;
	[SerializeField]
	private GameObject _ongoingResultContainer;
	[SerializeField]
	private GameObject _resultContainer;
	[SerializeField]
	private GameObject _resultPrefab;
	[SerializeField]
	private Text _finalPlacementText;
	private readonly List<GameObject> _currentCrewButtons = new List<GameObject>();
	private readonly List<Button> _recruitButtons = new List<Button>();
	private int _positionsEmpty;
	[SerializeField]
	private Button _quitBlocker;
	[SerializeField]
	private Sprite _practiceIcon;
	[SerializeField]
	private Sprite _raceIcon;

	/// <summary>
	/// On enabling this object, ensure correct UI sections are displayed depending on progress
	/// </summary>
	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		if (!GameManagement.SeasonOngoing)
		{
			_feedbackButton.onClick.RemoveAllListeners();
			if (GameManagement.GameManager.QuestionnaireCompleted)
			{
				_feedbackButton.onClick.AddListener(TriggerFeedback);
				_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("FEEDBACK_BUTTON");
			}
			else
			{
				_feedbackButton.onClick.AddListener(TriggerQuestionnaire);
				_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("CONFLICT_QUESTIONNAIRE");
			}
			_endRace.gameObject.Active(true);
			_boatMain.gameObject.Active(false);
		}
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Create UI for the previous four line-ups and one for the next line-up
	/// </summary>
	private void Start()
	{
		UIManagement.PostRaceEvents.ToList().ForEach(e => e.Display());
		ResetScrollbar();
		CreateSeasonProgress();
		CreateNewBoat();
	}

	/// <summary>
	/// Toggle interactivity of race and recruitment buttons by if they are currently allowable 
	/// </summary>
	private void Update()
	{
		if (_recruitButtons.Count > 0)
		{
			if (ConfigKeys.RecruitmentCost.Affordable() && GameManagement.CrewEditAllowed && GameManagement.Team.CanAddToCrew())
			{
				if (!_recruitButtons[0].IsInteractable())
				{
					foreach (var b in _recruitButtons)
					{
						b.interactable = true;
						b.GetComponent<HoverObject>().Enabled = false;
					}
				}
			}
			else
			{
				if (_recruitButtons[0].IsInteractable())
				{
					foreach (var b in _recruitButtons)
					{
						b.interactable = false;
						if (!ConfigKeys.RecruitmentCost.Affordable())
						{
							FeedbackHoverOver(b.transform, "RECRUIT_BUTTON_HOVER_ALLOWANCE");
						}
						else if (!GameManagement.CrewEditAllowed)
						{
							FeedbackHoverOver(b.transform, Localization.GetAndFormat("RECRUIT_BUTTON_HOVER_LIMIT", false, GameManagement.StartingCrewEditAllowance));
						}
					}
				}
			}
		}
	}

	private void EnableRacing()
	{
		_raceButton.interactable = true;
		_skipToRaceButton.interactable = true;
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	private void DisableRacing()
	{
		_raceButton.interactable = false;
		_skipToRaceButton.interactable = false;
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Used to rearrange CrewMember names. shortName set to true results in first initial and last name, set to false results in last names, first names
	/// </summary>
	private string SplitName(string original, bool shortName = false)
	{
		var splitName = original.Split(' ');
		if (shortName)
		{
			var formattedName = splitName.First()[0] + ".";
			foreach (var split in splitName)
			{
				if (split != splitName.First())
				{
					formattedName += split;
					if (split != splitName.Last())
					{
						formattedName += " ";
					}
				}
			}
			return formattedName;
		}
		var firstName = ",\n" + splitName.First();
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
		return lastName + firstName;
	}

	private void CreateSeasonProgress()
	{
		foreach (Transform child in _ongoingResultContainer.transform)
		{
			Destroy(child.gameObject);
		}
		var raceHistory = GameManagement.Team.RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();
		for (var i = 0; i < GameManagement.GameManager.GetTotalRaceCount(); i++)
		{
			var resultObj = Instantiate(_resultPrefab, _ongoingResultContainer.transform, false);
			if (raceHistory.Count > i)
			{
				var position = GameManagement.GetRacePosition(raceHistory[i].Key, raceHistory[i].Value);
				resultObj.GetComponent<Image>().fillAmount = 1;
				resultObj.GetComponentInChildren<Text>().gameObject.AddComponent<TextLocalization>();
				resultObj.GetComponentInChildren<TextLocalization>().Key = "POSITION_" + position;
				resultObj.GetComponentInChildren<TextLocalization>().Set();
			}
			else
			{
				resultObj.GetComponent<Image>().fillAmount = raceHistory.Count == i ? (GameManagement.CurrentRaceSession - 1) / (float)GameManagement.RaceSessionLength : 0;
				resultObj.GetComponentInChildren<Text>().text = string.Empty;
			}
		}
	}

	/// <summary>
	/// Set up a new boat (aka, one used for positioning and racing)
	/// </summary>
	private void CreateNewBoat()
	{
		var stageIcon = _boatMain.transform.Find("Stage").GetComponent<Image>();
		if (GameManagement.SeasonOngoing)
		{
			stageIcon.sprite = GameManagement.IsRace ? _raceIcon : _practiceIcon;
			stageIcon.gameObject.Active(true);
			_endRace.gameObject.Active(false);
			_boatMain.gameObject.Active(true);
		}
		//set up buttons and race result UI if player has completed all races
		else
		{
			stageIcon.gameObject.Active(false);
			_feedbackButton.onClick.RemoveAllListeners();
			if (GameManagement.GameManager.QuestionnaireCompleted)
			{
				_feedbackButton.onClick.AddListener(TriggerFeedback);
				_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("FEEDBACK_BUTTON");
			}
			else
			{
				_feedbackButton.onClick.AddListener(TriggerQuestionnaire);
				_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("CONFLICT_QUESTIONNAIRE");
			}

			foreach (Transform child in _resultContainer.transform)
			{
				Destroy(child.gameObject);
			}
			foreach (var result in GameManagement.Team.RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList())
			{
				var position = GameManagement.GetRacePosition(result.Key, result.Value);
				var resultObj = Instantiate(_resultPrefab, _resultContainer.transform, false);
				resultObj.GetComponent<Image>().fillAmount = 0;
				resultObj.GetComponentInChildren<Text>().text = position.ToString();
			}
			_ongoingResultContainer.transform.parent.gameObject.Active(false);
			var finalPosition = GameManagement.GetCupPosition();
			_finalPlacementText.GetComponent<TextLocalization>().Key = "POSITION_" + finalPosition;
			_finalPlacementText.GetComponent<TextLocalization>().Set();

			_endRace.gameObject.Active(true);
			_boatMain.gameObject.Active(false);
		}
		_boatMain.transform.Find("Stage Number").GetComponent<Text>().text = GameManagement.IsRace || !GameManagement.SeasonOngoing ? string.Empty : GameManagement.CurrentRaceSession.ToString();
		_positionsEmpty = GameManagement.PositionCount;
		_raceButton.onClick.RemoveAllListeners();
		_skipToRaceButton.onClick.RemoveAllListeners();
		//add click handler to raceButton according to session taking place
		if (!GameManagement.IsRace)
		{
			_raceButton.onClick.AddListener(UIManagement.PreRace.ConfirmLineUpCheck);
			_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat("RACE_BUTTON_PRACTICE", true, GameManagement.CurrentRaceSession, GameManagement.RaceSessionLength - 1);
			_raceButton.GetComponentInChildren<Text>().fontSize = 16;
		}
		else
		{
			_raceButton.onClick.AddListener(UIManagement.PreRace.ConfirmPopUp);
			_raceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE", true);
			_raceButton.GetComponentInChildren<Text>().fontSize = 20;
		}
		_raceButton.onClick.AddListener(() => Invoke("QuickClickDisable", 0.5f));
		_skipToRaceButton.onClick.AddListener(UIManagement.PreRace.ConfirmPopUp);
		_skipToRaceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE", true);
		_skipToRaceButton.GetComponentInChildren<Text>().fontSize = 20;
		var positionContainer = _boatMain.transform.Find("Position Container");
		//set up position containers
		for (var i = 0; i < positionContainer.childCount; i++)
		{
			var positionObject = positionContainer.Find("Position " + i).gameObject;
			if (GameManagement.PositionCount <= i)
			{
				positionObject.Active(false);
				continue;
			}
			positionObject.Active(true);
			var pos = GameManagement.Positions[i];
			positionObject.transform.Find("Text").GetComponent<Text>().text = Localization.Get(pos.ToString());
			positionObject.transform.Find("Image").GetComponent<Image>().sprite = _roleLogos.First(mo => mo.Name == pos.ToString()).Image;
			positionObject.GetComponent<PositionUI>().SetUp(pos);
		}
		_raceButton.gameObject.Active(GameManagement.SeasonOngoing);
		_skipToRaceButton.gameObject.Active(false);
		if (GameManagement.SeasonOngoing && !GameManagement.IsRace && !GameManagement.ShowTutorial) {
			var previousSessions = GetLineUpHistory(0, GameManagement.RaceSessionLength);
			foreach (var session in previousSessions)
			{
				if (session.Value.Value == 0)
				{
					break;
				}
				if (Mathf.Approximately(session.Key.IdealMatchScore, session.Key.Positions.Count))
				{
					_skipToRaceButton.gameObject.Active(true);
					break;
				}
			}
		}
		DoBestFit();
		ResetCrew();
		RepeatLineUp();
	}

	/// <summary>
	/// Set up new crew objects for a new race session/upon resets for hiring/firing
	/// </summary>
	private void CreateCrew()
	{
		foreach (var cm in GameManagement.CrewMembers.Values.ToList())
		{
			//create the static CrewMember UI (aka, the one that remains greyed out within the container at all times)
			var crewMember = CreateCrewMember(cm, _crewContainer.transform, false, true);
			//set anchoring, pivot and anchoredPosition so that they are positioned correctly within the container at the bottom of the screen
			((RectTransform)crewMember.transform).anchorMin = new Vector2(0.5f, 0.5f);
			((RectTransform)crewMember.transform).anchorMax = new Vector2(0.5f, 0.5f);
			((RectTransform)crewMember.transform).pivot = new Vector2(0, 0.5f);
			((RectTransform)crewMember.transform).anchoredPosition = Vector2.zero;
			_currentCrewButtons.Add(crewMember);
			//create the draggable copy of the above
			if (GameManagement.SeasonOngoing)
			{
				var crewMemberDraggable = CreateCrewMember(cm, crewMember.transform, true, true);
				crewMemberDraggable.transform.position = crewMember.transform.position;
				_currentCrewButtons.Add(crewMemberDraggable);
			}
		}
		//create a recruitment UI object for each empty spot in the crew
		for (var i = 0; i < GameManagement.Team.CrewLimitLeft(); i++)
		{
			var recruit = Instantiate(_recruitPrefab);
			recruit.transform.SetParent(_crewContainer.transform, false);
			recruit.name = "Recruit";
			recruit.GetComponent<Button>().onClick.AddListener(() => UIManagement.Recruitment.gameObject.Active(true));
			_recruitButtons.Add(recruit.GetComponent<Button>());
		}
		SortCrew();
	}

	/// <summary>
	/// Create a new CrewMember object
	/// </summary>
	private GameObject CreateCrewMember(CrewMember cm, Transform parent, bool usable, bool current)
	{
		var crewMember = Instantiate(_crewPrefab);
		crewMember.transform.SetParent(parent, false);
		crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(cm.Name, true);
		crewMember.name = SplitName(cm.Name);
		crewMember.GetComponent<CrewMemberUI>().SetUp(usable, current, cm, parent);
		crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(cm.Avatar, cm.GetMood(), true);
		return crewMember;
	}

	public void SortCrew()
	{
		var sortedCrewMembers = _crewContainer.GetComponentsInChildren<CrewMemberUI>().ToList();
		switch (_crewSort.value)
		{
			case 0:
				sortedCrewMembers = sortedCrewMembers.OrderBy(c => c.name).ToList();
				break;
			case 1:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[CrewMemberSkill.Charisma]).ToList();
				break;
			case 2:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[CrewMemberSkill.Perception]).ToList();
				break;
			case 3:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[CrewMemberSkill.Quickness]).ToList();
				break;
			case 4:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[CrewMemberSkill.Body]).ToList();
				break;
			case 5:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[CrewMemberSkill.Willpower]).ToList();
				break;
			case 6:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[CrewMemberSkill.Wisdom]).ToList();
				break;
			case 7:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.GetMood()).ToList();
				break;
			case 8:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedCrewOpinions.Values.Sum()/ (float)c.CrewMember.RevealedCrewOpinions.Count).ToList();
				break;
			case 9:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedCrewOpinions[GameManagement.Manager.Name.NoSpaces()]).ToList();
				break;
			case 10:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => GameManagement.LineUpHistory.Count(boat => boat.PositionCrew.Values.Any(cm => c.CrewMember.Name == cm.Name))).ToList();
				break;
			case 11:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => GameManagement.LineUpHistory.Count(boat => boat.PositionCrew.Values.Any(cm => c.CrewMember.Name == cm.Name) && GameManagement.GetRacePosition(boat.Score, boat.Positions.Count) == 1)).ToList();
				break;
			case 12:
				sortedCrewMembers = sortedCrewMembers.OrderBy(c => c.CrewMember.Age).ToList();
				break;
			case 13:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.Avatar.Height * (c.CrewMember.Avatar.IsMale ? 1 : 0.935f)).ToList();
				break;
		}
		var sortedCrew = sortedCrewMembers.Select(c => c.transform).ToList();
		foreach (var recruit in _recruitButtons)
		{
			sortedCrew.Add(recruit.transform);
		}
		foreach (var crewMember in sortedCrew)
		{
			crewMember.SetAsLastSibling();
		}
	}

	/// <summary>
	/// Get the history of results, taking and skipping the amount given
	/// </summary>
	public List<KeyValuePair<Boat, KeyValuePair<int, int>>> GetLineUpHistory(int skipAmount, int takeAmount)
	{
		var boats = GameManagement.LineUpHistory.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var offsets = GameManagement.Team.HistoricTimeOffset.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var sessions = GameManagement.Team.HistoricSessionNumber.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var boatOffsets = new List<KeyValuePair<Boat, KeyValuePair<int, int>>>();
		for (var i = 0; i < boats.Count; i++)
		{
			if (i < offsets.Count)
			{
				boatOffsets.Add(new KeyValuePair<Boat, KeyValuePair<int, int>>(boats[i], new KeyValuePair<int, int>(offsets[i], sessions[i])));
			}
		}
		return boatOffsets;
	}

	/// <summary>
	/// Instantiate and position UI for an existing boat (aka, line-up already selected in the past)
	/// </summary>
	private void CreateHistoricalBoat(GameObject oldBoat, Boat boat, int offset, int stageNumber)
	{
		oldBoat.Active(true);
		var stageIcon = oldBoat.transform.Find("Stage").GetComponent<Image>();
		var isRace = stageNumber == 0;
		stageIcon.sprite = isRace ? _raceIcon : _practiceIcon;
		oldBoat.transform.Find("Stage Number").GetComponent<Text>().text = isRace ? string.Empty : stageNumber.ToString();
		var idealScore = boat.IdealMatchScore;
		//get selection mistakes for this line-up and set-up feedback UI
		var mistakeList = boat.GetAssignmentMistakes(3);
		if (GameManagement.ShowTutorial && GameManagement.LineUpHistory.Count == 1)
		{
			mistakeList = _mistakeIcons.Select(m => m.Name).Where(m => m != "Correct" && m != "Hidden").OrderBy(m => Guid.NewGuid()).Take(2).ToList();
			mistakeList.Add("Hidden");
		}
		SetMistakeIcons(mistakeList, oldBoat, idealScore, boat.Positions.Count);
		var scoreDiff = GetResult(isRace, boat, offset, oldBoat.transform.Find("Score").GetComponent<Text>());
		var crewContainer = oldBoat.transform.Find("Crew Container");
		var crewCount = 0;
		//for each position, create a new CrewMember UI object and place accordingly
		foreach (var pair in boat.PositionCrew)
		{
			//create CrewMember UI object for the CrewMember that was in this position
			var crewMember = crewContainer.Find("Crew Member " + crewCount).gameObject;
			crewMember.Active(true);
			var current = GameManagement.CrewMembers.ContainsKey(pair.Value.Name);
			crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(pair.Value.Name, true);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(pair.Value.Avatar, scoreDiff * (2f / boat.Positions.Count) + 3, true);
			crewMember.GetComponent<CrewMemberUI>().SetUp(false, current, pair.Value, crewContainer);

			var positionImage = crewMember.transform.Find("Position").gameObject;
			//update current position button
			positionImage.GetComponent<Image>().enabled = true;
			positionImage.GetComponent<Image>().sprite = _roleIcons.First(mo => mo.Name == pair.Key.ToString()).Image;
			positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
			var currentPosition = pair.Key;
			positionImage.GetComponent<Button>().onClick.AddListener(() => UIManagement.PositionDisplay.SetUpDisplay(currentPosition, TrackerTriggerSources.TeamManagementScreen.ToString()));
			crewCount++;
		}
		for (var i = crewCount; i < crewContainer.childCount; i++)
		{
			var crewMember = crewContainer.Find("Crew Member " + i).gameObject;
			crewMember.Active(false);
		}
		DoBestFit();
	}

	/// <summary>
	/// Create mistake icons and set values for each feedback 'light'
	/// </summary>
	private void SetMistakeIcons(List<string> mistakes, GameObject boat, float idealScore, int positionCount)
	{
		var mistakeParent = boat.transform.Find("Icon Container");
		for (var i = 0; i < mistakeParent.childCount; i++)
		{
			var mistakeObject = mistakeParent.Find("Ideal Icon " + i).gameObject;
			if (mistakes.Count <= i || string.IsNullOrEmpty(mistakes[i]))
			{
				mistakeObject.Active(false);
				continue;
			}
			mistakeObject.Active(true);
			//set image based on mistake name
			var mistakeIcon = _mistakeIcons.First(mo => mo.Name == mistakes[i]).Image;
			mistakeObject.GetComponent<Image>().sprite = mistakeIcon;
			mistakeObject.GetComponent<Image>().color = mistakes[i] != "Hidden" ? new UnityEngine.Color((i + 1) * 0.33f, (i + 1) * 0.33f, 0.875f + (i * 0.125f)) : UnityEngine.Color.white;
			//add spaces between words where needed
			FeedbackHoverOver(mistakeObject.transform, Regex.Replace(mistakes[i], "([a-z])([A-Z])", "$1_$2") + "_FEEDBACK");
		}
		//set numbers for each 'light'
		var unideal = positionCount - (int)idealScore - ((idealScore % 1) * 10);
		boat.transform.Find("Light Container").gameObject.Active(true);
		boat.transform.Find("Light Container/Green").GetComponentInChildren<Text>().text = ((int)idealScore).ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Green"), "GREEN_PLACEMENT");
		boat.transform.Find("Light Container/Yellow").GetComponentInChildren<Text>().text = Mathf.RoundToInt(((idealScore % 1) * 10)).ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Yellow"), "YELLOW_PLACEMENT");
		boat.transform.Find("Light Container/Red").GetComponentInChildren<Text>().text = Mathf.RoundToInt(unideal).ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Red"), "RED_PLACEMENT");
	}

	/// <summary>
	/// Set up pointer enter and exit events for created objects that can be hovered over
	/// </summary>
	private void FeedbackHoverOver(Transform feedback, string text)
	{
		feedback.GetComponent<HoverObject>().Enabled = true;
		feedback.GetComponent<HoverObject>().SetHoverText(text);
	}

	/// <summary>
	/// Adjust the number of positions on the currentBoat that has not been given a CrewMember
	/// </summary>
	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
		//enable race button if all positions are filled and QuickClickDisable isn't being invoked
		if ((_positionsEmpty > 0 && _raceButton.interactable) || IsInvoking("QuickClickDisable"))
		{
			DisableRacing();
		}
		else if (_positionsEmpty == 0 && !_raceButton.interactable)
		{
			EnableRacing();
		}
	}

	/// <summary>
	/// Use Unity Event System OnScroll to change displayed results
	/// </summary>
	public void OnScroll(PointerEventData eventData)
	{
		if (!UIManagement.SmallBlocker.gameObject.activeInHierarchy && !UIManagement.Blocker.gameObject.activeInHierarchy && !_quitBlocker.gameObject.activeInHierarchy)
		{
			_boatContainerScroll.value += eventData.scrollDelta.y * 0.55f * _boatContainerScroll.size;
		}
	}

	/// <summary>
	/// Use Unity Event System OnDrag to change displayed results
	/// </summary>
	public void OnDrag(PointerEventData eventData)
	{
		if (!UIManagement.SmallBlocker.gameObject.activeInHierarchy && !UIManagement.Blocker.gameObject.activeInHierarchy && !_quitBlocker.gameObject.activeInHierarchy)
		{
			_boatContainerScroll.value -= Mathf.Clamp(eventData.delta.y * 0.1f, -1, 1) * _boatContainerScroll.size;
		}
	}

	/// <summary>
	/// Reset the scrollbar position
	/// </summary>
	public void ResetScrollbar()
	{
		_boatContainerScroll.numberOfSteps = GameManagement.LineUpHistory.Count - 3;
		_boatContainerScroll.size = 1f / _boatContainerScroll.numberOfSteps;
		_boatContainerScroll.value = 0;
		_previousScrollValue = 1;
		_boatContainerScroll.gameObject.Active(_boatContainerScroll.numberOfSteps >= 2);
		ChangeVisibleBoats();
	}

	/// <summary>
	/// Redraw the displayed historical results
	/// </summary>
	public void ChangeVisibleBoats(bool forceOverwrite = false)
	{
		if (!forceOverwrite && _previousScrollValue == Mathf.RoundToInt(_boatContainerScroll.value * (_boatContainerScroll.numberOfSteps - 1)))
		{
			return;
		}
		var skipAmount = _boatContainerScroll.size > 0 ? Mathf.RoundToInt(_boatContainerScroll.value * (_boatContainerScroll.numberOfSteps - 1)) : 0;
		var setUpCount = 0;
		foreach (var boat in GetLineUpHistory(skipAmount, 4))
		{
			var boatObject = _boatPool[setUpCount];
			boatObject.Active(true);
			CreateHistoricalBoat(boatObject, boat.Key, boat.Value.Key, boat.Value.Value);
			setUpCount++;
		}
		for (var i = setUpCount; i < _boatPool.Count; i++)
		{
			_boatPool[i].Active(false);
		}
		_previousScrollValue = skipAmount;
	}

	/// <summary>
	///  Skips all remaining practice sessions (if any)
	/// </summary>
	public void SkipToRace()
	{
		GameManagement.GameManager.SkipToRace();
	}

	/// <summary>
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUp()
	{
		//select random time offset
		var offset = UnityEngine.Random.Range(0, 10);
		//confirm the line-up with the simulation 
		SUGARManager.GameData.Send("Current Crew Size", GameManagement.CrewCount);
		GameManagement.GameManager.SaveLineUp(offset);
		ResetScrollbar();
		GetResult(GameManagement.CurrentRaceSession - 1 == 0, GameManagement.LineUpHistory.Last(), offset, _raceButton.GetComponentInChildren<Text>(), true);
		//set-up next boat
		CreateNewBoat();
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Repeat the line-up (or what can be repeated) from what is passed to it
	/// </summary>
	private void RepeatLineUp()
	{
		DisableRacing();
		//get currently positioned
		var currentPositions = new Dictionary<Position, CrewMember>();
		foreach (var pair in GameManagement.PositionCrew)
		{
			currentPositions.Add(pair.Key, pair.Value);
		}
		//get current CrewMember and Position objects
		var crewMembers = _crewContainer.GetComponentsInChildren<CrewMemberUI>().Where(cm => cm.Current && cm.Usable).ToList();
		var positions = _boatMain.GetComponentsInChildren<PositionUI>().OrderBy(p => p.transform.GetSiblingIndex()).ToList();
		foreach (var position in positions)
		{
			//don't try and position where nobody is positioned
			if (!currentPositions.ContainsKey(position.Position))
			{
				continue;
			}
			//get positioned CrewMember
			var member = currentPositions[position.Position];
			foreach (var crewMember in crewMembers)
			{
				//if this UI is for the positioned CrewMember, place and remove the CrewMemberUI and Position from their lists to remove their availability
				if (crewMember.name == SplitName(member.Name))
				{
					position.RemoveCrew();
					crewMember.Place(position.gameObject);
					crewMembers.Remove(crewMember);
					currentPositions.Remove(position.Position);
					break;
				}
			}
		}
		if (_positionsEmpty == 0)
		{
			EnableRacing();
		}
	}

	/// <summary>
	/// Destroy the currently created CrewMember object and adjust any older CrewMember objects where the member has retired/been fired
	/// </summary>
	public void ResetCrew()
	{
		//remove attachment between CrewMemberUI and PositionUI
		foreach (var position in (PositionUI[])FindObjectsOfType(typeof(PositionUI)))
		{
			position.RemoveCrew();
		}
		//destroy recruitment buttons
		foreach (var b in _currentCrewButtons)
		{
			Destroy(b);
		}
		foreach (var crewMember in UIManagement.CrewMemberUI)
		{
			//destroy CrewMemberUI (making them unclickable) from those that are no longer in the currentCrew. Update avatar so they change into their causal outfit
			if (GameManagement.CrewMembers.All(cm => cm.Key != crewMember.CrewMember.Name))
			{
				crewMember.GetComponentInChildren<AvatarDisplay>().UpdateAvatar(crewMember.CrewMember.Avatar, true);
				crewMember.NotCurrent();
			}
		}
		//destroy recruitment buttons
		foreach (var b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_currentCrewButtons.Clear();
		_recruitButtons.Clear();
		//reset empty positions
		_positionsEmpty = GameManagement.PositionCount;
		//recreate crew and repeat previous line-up
		CreateCrew();
		RepeatLineUp();
		//close any open pop-ups if needed
		UIManagement.MemberMeeting.CloseCrewMemberPopUp(string.Empty);
		UIManagement.PositionDisplay.ClosePositionPopUp(string.Empty);
		DoBestFit();
	}

	/// <summary>
	/// Get and display the result of the previous race session
	/// </summary>
	private float GetResult(bool isRace, Boat boat, int offset, Text scoreText, bool current = false)
	{
		var timeTaken = TimeSpan.FromSeconds(1800 - ((boat.Score - 22) * 10) + offset);
		var finishPosition = 1;
		var scoreDiff = boat.Score - GameManagement.GetExpectedScore(boat.Positions.Count);
		if (!isRace)
		{
			scoreText.text = string.Format("{0:D2}:{1:D2}", timeTaken.Minutes, timeTaken.Seconds);
			if (current)
			{
				UpdateSeasonProgress();
			}
		}
		else
		{
			finishPosition = GameManagement.GetRacePosition(boat.Score, boat.Positions.Count);
			var finishPositionText = Localization.Get("POSITION_" + finishPosition);
			scoreText.text = string.Format("{0} {1}", Localization.Get("RACE_POSITION"), finishPositionText);
			if (current)
			{
				UpdateSeasonProgress(finishPosition);
				UIManagement.RaceResult.Display(boat.PositionCrew, finishPosition, finishPositionText.ToLower());
			}
		}
		if (current)
		{
			var newString = string.Join(",", boat.Positions.Select(pos => pos.ToString()).ToArray());
			TrackerEventSender.SendEvent(new TraceEvent("RaceResult", TrackerVerbs.Completed, new Dictionary<string, string>
			{
				{ TrackerContextKeys.RaceNumber.ToString(), (GameManagement.Team.RaceHistory.Count + 1).ToString() },
				{ TrackerContextKeys.CurrentSession.ToString(), (isRace ? GameManagement.RaceSessionLength : GameManagement.CurrentRaceSession - 1) + "/" + GameManagement.RaceSessionLength },
				{ TrackerContextKeys.SessionType.ToString(), isRace ? "Race" : "Practice" },
				{ TrackerContextKeys.BoatLayout.ToString(), newString },
				{ TrackerContextKeys.Score.ToString(), boat.Score.ToString() },
				{ TrackerContextKeys.ScoreAverage.ToString(), ((float)boat.Score / boat.Positions.Count).ToString(CultureInfo.InvariantCulture) },
				{ TrackerContextKeys.IdealCorrectPlacement.ToString(), ((int)boat.IdealMatchScore).ToString() },
				{ TrackerContextKeys.IdealCorrectMemberWrongPosition.ToString(), Mathf.RoundToInt(((boat.IdealMatchScore % 1) * 10)).ToString() },
				{ TrackerContextKeys.IdealIncorrectPlacement.ToString(), Mathf.RoundToInt(boat.Positions.Count - (int)boat.IdealMatchScore - ((boat.IdealMatchScore % 1) * 10)).ToString() }
			}, CompletableTracker.Completable.Race));

			SUGARManager.GameData.Send("Race Session Score", boat.Score);
			SUGARManager.GameData.Send("Current Boat Size", boat.Positions.Count);
			SUGARManager.GameData.Send("Race Session Score Average", (float)boat.Score / boat.Positions.Count);
			SUGARManager.GameData.Send("Race Session Ideal Score Average", boat.IdealMatchScore / boat.Positions.Count);
			SUGARManager.GameData.Send("Race Time", (long)timeTaken.TotalSeconds);
			SUGARManager.GameData.Send("Post Race Crew Average Mood", GameManagement.Team.AverageTeamMood());
			SUGARManager.GameData.Send("Post Race Crew Average Manager Opinion", GameManagement.Team.AverageTeamManagerOpinion());
			SUGARManager.GameData.Send("Post Race Crew Average Opinion", GameManagement.Team.AverageTeamOpinion());
			SUGARManager.GameData.Send("Post Race Boat Average Mood", GameManagement.Boat.AverageBoatMood());
			SUGARManager.GameData.Send("Post Race Boat Average Manager Opinion", GameManagement.Boat.AverageBoatManagerOpinion(GameManagement.Manager.Name));
			SUGARManager.GameData.Send("Post Race Boat Average Opinion", GameManagement.Boat.AverageBoatOpinion());
			foreach (var feedback in boat.SelectionMistakes)
			{
				SUGARManager.GameData.Send("Race Session Feedback", feedback);
			}
			if (isRace)
			{
				SUGARManager.GameData.Send("Race Position", finishPosition);
				SUGARManager.GameData.Send("Time Remaining", GameManagement.ActionAllowance);
				SUGARManager.GameData.Send("Time Taken", GameManagement.StartingActionAllowance - GameManagement.ActionAllowance);
			}
		}
		return scoreDiff;
	}

	private void UpdateSeasonProgress(int result = 0)
	{
		if (result > 0)
		{
			_ongoingResultContainer.transform.GetChild(GameManagement.Team.RaceHistory.Count - 1).GetComponent<Image>().fillAmount = 1;
			_ongoingResultContainer.transform.GetChild(GameManagement.Team.RaceHistory.Count - 1).GetComponentInChildren<Text>().gameObject.AddComponent<TextLocalization>();
			_ongoingResultContainer.transform.GetChild(GameManagement.Team.RaceHistory.Count - 1).GetComponentInChildren<TextLocalization>().Key = "POSITION_" + result;
			_ongoingResultContainer.transform.GetChild(GameManagement.Team.RaceHistory.Count - 1).GetComponentInChildren<TextLocalization>().Set();
		}
		else
		{
			_ongoingResultContainer.transform.GetChild(GameManagement.Team.RaceHistory.Count).GetComponent<Image>().fillAmount = (GameManagement.CurrentRaceSession - 1) / (float)GameManagement.RaceSessionLength;
		}
	}

	/// <summary>
	/// On escape, trigger logic which depends on the pop-up being displayed
	/// </summary>
	public void OnEscape()
	{
		if (_quitBlocker.gameObject.activeInHierarchy)
		{
			_quitBlocker.onClick.Invoke();
		}
		else if (!SUGARManager.Unity.AnyActiveUI)
		{
			FindObjectOfType<ScreenSideUI>().DisplayQuitWarning();
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		foreach (var position in _boatMain.GetComponentsInChildren<PositionUI>()) {
			position.transform.Find("Text").GetComponent<Text>().text = Localization.Get(position.Position.ToString());
		}
		_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat(GameManagement.IsRace ? "RACE_BUTTON_RACE" : "RACE_BUTTON_PRACTICE", true, GameManagement.CurrentRaceSession, GameManagement.RaceSessionLength - 1);
		_skipToRaceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE", true);
		ChangeVisibleBoats(true);
		if (_endRace.gameObject.activeSelf)
		{
			_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get(GameManagement.GameManager.QuestionnaireCompleted ? "FEEDBACK_BUTTON" : "CONFLICT_QUESTIONNAIRE");
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		_boatMain.transform.Find("Position Container").gameObject.BestFit();
		_ongoingResultContainer.transform.parent.BestFit();
		UIManagement.CrewMemberUI.Select(c => c.gameObject).BestFit();
		var currentPosition = _boatContainer.transform.localPosition.y - ((RectTransform)_boatContainer.transform).anchoredPosition.y;
		if (!Mathf.Approximately(_boatMain.GetComponent<LayoutElement>().preferredHeight, Mathf.Abs(currentPosition) * 0.2f))
		{
			foreach (Transform boat in _boatContainer.transform)
			{
				boat.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(currentPosition) * 0.2f;
			}
		}
		_crewSort.transform.BestFit();
	}

	/// <summary>
	/// Go to questionnaire state
	/// </summary>
	private void TriggerQuestionnaire()
	{
		UIStateManager.StaticGoToQuestionnaire();
	}

	/// <summary>
	/// Go to feedback state
	/// </summary>
	private void TriggerFeedback()
	{
		UIStateManager.StaticGoToFeedback();
	}
}