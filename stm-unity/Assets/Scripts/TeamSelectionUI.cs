using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PlayGen.SUGAR.Unity;

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
[RequireComponent(typeof(TeamSelection))]
public class TeamSelectionUI : ObservableMonoBehaviour, IScrollHandler, IDragHandler {
	private TeamSelection _teamSelection;
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
	private Icon[] _mistakeIcons;
	[SerializeField]
	private Icon[] _roleIcons;
	public Icon[] RoleLogos;
	[SerializeField]
	private GameObject _crewPrefab;
	[SerializeField]
	private GameObject _recruitPrefab;
	[SerializeField]
	private Button _raceButton;
	[SerializeField]
	private GameObject _recruitPopUp;
	private readonly List<GameObject> _currentCrewButtons = new List<GameObject>();
	private readonly List<Button> _recruitButtons = new List<Button>();
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private PositionDisplayUI _positionUI;
	[SerializeField]
	private PostRaceEventUI[] _postRaceEvents;
	private int _positionsEmpty;
	[SerializeField]
	private GameObject _preRacePopUp;
	[SerializeField]
	private GameObject _postRacePopUp;
	[SerializeField]
	private GameObject _promotionPopUp;
	[SerializeField]
	private GameObject _postRaceCrewPrefab;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Button _smallerPopUpBlocker;
	[SerializeField]
	private Button _quitBlocker;
	[SerializeField]
	private Sprite _practiceIcon;
	[SerializeField]
	private Sprite _raceIcon;
	private float _recruitCost;
	[SerializeField]
	private HoverPopUpUI _hoverPopUp;

	private void Awake()
	{
		_teamSelection = GetComponent<TeamSelection>();
	}

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Create UI for all of the previous line-ups and one for the next line-up
	/// </summary>
	private void Start()
	{
		_recruitCost = _teamSelection.GetConfigValue(ConfigKeys.RecruitmentCost);
		ResetScrollbar();
		CreateNewBoat();
	}

	/// <summary>
	/// Toggle interactivity of race and recruitment buttons by if they are currently allowable 
	/// </summary>
	private void Update()
	{
		if (_positionsEmpty > 0 && _raceButton.interactable)
		{
			DisableRacing();
		}
		else if (_positionsEmpty == 0 && !_raceButton.interactable)
		{
			EnableRacing();
		}
		var actionAllowance = _teamSelection.QuestionAllowance();
		var crewAllowance = _teamSelection.CrewEditAllowance();
		var canAdd = _teamSelection.CanAddCheck();
		if ((actionAllowance < _recruitCost || crewAllowance == 0 || !canAdd) && _recruitButtons.Count > 0 && _recruitButtons[0].IsInteractable())
		{
			foreach (var b in _recruitButtons)
			{
				b.interactable = false;
				if (actionAllowance < _recruitCost)
				{
					FeedbackHoverOver(b.transform, "RECRUIT_BUTTON_HOVER_ALLOWANCE");
				}
				else if (crewAllowance == 0)
				{
					FeedbackHoverOver(b.transform, Localization.GetAndFormat("RECRUIT_BUTTON_HOVER_LIMIT", false, _teamSelection.StartingCrewEditAllowance()));
				}
			}
			
		}
		else if (actionAllowance >= _recruitCost && crewAllowance > 0 && canAdd && _recruitButtons.Count > 0 && !_recruitButtons[0].IsInteractable())
		{
			foreach (var b in _recruitButtons)
			{
				b.interactable = true;
				b.GetComponent<HoverObject>().Enabled = false;
			}
		}
		if (_raceButton.gameObject.activeSelf && _teamSelection.GetStage() != _teamSelection.GetSessionLength())
		{
			if (_raceButton.GetComponentInChildren<Text>().text.Last().ToString() != _teamSelection.GetSessionLength().ToString())
			{
				var stageIcon = _boatMain.transform.Find("Stage").GetComponent<Image>();
				var isRace = _teamSelection.GetStage() == _teamSelection.GetSessionLength();
				stageIcon.sprite = isRace ? _raceIcon : _practiceIcon;
				stageIcon.gameObject.SetActive(true);
				_boatMain.transform.Find("Stage Number").GetComponent<Text>().text = _teamSelection.GetStage().ToString();
				_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat("RACE_BUTTON_PRACTICE", true, _teamSelection.GetStage(), _teamSelection.GetSessionLength() - 1);
				_raceButton.GetComponentInChildren<Text>().fontSize = 16;
			}
		}
	}

