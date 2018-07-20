using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Loading;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

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
public class TeamSelectionUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _boatContainer;
	[SerializeField]
	private GameObject[] _boatPagingButtons;
	[SerializeField]
	private GameObject _boatMain;
	[SerializeField]
	private List<GameObject> _boatPool;
	private int _previousScrollValue;
	[SerializeField]
	private GameObject _crewContainer;
	public GameObject CrewContainer => _crewContainer;
	[SerializeField]
	private GameObject[] _crewPagingButtons;
	[SerializeField]
	private Dropdown _crewSort;
	[SerializeField]
	private Icon[] _mistakeIcons;
	[SerializeField]
	private Icon[] _roleIcons;
	public Dictionary<string, Sprite> RoleIcons => _roleIcons.ToDictionary(r => r.Name, r => r.Image);
	[SerializeField]
	private Icon[] _roleLogos;
	public Dictionary<string, Sprite> RoleLogos => _roleLogos.ToDictionary(r => r.Name, r => r.Image);
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
			if (!GameManagement.RageMode)
			{
				_feedbackButton.onClick.RemoveAllListeners();
				if (!GameManagement.QuestionnaireCompleted)
				{
					_feedbackButton.onClick.AddListener(TriggerFeedback);
					_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("FEEDBACK_BUTTON");
				}
				else
				{
					_feedbackButton.onClick.AddListener(TriggerQuestionnaire);
					_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("CONFLICT_QUESTIONNAIRE");
				}
			}
			else
			{
				_feedbackButton.gameObject.Active(false);
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
			if (ConfigKey.RecruitmentCost.Affordable() && GameManagement.CrewEditAllowed && GameManagement.CanAddToCrew)
			{
				if (!_recruitButtons[0].IsInteractable())
				{
					foreach (var b in _recruitButtons)
					{
						b.interactable = true;
						FeedbackHoverOver(b.transform);
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
						if (!ConfigKey.RecruitmentCost.Affordable())
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
	private string SplitName(CrewMember member, bool shortName = false)
	{
		return shortName ? member.FirstName[0] + "." + member.LastName : member.LastName + ",\n" + member.FirstName;
	}

	private void CreateSeasonProgress()
	{
		foreach (Transform child in _ongoingResultContainer.transform)
		{
			Destroy(child.gameObject);
		}
		for (var i = 0; i < GameManagement.GameManager.GetTotalRaceCount(); i++)
		{
			var resultObj = Instantiate(_resultPrefab, _ongoingResultContainer.transform, false);
			resultObj.GetComponentInChildren<Text>().color = UnityEngine.Color.black;
			if (GameManagement.RaceCount > i)
			{
				var position = GameManagement.GetRacePosition(GameManagement.RaceScorePositionCountPairs[i].Key, GameManagement.RaceScorePositionCountPairs[i].Value);
				resultObj.GetComponent<Image>().fillAmount = 1;
				resultObj.GetComponentInChildren<Text>().gameObject.AddComponent<TextLocalization>();
				resultObj.GetComponentInChildren<TextLocalization>().Key = "POSITION_" + position;
				resultObj.GetComponentInChildren<TextLocalization>().Set();
			}
			else
			{
				resultObj.GetComponent<Image>().fillAmount = GameManagement.RaceCount == i ? (GameManagement.CurrentRaceSession - 1) / (float)GameManagement.RaceSessionLength : 0;
				resultObj.GetComponentInChildren<Text>().text = string.Empty;
			}
		}
	}

	/// <summary>
	/// Set up a new boat (aka, one used for positioning and racing)
	/// </summary>
	private void CreateNewBoat()
	{
		var stageIcon = _boatMain.transform.FindImage("Stage");
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
			if (!GameManagement.RageMode)
			{
				_feedbackButton.onClick.RemoveAllListeners();
				if (!GameManagement.QuestionnaireCompleted)
				{
					_feedbackButton.onClick.AddListener(TriggerFeedback);
					_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("FEEDBACK_BUTTON");
				}
				else
				{
					_feedbackButton.onClick.AddListener(TriggerQuestionnaire);
					_feedbackButton.GetComponentInChildren<Text>().text = Localization.Get("CONFLICT_QUESTIONNAIRE");
				}
			}
			foreach (Transform child in _resultContainer.transform)
			{
				Destroy(child.gameObject);
			}
			foreach (var result in GameManagement.RaceScorePositionCountPairs)
			{
				var position = GameManagement.GetRacePosition(result.Key, result.Value);
				var resultObj = Instantiate(_resultPrefab, _resultContainer.transform, false);
				resultObj.GetComponent<Image>().fillAmount = 0;
				resultObj.GetComponentInChildren<Text>().text = position.ToString();
			}
			_ongoingResultContainer.Parent().Active(false);
			var finalPosition = GameManagement.GetCupPosition();
			_finalPlacementText.GetComponent<TextLocalization>().Key = "POSITION_" + finalPosition;
			_finalPlacementText.GetComponent<TextLocalization>().Set();

			_endRace.gameObject.Active(true);
			_feedbackButton.gameObject.Active(!GameManagement.RageMode);
			_boatMain.gameObject.Active(false);
		}
		_boatMain.transform.FindText("Stage Number").text = GameManagement.IsRace || !GameManagement.SeasonOngoing ? string.Empty : GameManagement.CurrentRaceSession.ToString();
		_positionsEmpty = GameManagement.PositionCount;
		_raceButton.onClick.RemoveAllListeners();
		_skipToRaceButton.onClick.RemoveAllListeners();
		//add click handler to raceButton according to session taking place
		if (!GameManagement.IsRace)
		{
			_raceButton.onClick.AddListener(UIManagement.PreRace.ConfirmLineUpCheck);
			_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat("RACE_BUTTON_PRACTICE", false, GameManagement.CurrentRaceSession, GameManagement.RaceSessionLength - 1);
			_raceButton.GetComponentInChildren<Text>().fontSize = 16;
		}
		else
		{
			_raceButton.onClick.AddListener(UIManagement.PreRace.ConfirmPopUp);
			_raceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE");
			_raceButton.GetComponentInChildren<Text>().fontSize = 20;
		}
		_raceButton.onClick.AddListener(() => Invoke("QuickClickDisable", 0.5f));
		_skipToRaceButton.onClick.AddListener(UIManagement.PreRace.ConfirmPopUp);
		_skipToRaceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE");
		_skipToRaceButton.GetComponentInChildren<Text>().fontSize = 20;
		var positionContainer = _boatMain.transform.Find("Position Container");
		//set up position containers
		for (var i = 0; i < positionContainer.childCount; i++)
		{
			var positionObject = positionContainer.FindObject("Position " + i);
			if (GameManagement.PositionCount <= i)
			{
				positionObject.Active(false);
				continue;
			}
			positionObject.Active(true);
			var pos = GameManagement.Positions[i];
			positionObject.transform.FindText("Text").text = Localization.Get(pos.ToString());
			positionObject.transform.FindImage("Image").sprite = RoleLogos[pos.ToString()];
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
				if (session.Key.PerfectSelections == session.Key.PositionCount)
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
		foreach (var cm in GameManagement.CrewMemberList)
		{
			//create the static CrewMember UI (aka, the one that remains greyed out within the container at all times)
			var crewMember = CreateCrewMember(cm, _crewContainer.transform, false, true);
			//set anchoredPosition so that they are positioned correctly within the container at the bottom of the screen
			crewMember.RectTransform().anchoredPosition = Vector2.zero;
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
		Invoke("CrewContainerPaging", Time.smoothDeltaTime * 2);
	}

	/// <summary>
	/// Create a new CrewMember object
	/// </summary>
	private GameObject CreateCrewMember(CrewMember cm, Transform parent, bool usable, bool current)
	{
		var crewMember = Instantiate(_crewPrefab, parent, false);
		crewMember.transform.FindText("Name").text = SplitName(cm, true);
		crewMember.name = SplitName(cm);
		crewMember.GetComponent<CrewMemberUI>().SetUp(usable, current, cm, parent);
		crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(cm.Avatar, cm.GetMood());
		return crewMember;
	}

	public void SortCrew(bool playerTriggered = false)
	{
		var sortedCrewMembers = _crewContainer.GetComponentsInChildren<CrewMemberUI>().Where(c => c.transform.parent == _crewContainer.transform).ToList();
		var currentMembers = sortedCrewMembers.Concat(UIManagement.CrewMemberUI.Where(c => c.Usable)).ToList();
		var sortType = string.Empty;
		switch (_crewSort.value)
		{
			case 0:
				sortedCrewMembers = sortedCrewMembers.OrderBy(c => c.name).ToList();
				sortType = "Name";
				currentMembers.ForEach(c => c.SetSortValue(string.Empty));
				break;
			case 1:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[Skill.Charisma]).ToList();
				sortType = "Charisma";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RevealedSkills[Skill.Charisma] == 0 ? "?" : c.CrewMember.RevealedSkills[Skill.Charisma].ToString()));
				break;
			case 2:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[Skill.Perception]).ToList();
				sortType = "Perception";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RevealedSkills[Skill.Perception] == 0 ? "?" : c.CrewMember.RevealedSkills[Skill.Perception].ToString()));
				break;
			case 3:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[Skill.Quickness]).ToList();
				sortType = "Quickness";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RevealedSkills[Skill.Quickness] == 0 ? "?" : c.CrewMember.RevealedSkills[Skill.Quickness].ToString()));
				break;
			case 4:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[Skill.Body]).ToList();
				sortType = "Strength";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RevealedSkills[Skill.Body] == 0 ? "?" : c.CrewMember.RevealedSkills[Skill.Body].ToString()));
				break;
			case 5:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[Skill.Willpower]).ToList();
				sortType = "Willpower";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RevealedSkills[Skill.Willpower] == 0 ? "?" : c.CrewMember.RevealedSkills[Skill.Willpower].ToString()));
				break;
			case 6:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedSkills[Skill.Wisdom]).ToList();
				sortType = "Wisdom";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RevealedSkills[Skill.Wisdom] == 0 ? "?" : c.CrewMember.RevealedSkills[Skill.Wisdom].ToString()));
				break;
			case 7:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.GetMood()).ToList();
				sortType = "Mood";
				currentMembers.ForEach(c => c.SetSortValue(string.Empty));
				break;
			case 8:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedCrewOpinions.Values.Sum()/ (float)c.CrewMember.RevealedCrewOpinions.Count).ToList();
				sortType = "Average Opinion";
				currentMembers.ForEach(c => c.SetSortValue(string.Empty));
				break;
			case 9:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RevealedCrewOpinions[GameManagement.ManagerName]).ToList();
				sortType = "Manager Opinion";
				currentMembers.ForEach(c => c.SetSortValue(string.Empty));
				break;
			case 10:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.SessionsIncluded()).ToList();
				sortType = "Races";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.SessionsIncluded().ToString()));
				break;
			case 11:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.RacesWon()).ToList();
				sortType = "Wins";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.RacesWon().ToString()));
				break;
			case 12:
				sortedCrewMembers = sortedCrewMembers.OrderBy(c => c.CrewMember.Age).ToList();
				sortType = "Age";
				currentMembers.ForEach(c => c.SetSortValue(c.CrewMember.Age.ToString()));
				break;
			case 13:
				sortedCrewMembers = sortedCrewMembers.OrderByDescending(c => c.CrewMember.Avatar.Height * (c.CrewMember.Avatar.IsMale ? 1 : 0.935f)).ToList();
				sortType = "Height";
				currentMembers.ForEach(c => c.SetSortValue(string.Empty));
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
		if (playerTriggered)
		{
			TrackerEventSender.SendEvent(new TraceEvent("CrewSortChanged", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>(), sortType, AlternativeTracker.Alternative.Dialog));
		}
	}

	private void CrewContainerPaging()
	{
		CrewContainerPaging(0);
	}

	public void CrewContainerPaging(int page)
	{
		if (_crewContainer.transform.RectTransform().rect.width > _crewContainer.transform.parent.RectTransform().rect.width)
		{
			_crewContainer.GetComponentInParent<ScrollRect>().horizontalNormalizedPosition = page;
			_crewContainer.transform.parent.RectTransform().anchorMin = new Vector2(page * 0.05f, 0);
			_crewContainer.transform.parent.RectTransform().anchorMax = new Vector2(0.95f + (page * 0.05f), 1);
			foreach (var button in _crewPagingButtons)
			{
				button.Active(true);
			}
			_crewPagingButtons[page].Active(false);
		}
		else
		{
			_crewContainer.GetComponentInParent<ScrollRect>().horizontalNormalizedPosition = 0;
			_crewContainer.transform.parent.RectTransform().anchorMin = new Vector2(0, 0);
			_crewContainer.transform.parent.RectTransform().anchorMax = new Vector2(1, 1);
			foreach (var button in _crewPagingButtons)
			{
				button.Active(false);
			}
		}
	}

	/// <summary>
	/// Get the history of results, taking and skipping the amount given
	/// </summary>
	public List<KeyValuePair<Boat, KeyValuePair<int, int>>> GetLineUpHistory(int skipAmount, int takeAmount)
	{
		var boats = GameManagement.ReverseLineUpHistory.Skip(skipAmount).Take(takeAmount).ToList();
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
		var stageIcon = oldBoat.transform.FindImage("Stage");
		var isRace = stageNumber == 0;
		stageIcon.sprite = isRace ? _raceIcon : _practiceIcon;
		oldBoat.transform.FindText("Stage Number").text = isRace ? string.Empty : stageNumber.ToString();
		//get selection mistakes for this line-up and set-up feedback UI
		var mistakeList = boat.GetAssignmentMistakes(3);
		if (GameManagement.ShowTutorial && GameManagement.SessionCount == 1)
		{
			mistakeList = _mistakeIcons.Select(m => m.Name).Where(m => m != "Correct" && m != "Hidden").OrderBy(m => Guid.NewGuid()).Take(2).ToList();
			mistakeList.Add("Hidden");
		}
		SetMistakeIcons(mistakeList, oldBoat, boat.PerfectSelections, boat.ImperfectSelections, boat.IncorrectSelections);
		GetResult(isRace, boat, offset, oldBoat.transform.FindText("Score"));
		var crewContainer = oldBoat.transform.Find("Crew Container");
		var crewCount = 0;
		//for each position, create a new CrewMember UI object and place accordingly
		foreach (var pair in boat.PositionCrew)
		{
			//create CrewMember UI object for the CrewMember that was in this position
			var crewMember = crewContainer.FindObject("Crew Member " + crewCount);
			crewMember.Active(true);
			var current = pair.Value.Current();
			crewMember.transform.FindText("Name").text = SplitName(pair.Value, true);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(pair.Value.Avatar, -(GameManagement.GetRacePosition(boat.Score, boat.PositionCount) - 3) * 2);
			crewMember.GetComponent<CrewMemberUI>().SetUp(false, current, pair.Value, crewContainer);

			var positionImage = crewMember.transform.FindObject("Position");
			//update current position button
			positionImage.GetComponent<Image>().enabled = true;
			positionImage.GetComponent<Image>().sprite = RoleIcons[pair.Key.ToString()];
			positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
			var currentPosition = pair.Key;
			positionImage.GetComponent<Button>().onClick.AddListener(() => UIManagement.PositionDisplay.SetUpDisplay(currentPosition, TrackerTriggerSource.TeamManagementScreen.ToString()));
			crewCount++;
		}
		for (var i = crewCount; i < crewContainer.childCount; i++)
		{
			var crewMember = crewContainer.FindObject("Crew Member " + i);
			crewMember.Active(false);
		}
		DoBestFit();
	}

	/// <summary>
	/// Create mistake icons and set values for each feedback 'light'
	/// </summary>
	private void SetMistakeIcons(List<string> mistakes, GameObject boat, int perfect, int imperfect, int incorrect)
	{
		var mistakeParent = boat.transform.Find("Icon Container");
		for (var i = 0; i < mistakeParent.childCount; i++)
		{
			var mistakeObject = mistakeParent.FindObject("Ideal Icon " + i);
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
		boat.transform.FindObject("Light Container").Active(true);
		boat.transform.FindComponentInChildren<Text>("Light Container/Green").text = perfect.ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Green"), "GREEN_PLACEMENT");
		boat.transform.FindComponentInChildren<Text>("Light Container/Yellow").text = imperfect.ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Yellow"), "YELLOW_PLACEMENT");
		boat.transform.FindComponentInChildren<Text>("Light Container/Red").text = incorrect.ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Red"), "RED_PLACEMENT");
	}

	/// <summary>
	/// Set up pointer enter and exit events for created objects that can be hovered over
	/// </summary>
	private void FeedbackHoverOver(Transform feedback, string text = "")
	{
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
	/// Reset the scrollbar position
	/// </summary>
	public void ResetScrollbar()
	{
		_previousScrollValue = 0;
		ChangeVisibleBoats(0);
	}

	/// <summary>
	/// Redraw the displayed historical results
	/// </summary>
	public void ChangeVisibleBoats(int amount)
	{
		_previousScrollValue += amount;
		_previousScrollValue = _previousScrollValue > GameManagement.SessionCount - 4 ? GameManagement.SessionCount - 4 : _previousScrollValue;
		_previousScrollValue = _previousScrollValue < 0 ? 0 : _previousScrollValue;
		var setUpCount = 0;
		foreach (var boat in GetLineUpHistory(_previousScrollValue, 4))
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
		_boatPagingButtons[1].Active(_previousScrollValue > 0);
		_boatPagingButtons[0].Active(_previousScrollValue < GameManagement.SessionCount - 4);
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
		Loading.Start();
		GameManagement.GameManager.SaveLineUpTask(offset, success =>
		{
			if (success)
			{
				ResetScrollbar();
				GetResult(GameManagement.CurrentRaceSession == 1, GameManagement.PreviousSession, offset, _raceButton.GetComponentInChildren<Text>(), true);
				//set-up next boat
				CreateNewBoat();
				UIManagement.Tutorial.ShareEvent(GetType().Name, "ConfirmLineUp");
			}
			Loading.Stop();
		});
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
				if (crewMember.name == SplitName(member))
				{
					position.RemoveCrew();
					crewMember.Place(position);
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
			if (!crewMember.CrewMember.Current())
			{
				crewMember.GetComponentInChildren<AvatarDisplay>().UpdateAvatar(crewMember.CrewMember.Avatar);
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
		UIManagement.PositionDisplay.ClosePositionPopUp(string.Empty);
		UIManagement.MemberMeeting.CloseCrewMemberPopUp(string.Empty);
		UIManagement.DisableSmallBlocker();
		DoBestFit();
	}

	/// <summary>
	/// Get and display the result of the previous race session
	/// </summary>
	private void GetResult(bool isRace, Boat boat, int offset, Text scoreText, bool current = false)
	{
		var timeTaken = TimeSpan.FromSeconds(1800 - ((boat.Score - 22) * 10) + offset);
		var finishPosition = 1;
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
			finishPosition = GameManagement.GetRacePosition(boat.Score, boat.PositionCount);
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
			TrackerEventSender.SendEvent(new TraceEvent("RaceResult", TrackerAsset.Verb.Completed, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.RaceNumber, GameManagement.RaceCount + 1 },
				{ TrackerContextKey.CurrentSession, (isRace ? GameManagement.RaceSessionLength : GameManagement.CurrentRaceSession - 1) + "/" + GameManagement.RaceSessionLength },
				{ TrackerContextKey.SessionType, isRace ? "Race" : "Practice" },
				{ TrackerContextKey.BoatLayout, newString },
				{ TrackerContextKey.Score, boat.Score },
				{ TrackerContextKey.ScoreAverage, ((float)boat.Score / boat.PositionCount).ToString(CultureInfo.InvariantCulture) },
				{ TrackerContextKey.IdealCorrectPlacement, boat.PerfectSelections },
				{ TrackerContextKey.IdealCorrectMemberWrongPosition, boat.ImperfectSelections },
				{ TrackerContextKey.IdealIncorrectPlacement, boat.IncorrectSelections }
			}, CompletableTracker.Completable.Race));

			SUGARManager.GameData.Send("Race Session Score", boat.Score);
			SUGARManager.GameData.Send("Current Boat Size", boat.PositionCount);
			SUGARManager.GameData.Send("Race Session Score Average", (float)boat.Score / boat.PositionCount);
			SUGARManager.GameData.Send("Race Session Perfect Selection Average", boat.PerfectSelections / boat.PositionCount);
			SUGARManager.GameData.Send("Race Time", (long)timeTaken.TotalSeconds);
			SUGARManager.GameData.Send("Post Race Crew Average Mood", GameManagement.AverageTeamMood);
			SUGARManager.GameData.Send("Post Race Crew Average Manager Opinion", GameManagement.AverageTeamManagerOpinion);
			SUGARManager.GameData.Send("Post Race Crew Average Opinion", GameManagement.AverageTeamOpinion);
			SUGARManager.GameData.Send("Post Race Boat Average Mood", GameManagement.AverageBoatMood);
			SUGARManager.GameData.Send("Post Race Boat Average Manager Opinion", GameManagement.AverageBoatManagerOpinion);
			SUGARManager.GameData.Send("Post Race Boat Average Opinion", GameManagement.AverageBoatOpinion);
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
	}

	private void UpdateSeasonProgress(int result = 0)
	{
		if (result > 0)
		{
			var progressBar = _ongoingResultContainer.transform.GetChild(GameManagement.RaceCount - 1);
			progressBar.GetComponent<Image>().fillAmount = 1;
			var positionText = progressBar.GetComponentInChildren<Text>().gameObject.AddComponent<TextLocalization>();
			positionText.Key = "POSITION_" + result;
			positionText.Set();
		}
		else
		{
			_ongoingResultContainer.transform.GetChild(GameManagement.RaceCount).GetComponent<Image>().fillAmount = (GameManagement.CurrentRaceSession - 1) / (float)GameManagement.RaceSessionLength;
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
		else if (GameManagement.RageMode && !SUGARManager.Unity.AnyActiveUI)
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
			position.transform.FindText("Text").text = Localization.Get(position.Position.ToString());
		}
		_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat(GameManagement.IsRace ? "RACE_BUTTON_RACE" : "RACE_BUTTON_PRACTICE", true, GameManagement.CurrentRaceSession, GameManagement.RaceSessionLength - 1);
		_skipToRaceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE");
		if (_endRace.gameObject.activeSelf && GameManagement.RageMode)
		{
			_feedbackButton.GetComponentInChildren<Text>(true).text = Localization.Get(GameManagement.QuestionnaireCompleted ? "FEEDBACK_BUTTON" : "CONFLICT_QUESTIONNAIRE");
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		_boatMain.transform.FindObject("Position Container").GetComponentsInChildren<PositionUI>().Select(p => p.transform.FindText("Text")).BestFit();
		_ongoingResultContainer.transform.parent.BestFit();
		UIManagement.CrewMemberUI.Select(c => c.gameObject.transform.FindText("Name")).BestFit();
		var currentPosition = _boatContainer.transform.localPosition.y - _boatContainer.RectTransform().anchoredPosition.y;
		if (!Mathf.Approximately(_boatMain.GetComponent<LayoutElement>().preferredHeight, Mathf.Abs(currentPosition) * 0.2f))
		{
			foreach (Transform boat in _boatContainer.transform)
			{
				boat.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(currentPosition) * 0.2f;
			}
		}
		_crewSort.GetComponentsInChildren<Text>(true).ToList().BestFit();
	}

	/// <summary>
	/// Go to questionnaire state
	/// </summary>
	private void TriggerQuestionnaire()
	{
		if (GameManagement.RageMode)
		{
			UIStateManager.StaticGoToQuestionnaire();
		}
	}

	/// <summary>
	/// Go to feedback state
	/// </summary>
	private void TriggerFeedback()
	{
		if (!GameManagement.RageMode)
		{
			UIStateManager.StaticGoToFeedback();
		}
	}
}