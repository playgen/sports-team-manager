using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
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

#if UNITY_EDITOR
	private List<BoatPosition> _lastCrew = new List<BoatPosition>();
#endif

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
		if ((_teamSelection.QuestionAllowance() < _recruitCost|| _teamSelection.CrewEditAllowance() == 0 || !_teamSelection.CanAddCheck()) && _recruitButtons.Count > 0 && _recruitButtons[0].IsInteractable())
		{
			foreach (Button b in _recruitButtons)
			{
				b.interactable = false;
			}
			
		}
		else if (_teamSelection.QuestionAllowance() >= _recruitCost && _teamSelection.CrewEditAllowance() > 0 && _teamSelection.CanAddCheck() && _recruitButtons.Count > 0 && !_recruitButtons[0].IsInteractable())
		{
			foreach (Button b in _recruitButtons)
			{
				b.interactable = true;
			}
		}
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.R))
		{
			if (_lastCrew.Count > 0)
			{
				RepeatLineUp(_lastCrew);
			}
		}
#endif
	}

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
		string[] splitName = original.Split(' ');
		string lastName = splitName.Last();
		if (!shortName)
		{
			lastName += ", ";
		}
		foreach (string split in splitName)
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
	private GameObject CreateBoat(Boat boat)
	{
		var position = boat.BoatPositions.Select(p => p.Position).ToList();
		GameObject newBoat = Instantiate(_boatPrefab);
		newBoat.transform.SetParent(_boatContainer.transform, false);
		newBoat.name = _boatPrefab.name;
		//required to ensure sizing stays in parallel with UI background
		newBoat.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(_boatContainer.transform.localPosition.y) * 0.2f;
		var stageIcon = newBoat.transform.Find("Stage").GetComponent<Image>();
		var stageNumber = _teamSelection.GetStage();
		stageIcon.sprite = stageNumber == _teamSelection.GetSessionLength() ? _raceIcon : _practiceIcon;
		if (stageNumber != _teamSelection.GetSessionLength()) {
			newBoat.transform.Find("Race").GetComponentInChildren<Text>().text = "PRACTICE " + stageNumber + "/" + (_teamSelection.GetSessionLength() - 1);
			newBoat.transform.Find("Race").GetComponentInChildren<Text>().fontSize = 16;
		}
		foreach (var pos in position)
		{
			GameObject positionObject = Instantiate(_positionPrefab);
			positionObject.transform.SetParent(newBoat.transform.Find("Position Container"), false);
			positionObject.transform.Find("Name").GetComponent<Text>().text = pos.Name;
			positionObject.transform.Find("Image").GetComponent<Image>().sprite = RoleLogos.FirstOrDefault(mo => mo.Name == pos.Name).Image;
			positionObject.name = pos.Name;
			positionObject.GetComponent<PositionUI>().SetUp(this, _positionUI, pos);
		}
		newBoat.transform.Find("Light Container").gameObject.SetActive(false);
		_positionsEmpty = position.Count;
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
		var boat = _teamSelection.GetBoat();
		var boatObject = CreateBoat(boat);
		ChangeVisibleBoats();
		_raceButton = boatObject.transform.Find("Race").GetComponent<Button>();
		if (_teamSelection.GetStage() == _teamSelection.GetSessionLength())
		{
			_raceButton.onClick.AddListener(delegate { ConfirmPopUp(); });
		}
		else
		{
			_raceButton.onClick.AddListener(delegate { ConfirmLineUp(); });
		}
		_currentBoat = boatObject;
		CreateCrew();
		AdjustBoatVisibility(boatObject.GetComponent<CanvasGroup>(), true);
	}

	/// <summary>
	/// Set up new crew objects for a new race session/upon resets for hiring/firing
	/// </summary>
	private void CreateCrew()
	{
		var crew = _teamSelection.LoadCrew();
		List<Transform> sortedCrew = new List<Transform>();
		foreach (var cm in crew)
		{
			GameObject crewMember = CreateCrewMember(cm, _crewContainer.transform, false, true);
			crewMember.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
			crewMember.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
			crewMember.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
			crewMember.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			sortedCrew.Add(crewMember.transform);
			GameObject crewMemberDraggable = CreateCrewMember(cm, crewMember.transform, true, true);
			crewMemberDraggable.transform.position = crewMember.transform.position;
		}
		for (int i = 0; i < _teamSelection.CanAddAmount(); i++)
		{
			GameObject recruit = Instantiate(_recruitPrefab);
			recruit.transform.SetParent(_crewContainer.transform, false);
			recruit.name = "zz Recruit";
			recruit.GetComponent<Button>().onClick.AddListener(delegate { _recruitPopUp.SetActive(true); });
			_recruitButtons.Add(recruit.GetComponent<Button>());
			sortedCrew.Add(recruit.transform);
		}
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
		GameObject crewMember = Instantiate(_crewPrefab);
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
		var oldBoat = CreateBoat(boat);
		var teamScore = boat.BoatPositions.Sum(bp => bp.PositionScore);
		var idealScore = boat.IdealMatchScore;
		var currentCrew = _teamSelection.GetBoat().GetAllCrewMembers();
		List<string> mistakeList = boat.GetAssignmentMistakes(3);
		CreateMistakeIcons(mistakeList, oldBoat, idealScore, boat.BoatPositions.Count);
		_teamSelection.ConfirmLineUp(0, true);
		float scoreDiff = GetResult(_teamSelection.IsRace(), teamScore, boat.BoatPositions.Count, offset, oldBoat.transform.Find("Score").GetComponent<Text>());
		foreach (var bp in boat.BoatPositions)
		{
			if (bp.CrewMember == null)
			{
				continue;
			}
			PositionUI position = oldBoat.GetComponentsInChildren<PositionUI>().FirstOrDefault(p => p.Position == bp.Position);
			if (position == null)
			{
				continue;
			}
			GameObject crewMember = CreateCrewMember(bp.CrewMember, position.transform, false, false);
			crewMember.transform.position = position.transform.position;
			crewMember.GetComponent<CrewMemberUI>().Place(position.gameObject);
			Destroy(crewMember.transform.Find("Opinion").GetComponent<Image>());
			crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(bp.CrewMember.Avatar, scoreDiff * (2f / boat.BoatPositions.Count) + 3);
			if (currentCrew.All(cm => cm.Name != bp.CrewMember.Name))
			{
				Destroy(crewMember.GetComponent<CrewMemberUI>());
			}
			crewMember.transform.SetParent(position.transform.parent, true);
			Destroy(position.gameObject);
			position.name = "Old Position";
		}
		_raceButton = oldBoat.transform.Find("Race").GetComponent<Button>();
		Destroy(_raceButton.gameObject);
	}

	private void CreateMistakeIcons(List<string> mistakes, GameObject boat, float idealScore, int positionCount)
	{
		Transform mistakeParent = boat.transform.Find("Icon Container");
		foreach (string mistake in mistakes)
		{
			GameObject mistakeObject = Instantiate(_mistakePrefab);
			mistakeObject.transform.SetParent(mistakeParent, false);
			mistakeObject.name = mistake;
			Sprite mistakeIcon = _mistakeIcons.FirstOrDefault(mo => mo.Name == mistake).Image;
			mistakeObject.GetComponent<Image>().sprite = mistakeIcon;
		}
		var unideal = positionCount - (int)idealScore - ((idealScore % 1) * 10);
		boat.transform.Find("Light Container").gameObject.SetActive(true);
		boat.transform.Find("Light Container/Green").GetComponentInChildren<Text>().text = ((int)idealScore).ToString();
		boat.transform.Find("Light Container/Yellow").GetComponentInChildren<Text>().text = Mathf.RoundToInt(((idealScore % 1) * 10)).ToString();
		boat.transform.Find("Light Container/Red").GetComponentInChildren<Text>().text = Mathf.RoundToInt(unideal).ToString();
	}

	/// <summary>
	/// Adjust the number of positions on the currentBoat that has not been given a CrewMember
	/// </summary>
	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
	}

	private void ChangeVisibleBoats()
	{
		float currentPositionTop = -_boatContainer.GetComponent<RectTransform>().localPosition.y;
		float currentPositionBottom = -_boatContainer.GetComponent<RectTransform>().anchoredPosition.y;
		foreach (var boat in _boatContainer.GetComponentsInChildren<CanvasGroup>())
		{
			float boatPosition = boat.GetComponent<RectTransform>().localPosition.y;
			if (boatPosition < currentPositionTop && boatPosition > currentPositionBottom)
			{
				AdjustBoatVisibility(boat, true);
			}
			else
			{
				AdjustBoatVisibility(boat, false);
			}
		}
	}

	private void AdjustBoatVisibility(CanvasGroup boat, bool visibility)
	{
		boat.alpha = visibility ? 1 : 0;
		boat.interactable = visibility;
		boat.blocksRaycasts = visibility;
		foreach (var aspect in boat.GetComponentsInChildren<AspectRatioFitter>())
		{
			aspect.enabled = visibility;
		}
		foreach (var layout in boat.GetComponentsInChildren<LayoutGroup>())
		{
			layout.enabled = visibility;
		}
		foreach (var layout in boat.GetComponentsInChildren<LayoutElement>())
		{
			if (layout.gameObject != boat.gameObject)
			{
				layout.enabled = visibility;
			}
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
		_popUpBlocker.onClick.AddListener(delegate { CloseConfirmPopUp(); });
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
		Tracker.T.completable.Completed("Crew Confirmed", CompletableTracker.Completable.Stage);
		List<BoatPosition> currentPositions = new List<BoatPosition>();
		foreach (BoatPosition bp in _teamSelection.GetBoat().BoatPositions)
		{
			if (bp.CrewMember != null)
			{
				currentPositions.Add(new BoatPosition
				{
					Position = bp.Position,
					CrewMember = bp.CrewMember
				});
			}
		}
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			Destroy(position);
		}
		float idealScore = _teamSelection.IdealCheck();
		List<string> mistakeList = _teamSelection.GetAssignmentMistakes(3);
		CreateMistakeIcons(mistakeList, _currentBoat, idealScore, _teamSelection.GetBoat().BoatPositions.Count);
		int offset = UnityEngine.Random.Range(0, 10);
		var teamScore = _teamSelection.ConfirmLineUp(offset);
		float scoreDiff = GetResult(_teamSelection.IsRace(), teamScore, currentPositions.Count, offset, _currentBoat.transform.Find("Score").GetComponent<Text>(), currentPositions);
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			if (crewMember.Current)
			{
				Destroy(crewMember.transform.Find("Opinion").GetComponent<Image>());
				if (crewMember.transform.parent.name == crewMember.name || crewMember.transform.parent.name == _crewContainer.name)
				{
					Destroy(crewMember.gameObject);
				}
				else
				{
					crewMember.GetComponent<CrewMemberUI>().SetUp(false, false, _teamSelection, _meetingUI, _positionUI, crewMember.CrewMember, _crewContainer.transform, _roleIcons);
					crewMember.transform.Find("Position").GetComponent<Button>().onClick.RemoveAllListeners();
					var position = crewMember.transform.parent.GetComponent<PositionUI>().Position;
					crewMember.transform.Find("Position").GetComponent<Button>().onClick.AddListener(delegate { _positionUI.SetUpDisplay(position); });
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(crewMember.CrewMember.Avatar, scoreDiff * (2f / _teamSelection.GetBoat().BoatPositions.Count) + 3);
				}
			}
		}
		foreach (Button b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();
#if UNITY_EDITOR
		_lastCrew = currentPositions;
#endif
		Destroy(_raceButton.gameObject);
		CreateNewBoat();
		if (!_teamSelection.IsRace())
		{
			_positionUI.UpdateDisplay();
			if (_meetingUI.gameObject.activeSelf)
			{
				_meetingUI.Display();
			}
		}
	}

	/// <summary>
	/// Display pop-up which shows the race result
	/// </summary>
	private void DisplayPostRacePopUp(List<BoatPosition> currentPositions, int finishPosition, string finishPositionText)
	{
		_meetingUI.gameObject.SetActive(false);
		_positionUI.ClosePositionPopUp();
		_postRacePopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_postRacePopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { ClosePostRacePopUp(); });
		foreach (Transform child in _postRacePopUp.transform.Find("Crew"))
		{
			Destroy(child.gameObject);
		}
		for (int i = 0; i < currentPositions.Count; i++)
		{
			GameObject memberObject = Instantiate(_postRaceCrewPrefab);
			memberObject.transform.SetParent(_postRacePopUp.transform.Find("Crew"), false);
			memberObject.name = currentPositions[i].CrewMember.Name;
			memberObject.transform.Find("Avatar").GetComponentInChildren<AvatarDisplay>().SetAvatar(currentPositions[i].CrewMember.Avatar, -(finishPosition - 3) * 2);
			memberObject.transform.Find("Position").GetComponent<Image>().sprite = RoleLogos.FirstOrDefault(mo => mo.Name == currentPositions[i].Position.Name).Image;
			memberObject.transform.Find("Position").GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
			if (i % 2 != 0)
			{
				Vector3 currentScale = memberObject.transform.Find("Avatar").localScale;
				memberObject.transform.Find("Avatar").localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
				memberObject.transform.Find("Position").GetComponent<RectTransform>().offsetMin = new Vector2(-10, 0);
			}
		}
		_postRacePopUp.transform.Find("Result").GetComponent<Text>().text = _teamSelection.GetBoat().Name + " finished " + finishPositionText + "!";
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
		}
	}

	/// <summary>
	/// Repeat the line-up (or what can be repeated) from what is passed to it
	/// </summary>
	private void RepeatLineUp(List<BoatPosition> boatPositions)
	{
		List<BoatPosition> tempBoatPositions = new List<BoatPosition>();
		tempBoatPositions.AddRange(boatPositions);
		List<CrewMemberUI> crewMembers = (FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[]).Where(cm => cm.Current && cm.Usable).ToList();
		List<PositionUI> positions = (FindObjectsOfType(typeof(PositionUI)) as PositionUI[]).ToList();
		var sortedPositions = positions.OrderBy(p => p.transform.GetSiblingIndex());
		foreach (var position in sortedPositions)
		{
			BoatPosition boatPosition = tempBoatPositions.FirstOrDefault(bp => bp.Position.Name == position.Position.Name);
			if (boatPosition != null)
			{
				foreach (var crewMember in crewMembers)
				{
					if (crewMember.name == SplitName(boatPosition.CrewMember.Name))
					{
						crewMember.PlacedEvent();
						crewMember.Place(position.gameObject);
						crewMembers.Remove(crewMember);
						tempBoatPositions.Remove(boatPosition);
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Destroy the currently created CrewMember object and adjust any older CrewMember objects where the member has retired/been fired
	/// </summary>
	public void ResetCrew()
	{
		List<BoatPosition> currentPositions = new List<BoatPosition>();
		foreach (BoatPosition bp in _teamSelection.GetBoat().BoatPositions)
		{
			if (bp.CrewMember != null)
			{
				currentPositions.Add(new BoatPosition
				{
					Position = bp.Position,
					CrewMember = bp.CrewMember
				});
			}
		}
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			position.RemoveCrew();
		}
		var currentCrew = _teamSelection.GetBoat().GetAllCrewMembers();
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			if (crewMember.Current)
			{
				Destroy(crewMember.gameObject);
			}
			else
			{
				if (currentCrew.All(cm => cm.Name != crewMember.CrewMember.Name))
				{
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateAvatar(crewMember.CrewMember.Avatar, true);
					Destroy(crewMember);
				}
			}
		}
		foreach (Button b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();
		_positionsEmpty = (FindObjectsOfType(typeof(PositionUI)) as PositionUI[]).Length;
		CreateCrew();
		RepeatLineUp(currentPositions);
		_meetingUI.gameObject.SetActive(false);
		_positionUI.ClosePositionPopUp();
	}

	/// <summary>
	/// Get and display the result of the previous race session
	/// </summary>
	private float GetResult(bool isRace, int teamScore, int positions, int offset, Text scoreText, List<BoatPosition> currentPositions = null)
	{
		float expected = 7.5f * positions;
		if (!isRace)
		{
			TimeSpan timeTaken = TimeSpan.FromSeconds(1800 - ((teamScore - 20) * 10) + offset);
			scoreText.text = string.Format("{0:D2}:{1:D2}", timeTaken.Minutes, timeTaken.Seconds);
		}
		else
		{
			int finishPosition = 1;
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
		return teamScore - expected;
	}
}