	private void EnableRacing()
	{
		_raceButton.interactable = true;
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	private void DisableRacing()
	{
		_raceButton.interactable = false;
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Used to rearrange CrewMember names. shortName set to true results in first initial and last name, set to false results in last name, first names
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

	/// <summary>
	/// Set up a new boat (aka, one used for positioning and racing)
	/// </summary>
	private void CreateNewBoat()
	{
		var team = _teamSelection.GetTeam();
		var stageIcon = _boatMain.transform.Find("Stage").GetComponent<Image>();
		var isRace = _teamSelection.GetStage() == _teamSelection.GetSessionLength();
		if (team.Boat.Positions.Count > 0)
		{
			stageIcon.sprite = isRace ? _raceIcon : _practiceIcon;
			stageIcon.gameObject.SetActive(true);
		}
		else
		{
			stageIcon.gameObject.SetActive(false);
		}
		_boatMain.transform.Find("Stage Number").GetComponent<Text>().text = isRace || team.Boat.Positions.Count == 0 ? string.Empty : _teamSelection.GetStage().ToString();
		_positionsEmpty = team.Boat.Positions.Count;
		_raceButton.onClick.RemoveAllListeners();
		//add click handler to raceButton according to session taking place
		if (_teamSelection.GetStage() != _teamSelection.GetSessionLength())
		{
			_raceButton.onClick.AddListener(ConfirmLineUpCheck);
			_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat("RACE_BUTTON_PRACTICE", true, _teamSelection.GetStage(), _teamSelection.GetSessionLength() - 1);
			_raceButton.GetComponentInChildren<Text>().fontSize = 16;
		}
		else
		{
			_raceButton.onClick.AddListener(ConfirmPopUp);
			_raceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE", true);
			_raceButton.GetComponentInChildren<Text>().fontSize = 20;
		}
		var positionContainer = _boatMain.transform.Find("Position Container");
		for (int i = 0; i < positionContainer.childCount; i++)
		{
			var positionObject = positionContainer.Find("Position " + i).gameObject;
			if (team.Boat.Positions.Count <= i)
			{
				positionObject.SetActive(false);
				continue;
			}
			positionObject.SetActive(true);
			var pos = team.Boat.Positions[i];
			positionObject.transform.Find("Name").GetComponent<Text>().text = Localization.Get(pos.ToString());
			positionObject.transform.Find("Image").GetComponent<Image>().sprite = RoleLogos.First(mo => mo.Name == pos.ToString()).Image;
			positionObject.GetComponent<PositionUI>().SetUp(this, _positionUI, pos);
		}
		_raceButton.gameObject.SetActive(team.Boat.Positions.Count > 0);
		DoBestFit();
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(CompletableTracker).Name, "Started", "RaceSession", "RaceSessionStarted", CompletableTracker.Completable.Race));
		ResetCrew();
		RepeatLineUp();
	}

