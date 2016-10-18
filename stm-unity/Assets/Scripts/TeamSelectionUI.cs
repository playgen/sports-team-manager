using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
public class TeamSelectionUI : MonoBehaviour {
	private TeamSelection _teamSelection;
	[SerializeField]
	private GameObject _boatContainer;
	[SerializeField]
	private Scrollbar _boatContainerScroll;
	[SerializeField]
	private GameObject _crewContainer;
	[SerializeField]
	private GameObject _boatPrefab;
	[SerializeField]
	private GameObject _positionPrefab;
	[SerializeField]
	private GameObject _mistakePrefab;
	[SerializeField]
	private Icon[] _mistakeIcons;
	[SerializeField]
	private Icon[] _roleIcons;
	public Icon[] RoleLogos;
	[SerializeField]
	private GameObject _crewPrefab;
	[SerializeField]
	private GameObject _recruitPrefab;
	private Button _raceButton;
	[SerializeField]
	private GameObject _recruitPopUp;
	private readonly List<Button> _recruitButtons = new List<Button>();
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private PositionDisplayUI _positionUI;
	[SerializeField]
	private PostRaceEventUI _postRaceEventUI;
	private GameObject _currentBoat;
	private int _positionsEmpty;
	[SerializeField]
	private GameObject _preRacePopUp;
	[SerializeField]
	private GameObject _postRacePopUp;
	[SerializeField]
	private GameObject _postRaceCrewPrefab;
	[SerializeField]
	private Button _popUpBlocker;
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

	/// <summary>
	/// Create UI for all of the previous line-ups and one for the next line-up
	/// </summary>
	private void Start()
	{
		_recruitCost = _teamSelection.GetConfigValue(ConfigKeys.RecruitmentCost);
		foreach (var boat in _teamSelection.GetLineUpHistory())
		{
			CreateHistoricalBoat(boat.Key, boat.Value);
		}
		//force rebuild of layout in order to ensure historical boats are laid out correctly after being created
		LayoutRebuilder.ForceRebuildLayoutImmediate(_boatContainer.GetComponent<RectTransform>());
		CreateNewBoat();
	}

	/// <summary>
	/// Toggle interactivity of race and recruitment buttons by if they are currently allowable 
	/// </summary>
	private void Update()
	{
		if (_positionsEmpty > 0 && _raceButton.interactable)
		{
			_raceButton.interactable = false;
		}
		else if (_positionsEmpty == 0 && !_raceButton.interactable)
		{
			_raceButton.interactable = true;
		}
		var actionAllowance = _teamSelection.QuestionAllowance();
		var crewAllowance = _teamSelection.CrewEditAllowance();
		var canAdd = _teamSelection.CanAddCheck();
		if ((actionAllowance < _recruitCost || crewAllowance == 0 || !canAdd) && _recruitButtons.Count > 0 && _recruitButtons[0].IsInteractable())
		{
			foreach (var b in _recruitButtons)
			{
				b.interactable = false;
			}
			
		}
		else if (actionAllowance >= _recruitCost && crewAllowance > 0 && canAdd && _recruitButtons.Count > 0 && !_recruitButtons[0].IsInteractable())
		{
			foreach (var b in _recruitButtons)
			{
				b.interactable = true;
			}
		}
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.R))
		{
			RepeatLineUp();
		}
