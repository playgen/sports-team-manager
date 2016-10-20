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
	private GameObject _boatMain;
	[SerializeField]
	private List<GameObject> _boatPool;
	[SerializeField]
	private Scrollbar _boatContainerScroll;
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
	private readonly List<Button> _recruitButtons = new List<Button>();
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private PositionDisplayUI _positionUI;
	[SerializeField]
	private PostRaceEventUI _postRaceEventUI;
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
		var setUpCount = 1;
		foreach (var boat in _teamSelection.GetLineUpHistory().Reverse().Take(4).ToList())
		{
			var firstBoat = _boatPool.Last();
			_boatPool.Remove(firstBoat);
			CreateHistoricalBoat(firstBoat, boat.Key, boat.Value, _teamSelection.GetStage() - setUpCount);
			setUpCount++;
			_boatPool.Insert(0, firstBoat);
			firstBoat.transform.SetAsFirstSibling();

		}
		_boatContainerScroll.numberOfSteps = _teamSelection.GetStage() - _teamSelection.GetSessionLength();
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
	}

	/// <summary>
	/// Update the height of boat objects to scale with screen size
	/// </summary>
	private void FixedUpdate()
	{
		var currentPosition = _boatContainer.transform.localPosition.y - _boatContainer.GetComponent<RectTransform>().anchoredPosition.y;
		if (!Mathf.Approximately(_boatMain.GetComponent<LayoutElement>().preferredHeight, Mathf.Abs(currentPosition) * 0.2f))
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
	/// Set up a new boat (aka, one used for positioning and racing)
	/// </summary>
	private void CreateNewBoat()
	{
		var team = _teamSelection.GetTeam();
		var stageIcon = _boatMain.transform.Find("Stage").GetComponent<Image>();
		stageIcon.sprite = _teamSelection.GetStage() % _teamSelection.GetSessionLength() == 0 ? _raceIcon : _practiceIcon;
		_positionsEmpty = team.Boat.Positions.Count;
		_raceButton.onClick.RemoveAllListeners();
		//add click handler to raceButton according to session taking place
		if (_teamSelection.GetStage() % _teamSelection.GetSessionLength() != 0)
		{
			_raceButton.onClick.AddListener(ConfirmLineUp);
			_raceButton.GetComponentInChildren<Text>().text = "PRACTICE " + (_teamSelection.GetStage() % _teamSelection.GetSessionLength()) + "/" + (_teamSelection.GetSessionLength() - 1);
			_raceButton.GetComponentInChildren<Text>().fontSize = 16;
		}
		else
		{
			_raceButton.onClick.AddListener(ConfirmPopUp);
			_raceButton.GetComponentInChildren<Text>().text = "RACE!";
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
			positionObject.transform.Find("Name").GetComponent<Text>().text = pos.GetName();
			positionObject.transform.Find("Image").GetComponent<Image>().sprite = RoleLogos.First(mo => mo.Name == pos.GetName()).Image;
			positionObject.GetComponent<PositionUI>().SetUp(this, _positionUI, pos);
		}
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
			crewMember.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
			crewMember.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
			crewMember.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
			crewMember.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			sortedCrew.Add(crewMember.transform);
			//create the draggable copy of the above
			var crewMemberDraggable = CreateCrewMember(cm, crewMember.transform, true, true);
			crewMemberDraggable.transform.position = crewMember.transform.position;
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
		var isRace = stageNumber % _teamSelection.GetSessionLength() == 0;
		stageIcon.sprite = isRace ? _raceIcon : _practiceIcon;
		var teamScore = boat.Score;
		var idealScore = boat.IdealMatchScore;
		var currentCrew = _teamSelection.GetTeam().CrewMembers;
		//get selection mistakes for this line-up and set-up feedback UI
		var mistakeList = boat.GetAssignmentMistakes(6);
		SetMistakeIcons(mistakeList, oldBoat, idealScore, boat.Positions.Count);
		var scoreDiff = GetResult(isRace, teamScore, boat.Positions.Count, offset, oldBoat.transform.Find("Score").GetComponent<Text>());
		var crewContainer = oldBoat.transform.Find("Crew Container");
		var crewCount = 0;
		//for each position, create a new CrewMember UI object and place accordingly
		foreach (var pair in boat.PositionCrew)
		{
			//create CrewMember UI object for the CrewMember that was in this position
			var crewMember = crewContainer.Find("Crew Member " + crewCount).gameObject;
			crewMember.SetActive(true);
			crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(pair.Value.Name, true);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(pair.Value.Avatar, scoreDiff * (2f / boat.Positions.Count) + 3, true);
			crewMember.GetComponent<CrewMemberUI>().SetUp(false, false, _teamSelection, _meetingUI, _positionUI, pair.Value, crewContainer, _roleIcons);

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
				crewMember.GetComponent<CrewMemberUI>().RemoveEvents();
				crewMember.transform.Find("Name").GetComponent<Text>().color = UnityEngine.Color.grey;
			}
			crewCount++;
		}
		for (int i = crewCount; i < crewContainer.childCount; i++)
		{
			var crewMember = crewContainer.Find("Crew Member " + i).gameObject;
			crewMember.SetActive(false);
		}
	}

	/// <summary>
	/// Create mistake icons and set values for each feedback 'light'
	/// </summary>
	private void SetMistakeIcons(List<string> mistakes, GameObject boat, float idealScore, int positionCount)
	{
		var mistakeParentTop = boat.transform.Find("Icon Container Top");
		var mistakeParentBottom = boat.transform.Find("Icon Container Bottom");
		//create new mistake icon for each mistake
		mistakeParentTop.gameObject.SetActive(true);
		mistakeParentBottom.gameObject.SetActive(true);
		for (int i = 0; i < mistakes.Count; i++)
		{
			var mistakeObjectParent = mistakes.Count / 2 > i ? mistakeParentTop : mistakeParentBottom;
			var mistakeObject = mistakeObjectParent.Find("Ideal Icon " + (mistakes.Count / 2 > i ? i : i - 3)).gameObject;
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
		trans.GetComponent<EventTrigger>().triggers.Clear();
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
	private void ChangeVisibleBoats(bool up, int value)
	{
		
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
		//select random time offset
		var offset = UnityEngine.Random.Range(0, 10);
		//confirm the line-up with the simulation 
		var currentBoat = _teamSelection.ConfirmLineUp(offset);
		var firstBoat = _boatPool.First();
		_boatPool.Remove(firstBoat);
		CreateHistoricalBoat(firstBoat, currentBoat, offset, _teamSelection.GetStage() - 1);
		_boatPool.Add(firstBoat);
		firstBoat.transform.SetSiblingIndex(_boatPool.Count - 1);
		Tracker.T.completable.Completed("Crew Confirmed", CompletableTracker.Completable.Stage, true, currentBoat.Score);
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
					crewMember.RemoveEvents();
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