	/// <summary>
	/// Set up new crew objects for a new race session/upon resets for hiring/firing
	/// </summary>
	private void CreateCrew()
	{
		var crew = _teamSelection.LoadCrew();
		var sortedCrew = new List<Transform>();
		foreach (var cm in crew)
		{
			//create the static CrewMember UI (aka, the one that remains greyed out within the container at all times)
			var crewMember = CreateCrewMember(cm, _crewContainer.transform, false, true);
			//set anchoring, pivot and anchoredPosition so that they are positioned correctly within the container at the bottom of the screen
			((RectTransform)crewMember.transform).anchorMin = new Vector2(0.5f, 0.5f);
			((RectTransform)crewMember.transform).anchorMax = new Vector2(0.5f, 0.5f);
			((RectTransform)crewMember.transform).pivot = new Vector2(0, 0.5f);
			((RectTransform)crewMember.transform).anchoredPosition = Vector2.zero;
			sortedCrew.Add(crewMember.transform);
			_currentCrewButtons.Add(crewMember);
			//create the draggable copy of the above
			if (_teamSelection.GetTeam().Boat.Positions.Count > 0)
			{
				var crewMemberDraggable = CreateCrewMember(cm, crewMember.transform, true, true);
				crewMemberDraggable.transform.position = crewMember.transform.position;
				_currentCrewButtons.Add(crewMemberDraggable);
			}
		}
		//create a recruitment UI object for each empty spot in the crew
		for (var i = 0; i < _teamSelection.CanAddAmount(); i++)
		{
			var recruit = Instantiate(_recruitPrefab);
			recruit.transform.SetParent(_crewContainer.transform, false);
			recruit.name = "zz Recruit";
			recruit.GetComponent<Button>().onClick.AddListener(delegate { _recruitPopUp.SetActive(true); });
			_recruitButtons.Add(recruit.GetComponent<Button>());
			sortedCrew.Add(recruit.transform);
		}
		//sort CrewMembers by their surnames
		sortedCrew = sortedCrew.OrderBy(c => c.name).ToList();
		foreach (var crewMember in sortedCrew)
		{
			crewMember.SetAsLastSibling();
		}
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
		crewMember.GetComponent<CrewMemberUI>().SetUp(usable, current, _teamSelection, _meetingUI, _positionUI, cm, parent, _roleIcons);
		crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(cm.Avatar, cm.GetMood(), true);
		return crewMember;
	}

	/// <summary>
	/// Instantiate and position UI for an existing boat (aka, line-up already selected in the past)
	/// </summary>
	private void CreateHistoricalBoat(GameObject oldBoat, Boat boat, int offset, int stageNumber)
	{
		oldBoat.SetActive(true);
		var stageIcon = oldBoat.transform.Find("Stage").GetComponent<Image>();
		var isRace = stageNumber == 0;
		stageIcon.sprite = isRace ? _raceIcon : _practiceIcon;
		oldBoat.transform.Find("Stage Number").GetComponent<Text>().text = isRace ? string.Empty : stageNumber.ToString();
		var idealScore = boat.IdealMatchScore;
		var currentCrew = _teamSelection.GetTeam().CrewMembers;
		//get selection mistakes for this line-up and set-up feedback UI
		var mistakeList = boat.GetAssignmentMistakes(3);
		SetMistakeIcons(mistakeList, oldBoat, idealScore, boat.Positions.Count);
		var scoreDiff = GetResult(isRace, boat, offset, oldBoat.transform.Find("Score").GetComponent<Text>());
		var crewContainer = oldBoat.transform.Find("Crew Container");
		var crewCount = 0;
		//for each position, create a new CrewMember UI object and place accordingly
		foreach (var pair in boat.PositionCrew)
		{
			//create CrewMember UI object for the CrewMember that was in this position
			var crewMember = crewContainer.Find("Crew Member " + crewCount).gameObject;
			crewMember.SetActive(true);
			var current = currentCrew.ContainsKey(pair.Value.Name);
			crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(pair.Value.Name, true);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(pair.Value.Avatar, scoreDiff * (2f / boat.Positions.Count) + 3, true);
			crewMember.GetComponent<CrewMemberUI>().SetUp(false, current, _teamSelection, _meetingUI, _positionUI, pair.Value, crewContainer, _roleIcons);

			var positionImage = crewMember.transform.Find("Position").gameObject;
			//update current position button
			positionImage.GetComponent<Image>().enabled = true;
			positionImage.GetComponent<Image>().sprite = _roleIcons.First(mo => mo.Name == pair.Key.ToString()).Image;
			positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
			var currentPosition = pair.Key;
			positionImage.GetComponent<Button>().onClick.AddListener(delegate { _positionUI.SetUpDisplay(currentPosition); });
			//if CrewMember has since retired, change color of the object
			crewMember.transform.Find("Name").GetComponent<Text>().color = current ? UnityEngine.Color.white : UnityEngine.Color.grey;
			crewCount++;
		}
		for (int i = crewCount; i < crewContainer.childCount; i++)
		{
			var crewMember = crewContainer.Find("Crew Member " + i).gameObject;
			crewMember.SetActive(false);
		}
		DoBestFit();
	}