#endif
	}

	/// <summary>
	/// Update the height of boat objects to scale with screen size
	/// </summary>
	private void FixedUpdate()
	{
		var currentPosition = _boatContainer.transform.localPosition.y - _boatContainer.GetComponent<RectTransform>().anchoredPosition.y;
		if (!Mathf.Approximately(_currentBoat.GetComponent<LayoutElement>().preferredHeight, Mathf.Abs(currentPosition) * 0.2f))
		{
			foreach (Transform boat in _boatContainer.transform)
			{
				boat.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(currentPosition) * 0.2f;
			}
		}
	}

	/// <summary>
	/// Used to rearrange CrewMember names. shortName set to true results in first initial and last name, set to false results in last name, first names
	/// </summary>
	private string SplitName(string original, bool shortName = false)
	{
		var splitName = original.Split(' ');
		var lastName = splitName.Last();
		if (!shortName)
		{
			lastName += ", ";
		}
		foreach (var split in splitName)
		{
			if (split != splitName.Last())
			{
				if (!shortName)
				{
					lastName += split + " ";
				}
				else
				{
					lastName = split[0] + "." + lastName;
				}
			}
		}
		if (!shortName)
		{
			lastName = lastName.Remove(lastName.Length - 1, 1);
		}
		return lastName;
	}

	/// <summary>
	/// Instantiate a new UI object for a Boat line-up
	/// </summary>
	private GameObject CreateBoat(Boat boat, bool historical)
	{
		var newBoat = Instantiate(_boatPrefab);
		newBoat.transform.SetParent(_boatContainer.transform, false);
		newBoat.name = _boatPrefab.name;
		//required to ensure sizing stays in parallel with UI background
		newBoat.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(_boatContainer.transform.localPosition.y) * 0.2f;
		//set stage icon and button text according to type of session taking place
		var stageIcon = newBoat.transform.Find("Stage").GetComponent<Image>();
		var stageNumber = _teamSelection.GetStage();
		stageIcon.sprite = stageNumber == _teamSelection.GetSessionLength() ? _raceIcon : _practiceIcon;
		if (stageNumber != _teamSelection.GetSessionLength()) {
			newBoat.transform.Find("Race").GetComponentInChildren<Text>().text = "PRACTICE " + stageNumber + "/" + (_teamSelection.GetSessionLength() - 1);
			newBoat.transform.Find("Race").GetComponentInChildren<Text>().fontSize = 16;
		}
		//create UI object for each position (aka, what CrewMembers are dragged onto)
		if (!historical)
		{
			foreach (var pos in boat.Positions)
			{
				var positionObject = Instantiate(_positionPrefab);
				positionObject.transform.SetParent(newBoat.transform.Find("Position Container"), false);
				positionObject.transform.Find("Name").GetComponent<Text>().text = pos.GetName();
				positionObject.transform.Find("Image").GetComponent<Image>().sprite = RoleLogos.First(mo => mo.Name == pos.GetName()).Image;
				positionObject.name = pos.GetName();
				positionObject.GetComponent<PositionUI>().SetUp(this, _positionUI, pos);
			}
		}
		//hide feedback 'lights'/icons
		newBoat.transform.Find("Light Container").gameObject.SetActive(false);
		//set number of positions currently filled to match number of positions
		_positionsEmpty = boat.Positions.Count;
		//required to ensure positioning stays in parallel with UI background
		if (_boatContainer.transform.childCount > _teamSelection.GetSessionLength())
		{
			_boatContainerScroll.numberOfSteps = _boatContainer.transform.childCount - _teamSelection.GetSessionLength() + 1;
		}
		return newBoat;
	}

	/// <summary>
	/// Set up a new boat (aka, one used for positioning and racing)
	/// </summary>
	private void CreateNewBoat()
	{
		var team = _teamSelection.GetTeam();
		var boatObject = CreateBoat(team.Boat, false);
		//adjust visible boats so that this newly created one is displayed
		ChangeVisibleBoats();
		_raceButton = boatObject.transform.Find("Race").GetComponent<Button>();
		//add click handler to raceButton according to session taking place
		if (_teamSelection.GetStage() == _teamSelection.GetSessionLength())
		{
			_raceButton.onClick.AddListener(ConfirmPopUp);
		}
		else
		{
			_raceButton.onClick.AddListener(ConfirmLineUp);
		}
		_currentBoat = boatObject;
		CreateCrew();
		AdjustBoatVisibility(boatObject.GetComponent<CanvasGroup>(), true);
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
			crewMember.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
			crewMember.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
			crewMember.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
			crewMember.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			sortedCrew.Add(crewMember.transform);
			//create the draggable copy of the above
			var crewMemberDraggable = CreateCrewMember(cm, crewMember.transform, true, true);
			crewMemberDraggable.transform.position = crewMember.transform.position;
		}
		AdjustCrewMemberVisibility(_crewContainer, true);
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
		crewMember.transform.Find("Opinion").GetComponent<Image>().enabled = false;
		crewMember.transform.Find("Position").GetComponent<Image>().enabled = false;
		return crewMember;
	}

	/// <summary>
	/// Instantiate and position UI for an existing boat (aka, line-up already selected in the past)
	/// </summary>
	private void CreateHistoricalBoat(Boat boat, int offset)
	{
		var oldBoat = CreateBoat(boat, true);
		var teamScore = boat.Score;
		var idealScore = boat.IdealMatchScore;
		var currentCrew = _teamSelection.GetTeam().CrewMembers;
		//get selection mistakes for this line-up and set-up feedback UI
		var mistakeList = boat.GetAssignmentMistakes(6);
		CreateMistakeIcons(mistakeList, oldBoat, idealScore, boat.Positions.Count);
		_teamSelection.ConfirmLineUp(0, true);
		var scoreDiff = GetResult(_teamSelection.IsRace(), teamScore, boat.Positions.Count, offset, oldBoat.transform.Find("Score").GetComponent<Text>());
		var crewContainer = oldBoat.transform.Find("Position Container");
		//for each position, create a new CrewMember UI object and place accordingly
		foreach (var pair in boat.PositionCrew)
		{
			//create CrewMember UI object for the CrewMember that was in this position
			var crewMember = CreateCrewMember(pair.Value, crewContainer.transform, false, false);
			Destroy(crewMember.transform.Find("Opinion").GetComponent<Image>());
			crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(pair.Value.Avatar, scoreDiff * (2f / boat.Positions.Count) + 3);
			var positionImage = crewMember.transform.Find("Position").gameObject;
			//update current position button
			positionImage.GetComponent<Image>().enabled = true;
			positionImage.GetComponent<Image>().sprite = _roleIcons.First(mo => mo.Name == pair.Key.GetName()).Image;
			positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
			var currentPosition = pair.Key;
			positionImage.GetComponent<Button>().onClick.AddListener(delegate { _positionUI.SetUpDisplay(currentPosition); });
			//if CrewMember has since retired, remove CrewMemberUI from  the object
			if (!currentCrew.ContainsKey(pair.Value.Name))
			{
				Destroy(crewMember.GetComponent<CrewMemberUI>());
				crewMember.transform.Find("Name").GetComponent<Text>().color = UnityEngine.Color.grey;
			}
		}
		Destroy(oldBoat.transform.Find("Race").gameObject);
	}

	/// <summary>
	/// Create mistake icons and set values for each feedback 'light'
	/// </summary>
	private void CreateMistakeIcons(List<string> mistakes, GameObject boat, float idealScore, int positionCount)
	{
		var mistakeParentTop = boat.transform.Find("Icon Container Top");
		var mistakeParentBottom = boat.transform.Find("Icon Container Bottom");
		//create new mistake icon for each mistake
		for (int i = 0; i < mistakes.Count; i++)
		{
			var mistakeObject = Instantiate(_mistakePrefab);
			mistakeObject.transform.SetParent(mistakes.Count / 2 > i ? mistakeParentTop : mistakeParentBottom, false);
			mistakeObject.name = mistakes[i];
			//set image based on mistake name
			var mistakeIcon = _mistakeIcons.First(mo => mo.Name == mistakes[i]).Image;
			mistakeObject.GetComponent<Image>().sprite = mistakeIcon;
			//add spaces between words where needed
			FeedbackHoverOver(mistakeObject.transform, Regex.Replace(mistakes[i], "([a-z])([A-Z])", "$1 $2"));
		}
		//set numbers for each 'light'
		var unideal = positionCount - (int)idealScore - ((idealScore % 1) * 10);
		boat.transform.Find("Light Container").gameObject.SetActive(true);
		boat.transform.Find("Light Container/Green").GetComponentInChildren<Text>().text = ((int)idealScore).ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Green"), "Ideal Placements");
		boat.transform.Find("Light Container/Yellow").GetComponentInChildren<Text>().text = Mathf.RoundToInt(((idealScore % 1) * 10)).ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Yellow"), "Ideal In Another Position");
		boat.transform.Find("Light Container/Red").GetComponentInChildren<Text>().text = Mathf.RoundToInt(unideal).ToString();
		FeedbackHoverOver(boat.transform.Find("Light Container/Red"), "Unideal Placements");
	}

	/// <summary>
	/// Set up pointer enter and exit events for created objects that can be hovered over
	/// </summary>
	private void FeedbackHoverOver(Transform feedback, string text)
	{
		var trans = feedback;
		var mis = text;
		var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
		enter.callback.AddListener(data => { _hoverPopUp.SetHoverObject(trans); });
		enter.callback.AddListener(data => { _hoverPopUp.DisplayHover(mis); });
		trans.GetComponent<EventTrigger>().triggers.Add(enter);
		var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
		click.callback.AddListener(data => { _hoverPopUp.SetHoverObject(trans); });
		click.callback.AddListener(data => { _hoverPopUp.DisplayHoverNoDelay(mis); });
		trans.GetComponent<EventTrigger>().triggers.Add(click);
		var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
		exit.callback.AddListener(data => { _hoverPopUp.HideHover(); });
		trans.GetComponent<EventTrigger>().triggers.Add(exit);
	}

	/// <summary>
	/// Adjust the number of positions on the currentBoat that has not been given a CrewMember
	/// </summary>
	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
	}

	/// <summary>
	/// Make it so only boats visible on screen are drawn. Improves frame rate greatly after many races.
	/// </summary>
	private void ChangeVisibleBoats()
	{
		var currentPositionTop = -_boatContainer.GetComponent<RectTransform>().localPosition.y;
		var currentPositionBottom = -_boatContainer.GetComponent<RectTransform>().anchoredPosition.y;
		foreach (var boat in _boatContainer.GetComponentsInChildren<CanvasGroup>())
		{
			var boatPosition = boat.GetComponent<RectTransform>().localPosition.y;
			if (boatPosition < currentPositionTop && boatPosition > currentPositionBottom)
			{
				AdjustBoatVisibility(boat, true);
			}
			else
			{
				AdjustBoatVisibility(boat, false);
			}
		}
		_boatContainerScroll.value = Mathf.Round(_boatContainerScroll.value * _boatContainerScroll.numberOfSteps) / _boatContainerScroll.numberOfSteps;
	}

	/// <summary>
	/// Adjust elements on boat UI objects according to if they are on screen or not
	/// </summary>
	private void AdjustBoatVisibility(CanvasGroup boat, bool visibility)
	{
		boat.alpha = visibility ? 1 : 0;
		boat.interactable = visibility;
		boat.blocksRaycasts = visibility;
		AdjustCrewMemberVisibility(boat.gameObject, visibility);
	}

	private void AdjustCrewMemberVisibility(GameObject group, bool visibility)
	{
		foreach (var aspect in group.GetComponentsInChildren<AspectRatioFitter>())
		{
			aspect.enabled = visibility;
		}
		foreach (var layout in group.GetComponentsInChildren<LayoutGroup>())
		{
			layout.enabled = visibility;
		}
		foreach (var layout in group.GetComponentsInChildren<LayoutElement>())
		{
			if (layout.gameObject != group.gameObject)
			{
				layout.enabled = visibility;
			}
		}
		foreach (var crewMember in group.GetComponentsInChildren<CrewMemberUI>())
		{
			crewMember.enabled = visibility;
		}
		foreach (var image in group.GetComponentsInChildren<Image>())
		{
			image.enabled = image.sprite != null && visibility;
		}
		foreach (var text in group.GetComponentsInChildren<Text>())
		{
			text.enabled = visibility;
		}
	}

	/// <summary>
	/// Display a pop-up before a race, with different text depending on if the player has ActionAllowance remaining
	/// </summary>
	private void ConfirmPopUp()
	{
		_preRacePopUp.SetActive(true);
		_preRacePopUp.GetComponentInChildren<Text>().text = _teamSelection.QuestionAllowance() > 0 ? string.Format("You have {0} Talk Time remaining! If you don't use it before the race, they will be lost.\n\nAre you sure you want to race with this line-up now?", _teamSelection.QuestionAllowance()) : "Are you sure you want to race with this line-up now?";
		_popUpBlocker.transform.SetAsLastSibling();
		_preRacePopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(CloseConfirmPopUp);
	}

	/// <summary>
	/// Close the race confirm pop-up
	/// </summary>
	public void CloseConfirmPopUp()
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
		//store current positions
		var currentPositions = new Dictionary<Position, CrewMember>();
		foreach (var pair in _teamSelection.GetTeam().Boat.PositionCrew)
		{
			currentPositions.Add(pair.Key, pair.Value);
		}
		//select random time offset
		var offset = UnityEngine.Random.Range(0, 10);
		//confirm the line-up with the simulation 
		var currentBoat = _teamSelection.ConfirmLineUp(offset);
		Tracker.T.completable.Completed("Crew Confirmed", CompletableTracker.Completable.Stage, true, currentBoat.Score);
		var idealScore = currentBoat.IdealMatchScore;
		var mistakeList = currentBoat.GetAssignmentMistakes(6);
		CreateMistakeIcons(mistakeList, _currentBoat, idealScore, currentPositions.Count);
		var scoreDiff = GetResult(_teamSelection.IsRace(), currentBoat.Score, currentPositions.Count, offset, _currentBoat.transform.Find("Score").GetComponent<Text>(), currentPositions);
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			//for 'Current' CrewMemberUI objects (aka, those that were in the CrewContainer) 
			if (crewMember.Current)
			{
				//destroy the opinion image
				Destroy(crewMember.transform.Find("Opinion").GetComponent<Image>());
				//Destroy CrewmemberUI Gameobject if still contained within CrewContainer
				if (crewMember.transform.parent.name == crewMember.name || crewMember.transform.parent.name == _crewContainer.name)
				{
					Destroy(crewMember.gameObject);
				}
				//otherwise, set-up again to not longer be dragable, reset position icon click handler, update displayed mood according to score line-up got
				else
				{
					crewMember.GetComponent<CrewMemberUI>().SetUp(false, false, _teamSelection, _meetingUI, _positionUI, crewMember.CrewMember, _crewContainer.transform, _roleIcons);
					crewMember.transform.Find("Position").GetComponent<Button>().onClick.RemoveAllListeners();
					var position = crewMember.transform.parent.GetComponent<PositionUI>().Position;
					crewMember.transform.Find("Position").GetComponent<Button>().onClick.AddListener(delegate { _positionUI.SetUpDisplay(position); });
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(crewMember.CrewMember.Avatar, scoreDiff * (2f / _teamSelection.GetTeam().Boat.Positions.Count) + 3);
				}
			}
		}
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			var crewMember = position.transform.GetChild(2);
			position.transform.GetChild(2).SetParent(position.transform.parent, true);
			Destroy(position.gameObject);
			crewMember.SetAsFirstSibling();
		}
		//destroy recruitment buttons
		foreach (var b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();
		//destroy race button
		Destroy(_raceButton.gameObject);
		//set-up next boat
		CreateNewBoat();
		//update the position pop-up and meeting pop-up if they are being displayed
		_positionUI.UpdateDisplay();
		if (_meetingUI.gameObject.activeSelf)
		{
			_meetingUI.Display();
		}
		//set new boat to display the same line-up (or as much as possible if boat layout is different)
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
			memberObject.transform.Find("Position").GetComponent<Image>().sprite = RoleLogos.First(mo => mo.Name == pair.Key.GetName()).Image;
			memberObject.transform.Find("Position").GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
			if (crewCount % 2 != 0)
			{
				var currentScale = memberObject.transform.Find("Avatar").localScale;
				memberObject.transform.Find("Avatar").localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
				memberObject.transform.Find("Position").GetComponent<RectTransform>().offsetMin = new Vector2(-10, 0);
			}
			crewCount++;
			memberObject.transform.SetAsLastSibling();
		}
		_postRacePopUp.transform.Find("Result").GetComponent<Text>().text = _teamSelection.GetTeam().Name + " finished " + finishPositionText + "!";
	}

	/// <summary>
	/// Close the race result pop-up
	/// </summary>
	public void ClosePostRacePopUp()
	{
		_postRacePopUp.SetActive(false);
		_popUpBlocker.gameObject.SetActive(false);
		if (_postRaceEventUI.gameObject.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			_postRaceEventUI.transform.SetAsLastSibling();
			_popUpBlocker.gameObject.SetActive(true);
			_popUpBlocker.onClick.RemoveAllListeners();
			_postRaceEventUI.SetBlockerOnClick();
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
		var positions = _currentBoat.GetComponentsInChildren<PositionUI>().OrderBy(p => p.transform.GetSiblingIndex()).ToList();
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
	}

	/// <summary>
	/// Destroy the currently created CrewMember object and adjust any older CrewMember objects where the member has retired/been fired
	/// </summary>
	public void ResetCrew()
	{
		//remove attachment between CrewMemberUI and PositionUI
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			position.RemoveCrew();
		}
		//get current list of CrewMembers
		var currentCrew = _teamSelection.GetTeam().CrewMembers;
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			//destroy all current CrewMemberUI objects
			if (crewMember.Current)
			{
				Destroy(crewMember.gameObject);
			}
			else
			{
				//destroy CrewMemberUI (making them unclickable) from those that are no longer in the currentCrew. Update avatar so they change into their causal outfit
				if (currentCrew.All(cm => cm.Key != crewMember.CrewMember.Name))
				{
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateAvatar(crewMember.CrewMember.Avatar, true);
					Destroy(crewMember);
					crewMember.transform.Find("Name").GetComponent<Text>().color = UnityEngine.Color.grey;
				}
			}
		}
		//destroy recruitment buttons
		foreach (var b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();
		//reset empty positions
		_positionsEmpty = (FindObjectsOfType(typeof(PositionUI)) as PositionUI[]).Length;
		//recreate crew and repeat previous line-up
		CreateCrew();
		RepeatLineUp();
		//close any open pop-ups
		_meetingUI.gameObject.SetActive(false);
		_positionUI.ClosePositionPopUp();
	}

	/// <summary>
	/// Get and display the result of the previous race session
	/// </summary>
	private float GetResult(bool isRace, int teamScore, int positions, int offset, Text scoreText, Dictionary<Position, CrewMember> currentPositions = null)
	{
		var expected = 7.5f * positions;
		var scoreDiff = teamScore - expected;
		if (!isRace)
		{
			var timeTaken = TimeSpan.FromSeconds(1800 - ((teamScore - 20) * 10) + offset);
			scoreText.text = string.Format("{0:D2}:{1:D2}", timeTaken.Minutes, timeTaken.Seconds);
		}
		else
		{
			var finishPosition = 1;
			while (teamScore < expected)
			{
				finishPosition++;
				expected -= positions;
			}
			string finishPositionText;
			switch (finishPosition)
			{
				case 1:
					finishPositionText = "1st";
					break;
				case 2:
					finishPositionText = "2nd";
					break;
				case 3:
					finishPositionText = "3rd";
					break;
				default:
					finishPositionText = finishPosition + "th";
					break;
			}
			scoreText.text = "POSITION: " + finishPositionText;
			if (currentPositions != null)
			{
				DisplayPostRacePopUp(currentPositions, finishPosition, finishPositionText);
			}
		}
		return scoreDiff;
	}
}