	/// <summary>
	/// Create mistake icons and set values for each feedback 'light'
	/// </summary>
	private void SetMistakeIcons(List<string> mistakes, GameObject boat, float idealScore, int positionCount)
	{
		var mistakeParent = boat.transform.Find("Icon Container");
		for (int i = 0; i < mistakeParent.childCount; i++)
		{
			var mistakeObject = mistakeParent.Find("Ideal Icon " + i).gameObject;
			if (mistakes.Count <= i || string.IsNullOrEmpty(mistakes[i]))
			{
				mistakeObject.SetActive(false);
				_hoverPopUp.DisplayHoverNoDelay(string.Empty);
				continue;
			}
			mistakeObject.SetActive(true);
			//set image based on mistake name
			var mistakeIcon = _mistakeIcons.First(mo => mo.Name == mistakes[i]).Image;
			mistakeObject.GetComponent<Image>().sprite = mistakeIcon;
			//add spaces between words where needed
			FeedbackHoverOver(mistakeObject.transform, Regex.Replace(mistakes[i], "([a-z])([A-Z])", "$1_$2"));
		}
		//set numbers for each 'light'
		var unideal = positionCount - (int)idealScore - ((idealScore % 1) * 10);
		boat.transform.Find("Light Container").gameObject.SetActive(true);
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
		feedback.GetComponent<HoverObject>().SetHoverText(text, _hoverPopUp);
	}

	/// <summary>
	/// Adjust the number of positions on the currentBoat that has not been given a CrewMember
	/// </summary>
	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!_popUpBlocker.gameObject.activeInHierarchy && !_smallerPopUpBlocker.gameObject.activeInHierarchy && !_quitBlocker.gameObject.activeInHierarchy)
		{
			_boatContainerScroll.value += eventData.scrollDelta.y * 0.55f * _boatContainerScroll.size;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!_popUpBlocker.gameObject.activeInHierarchy && !_smallerPopUpBlocker.gameObject.activeInHierarchy && !_quitBlocker.gameObject.activeInHierarchy)
		{
			_boatContainerScroll.value -= Mathf.Clamp(eventData.delta.y * 0.1f, -1, 1) * _boatContainerScroll.size;
		}
	}

	public void ResetScrollbar()
	{
		_boatContainerScroll.numberOfSteps = _teamSelection.GetTotalStages() - 4;
		_boatContainerScroll.size = 1f / _boatContainerScroll.numberOfSteps;
		_boatContainerScroll.value = 0;
		_previousScrollValue = 1;
		_boatContainerScroll.gameObject.SetActive(_boatContainerScroll.numberOfSteps >= 2);
		ChangeVisibleBoats();
	}

	public void ChangeVisibleBoats(bool forceOverwrite = false)
	{
		if (!forceOverwrite && _previousScrollValue == Mathf.RoundToInt(_boatContainerScroll.value * (_boatContainerScroll.numberOfSteps - 1)))
		{
			return;
		}
		var skipAmount = _boatContainerScroll.size > 0 ? Mathf.RoundToInt(_boatContainerScroll.value * (_boatContainerScroll.numberOfSteps - 1)) : 0;
		var setUpCount = 0;
		foreach (var boat in _teamSelection.GetLineUpHistory(skipAmount, 4))
		{
			var boatObject = _boatPool[setUpCount];
			boatObject.SetActive(true);
			CreateHistoricalBoat(boatObject, boat.Key, boat.Value.Key, boat.Value.Value);
			setUpCount++;
		}
		for (int i = setUpCount; i < _boatPool.Count; i++)
		{
			_boatPool[i].SetActive(false);
		}
		_previousScrollValue = skipAmount;
	}

	/// <summary>
	/// Display a pop-up before a race, with different text depending on if the player has ActionAllowance remaining
	/// </summary>
	private void ConfirmPopUp()
	{
		_preRacePopUp.SetActive(true);
		_preRacePopUp.GetComponentInChildren<Text>().text = _teamSelection.QuestionAllowance() > 0 ? Localization.GetAndFormat("RACE_CONFIRM_ALLOWANCE_REMAINING", false, _teamSelection.QuestionAllowance()) : Localization.Get("RACE_CONFIRM_NO_ALLOWANCE");
		var yesButton = _preRacePopUp.transform.Find("Yes").GetComponent<Button>();
		yesButton.onClick.RemoveAllListeners();
		yesButton.onClick.AddListener(ConfirmLineUp);
		var noButton = _preRacePopUp.transform.Find("No").GetComponent<Button>();
		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(CloseConfirmPopUp);
		DoBestFit();
		_popUpBlocker.transform.SetAsLastSibling();
		_preRacePopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(CloseConfirmPopUp);
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "CrewConfirm", "CrewConfirmCheck", AlternativeTracker.Alternative.Menu));
	}

	/// <summary>
	/// Close the race confirm pop-up
	/// </summary>
	public void CloseConfirmPopUp()
	{
		_preRacePopUp.SetActive(false);
		_popUpBlocker.gameObject.SetActive(false);
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "CrewConfirm", "CrewNotConfirmed", AlternativeTracker.Alternative.Menu));
	}

	/// <summary>
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUpCheck()
	{
		var lastRace = _teamSelection.GetLineUpHistory(0, 1).FirstOrDefault();
		var currentRace = _teamSelection.GetTeam().Boat;
		if (lastRace.Key != null)
		{
			if (currentRace.Positions.SequenceEqual(lastRace.Key.Positions) && currentRace.PositionCrew.OrderBy(pc => pc.Key.ToString()).SequenceEqual(lastRace.Key.PositionCrew.OrderBy(pc => pc.Key.ToString())))
			{
				DisplayRepeatWarning();
				return;
			}
		}
		ConfirmLineUp();
	}

	/// <summary>
	/// Display a pop-up before a race if the player is using the line-up as the previous race
	/// </summary>
	private void DisplayRepeatWarning()
	{
		_preRacePopUp.SetActive(true);
		_preRacePopUp.GetComponentInChildren<Text>().text = Localization.Get("REPEAT_CONFIRM");
		var yesButton = _preRacePopUp.transform.Find("Yes").GetComponent<Button>();
		yesButton.onClick.RemoveAllListeners();
		yesButton.onClick.AddListener(ConfirmLineUp);
		var noButton = _preRacePopUp.transform.Find("No").GetComponent<Button>();
		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(CloseRepeatWarning);
		DoBestFit();
		_popUpBlocker.transform.SetAsLastSibling();
		_preRacePopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(CloseRepeatWarning);
	}

	/// <summary>
	/// Close the repeat line-up warning
	/// </summary>
	public void CloseRepeatWarning()
	{
		_preRacePopUp.SetActive(false);
		_popUpBlocker.gameObject.SetActive(false);
	}

	/// <summary>
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUp()
	{
		CloseConfirmPopUp();
		//select random time offset
		var offset = UnityEngine.Random.Range(0, 10);
		//confirm the line-up with the simulation 
		SUGARManager.GameData.Send("Current Crew Size", _teamSelection.GetTeam().CrewMembers.Count);
		var currentBoat = _teamSelection.ConfirmLineUp(offset);
		ResetScrollbar();
		var oldLayout = currentBoat.Positions;
		var newLayout = _teamSelection.GetTeam().Boat.Positions;
		if (newLayout.Count > 0 && !oldLayout.SequenceEqual(newLayout))
		{
			DisplayPromotionPopUp(oldLayout, newLayout);
		}
		GetResult(_teamSelection.GetStage() - 1 == 0, currentBoat, offset, _raceButton.GetComponentInChildren<Text>(), true);
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "CrewConfirm", "CrewConfirmed", AlternativeTracker.Alternative.Menu));
		//set-up next boat
		CreateNewBoat();
	}

	/// <summary>
	/// Display pop-up which shows the race result
	/// </summary>
	private void DisplayPostRacePopUp(Dictionary<Position, CrewMember> currentPositions, int finishPosition, string finishPositionText)
	{
		_meetingUI.gameObject.SetActive(false);
		_positionUI.ClosePositionPopUp();
		_postRacePopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_postRacePopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(ClosePostRacePopUp);
		foreach (Transform child in _postRacePopUp.transform.Find("Crew"))
		{
			Destroy(child.gameObject);
		}
		var crewCount = 0;
		foreach (var pair in currentPositions)
		{
			var memberObject = Instantiate(_postRaceCrewPrefab);
			memberObject.transform.SetParent(_postRacePopUp.transform.Find("Crew"), false);
			memberObject.name = pair.Value.Name;
			memberObject.transform.Find("Avatar").GetComponentInChildren<AvatarDisplay>().SetAvatar(pair.Value.Avatar, -(finishPosition - 3) * 2);
			memberObject.transform.Find("Position").GetComponent<Image>().sprite = RoleLogos.First(mo => mo.Name == pair.Key.ToString()).Image;
			((RectTransform)memberObject.transform.Find("Position").transform).offsetMin = new Vector2(10, 0);
			if (crewCount % 2 != 0)
			{
				var currentScale = memberObject.transform.Find("Avatar").localScale;
				memberObject.transform.Find("Avatar").localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
				((RectTransform)memberObject.transform.Find("Position").transform).offsetMin = new Vector2(-10, 0);
			}
			crewCount++;
			memberObject.transform.SetAsLastSibling();
		}
		_postRacePopUp.transform.Find("Result").GetComponent<Text>().text = Localization.GetAndFormat("RACE_RESULT_POSTION", false, _teamSelection.GetTeam().Name, finishPositionText);
		DoBestFit();
	}

	/// <summary>
	/// Close the race result pop-up
	/// </summary>
	public void ClosePostRacePopUp()
	{
		_postRacePopUp.SetActive(false);
		_popUpBlocker.gameObject.SetActive(false);
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "RaceResult", "RaceResultClosed", AlternativeTracker.Alternative.Menu));
		if (_promotionPopUp.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			_promotionPopUp.transform.SetAsLastSibling();
			_popUpBlocker.gameObject.SetActive(true);
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(ClosePromotionPopUp);
		}
		else
		{
			SetPostRaceEventBlocker();
		}
	}

	/// <summary>
	/// Display pop-up which shows the boat promotion
	/// </summary>
	private void DisplayPromotionPopUp(List<Position> oldPos, List<Position> newPos)
	{
		var addedText = _promotionPopUp.transform.Find("Added List").GetComponent<Text>();
		var removedText = _promotionPopUp.transform.Find("Removed List").GetComponent<Text>();
		var newPositions = newPos.Where(n => !oldPos.Contains(n)).Select(n => Localization.Get(n.ToString())).ToArray();
		var oldPositions = oldPos.Where(o => !newPos.Contains(o)).Select(o => Localization.Get(o.ToString())).ToArray();
		var newList = string.Join("\n", newPositions);
		var oldList = string.Join("\n", oldPositions);
		addedText.text = newList;
		removedText.text = oldList;
		_promotionPopUp.SetActive(true);
	}

	/// <summary>
	/// Close the promotion pop-up
	/// </summary>
	public void ClosePromotionPopUp()
	{
		_promotionPopUp.SetActive(false);
		_popUpBlocker.gameObject.SetActive(false);
		SetPostRaceEventBlocker();
	}

	/// <summary>
	/// Set the blockers and transform order for any post race events being shown
	/// </summary>
	private void SetPostRaceEventBlocker()
	{
		foreach (var pre in _postRaceEvents)
		{
			if (pre.gameObject.activeInHierarchy && !Mathf.Approximately(pre.GetComponent<CanvasGroup>().alpha, 0))
			{
				_popUpBlocker.transform.SetAsLastSibling();
				pre.transform.parent.SetAsLastSibling();
				_popUpBlocker.gameObject.SetActive(true);
				_popUpBlocker.onClick.RemoveAllListeners();
				pre.SetBlockerOnClick();
				return;
			}
		}
	}

	/// <summary>
	/// Repeat the line-up (or what can be repeated) from what is passed to it
	/// </summary>
	private void RepeatLineUp()
	{
		//get currently positioned
		var currentPositions = new Dictionary<Position, CrewMember>();
		foreach (var pair in _teamSelection.GetTeam().Boat.PositionCrew)
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
					crewMember.PlacedEvent();
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
		//get current list of CrewMembers
		var currentCrew = _teamSelection.GetTeam().CrewMembers;
		//destroy recruitment buttons
		foreach (var b in _currentCrewButtons)
		{
			Destroy(b);
		}
		foreach (var crewMember in (CrewMemberUI[])FindObjectsOfType(typeof(CrewMemberUI)))
		{
			//destroy CrewMemberUI (making them unclickable) from those that are no longer in the currentCrew. Update avatar so they change into their causal outfit
			if (currentCrew.All(cm => cm.Key != crewMember.CrewMember.Name))
			{
				crewMember.GetComponentInChildren<AvatarDisplay>().UpdateAvatar(crewMember.CrewMember.Avatar, true);
				crewMember.Current = false;
				crewMember.transform.Find("Name").GetComponent<Text>().color = UnityEngine.Color.grey;
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
		_positionsEmpty = ((PositionUI[])FindObjectsOfType(typeof(PositionUI))).Length;
		//recreate crew and repeat previous line-up
		CreateCrew();
		RepeatLineUp();
		//close any open pop-ups
		_meetingUI.gameObject.SetActive(false);
		_positionUI.ClosePositionPopUp();
		DoBestFit();
	}

	/// <summary>
	/// Get and display the result of the previous race session
	/// </summary>
	private float GetResult(bool isRace, Boat boat, int offset, Text scoreText, bool current = false)
	{
		var timeTaken = TimeSpan.FromSeconds(1800 - ((boat.Score - 20) * 10) + offset);
		var finishPosition = 1;
		var expected = 7.5f * boat.Positions.Count;
		var scoreDiff = boat.Score - expected;
		if (!isRace)
		{
			scoreText.text = string.Format("{0:D2}:{1:D2}", timeTaken.Minutes, timeTaken.Seconds);
		}
		else
		{
			while (boat.Score < expected)
			{
				finishPosition++;
				expected -= boat.Positions.Count;
			}
			var finishPositionText = Localization.Get("POSITION_" + finishPosition);
			scoreText.text = string.Format("{0} {1}", Localization.Get("RACE_POSITION"), finishPositionText);
			if (current)
			{
				DisplayPostRacePopUp(boat.PositionCrew, finishPosition, finishPositionText);
			}
		}
		if (current)
		{
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(CompletableTracker).Name, "Completed", "CrewScore" + (_teamSelection.GetStage()), CompletableTracker.Completable.Race, true, boat.Score));
			SUGARManager.GameData.Send("Race Session Score", boat.Score);
			SUGARManager.GameData.Send("Current Boat Size", boat.Positions.Count);
			SUGARManager.GameData.Send("Race Session Score Average", (float)boat.Score / boat.Positions.Count);
			SUGARManager.GameData.Send("Race Session Ideal Score Average", boat.IdealMatchScore / boat.Positions.Count);
			SUGARManager.GameData.Send("Race Time", (long)timeTaken.TotalSeconds);
			SUGARManager.GameData.Send("Post Race Crew Average Mood", _teamSelection.GetTeamAverageMood());
			SUGARManager.GameData.Send("Post Race Crew Average Manager Opinion", _teamSelection.GetTeamAverageManagerOpinion());
			SUGARManager.GameData.Send("Post Race Crew Average Opinion", _teamSelection.GetTeamAverageOpinion());
			SUGARManager.GameData.Send("Post Race Boat Average Mood", _teamSelection.GetBoatAverageMood());
			SUGARManager.GameData.Send("Post Race Boat Average Manager Opinion", _teamSelection.GetBoatAverageManagerOpinion());
			SUGARManager.GameData.Send("Post Race Boat Average Opinion", _teamSelection.GetBoatAverageOpinion());
			foreach (var feedback in boat.SelectionMistakes)
			{
				SUGARManager.GameData.Send("Race Session Feedback", feedback);
			}
			if (isRace)
			{
				SUGARManager.GameData.Send("Race Position", finishPosition);
				SUGARManager.GameData.Send("Time Remaining", _teamSelection.QuestionAllowance());
				SUGARManager.GameData.Send("Time Taken", _teamSelection.StartingQuestionAllowance() - _teamSelection.QuestionAllowance());
			}
		}
		return scoreDiff;
	}

	public void OnEscape()
	{
		if (_preRacePopUp.activeInHierarchy)
		{
			if (_preRacePopUp.GetComponentInChildren<Text>().text == Localization.Get("REPEAT_CONFIRM"))
			{
				CloseRepeatWarning();
			}
			else
			{
				CloseConfirmPopUp();
			}
		}
		else if (_postRacePopUp.activeInHierarchy)
		{
			ClosePostRacePopUp();
		}
		else if (_promotionPopUp.activeInHierarchy)
		{
			ClosePromotionPopUp();
		}
		else if (_quitBlocker.gameObject.activeInHierarchy)
		{
			_quitBlocker.onClick.Invoke();
		}
		else if (!SUGARManager.Unity.AnyActiveUI)
		{
			FindObjectOfType<ScreenSideUI>().DisplayQuitWarning();
		}
	}

	private void OnLanguageChange()
	{
		foreach (var position in _boatMain.GetComponentsInChildren<PositionUI>()) {
			position.transform.Find("Name").GetComponent<Text>().text = Localization.Get(position.Position.ToString());
		}
		if (_teamSelection.GetStage() != _teamSelection.GetSessionLength())
		{
			_raceButton.GetComponentInChildren<Text>().text = Localization.GetAndFormat("RACE_BUTTON_PRACTICE", true, _teamSelection.GetStage(), _teamSelection.GetSessionLength() - 1);
		}
		else
		{
			_raceButton.GetComponentInChildren<Text>().text = Localization.Get("RACE_BUTTON_RACE", true);
		}
		ChangeVisibleBoats(true);
		_preRacePopUp.GetComponentInChildren<Text>().text = _teamSelection.QuestionAllowance() > 0 ? Localization.GetAndFormat("RACE_CONFIRM_ALLOWANCE_REMAINING", false, _teamSelection.QuestionAllowance()) : Localization.Get("RACE_CONFIRM_NO_ALLOWANCE");
		if (_postRacePopUp.activeInHierarchy)
		{
			var lastRace = _teamSelection.GetLineUpHistory(0, 1).First();
			GetResult(true, lastRace.Key, lastRace.Value.Key, _boatPool[0].transform.Find("Score").GetComponent<Text>());
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		_boatMain.transform.Find("Position Container").gameObject.BestFit();
		((CrewMemberUI[])FindObjectsOfType(typeof(CrewMemberUI))).Select(c => c.gameObject).BestFit();
		_preRacePopUp.GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
		_postRacePopUp.GetComponentsInChildren<Text>().Where(t => t.transform.parent == _postRacePopUp.transform).BestFit();
		_promotionPopUp.GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
		var currentPosition = _boatContainer.transform.localPosition.y - ((RectTransform)_boatContainer.transform).anchoredPosition.y;
		if (!Mathf.Approximately(_boatMain.GetComponent<LayoutElement>().preferredHeight, Mathf.Abs(currentPosition) * 0.2f))
		{
			foreach (Transform boat in _boatContainer.transform)
			{
				boat.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(currentPosition) * 0.2f;
			}
		}
	}
}
