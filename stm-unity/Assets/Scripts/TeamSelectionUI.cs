using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class Icon
{
	public string Name;
	public Sprite Image;
}

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
	[SerializeField]
	private GameObject _opinionPrefab;
	private Button _raceButton;
	[SerializeField]
	private GameObject _recruitPopUp;
	private List<Button> _recruitButtons = new List<Button>();
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private PositionDisplayUI _positionUI;

	private GameObject _currentBoat;
	private List<GameObject> _boatHistory = new List<GameObject>();
	private int _positionsEmpty;
	[SerializeField]
	private GameObject _preRacePopUp;
	[SerializeField]
	private Button _popUpBlocker;

	[SerializeField]
	private Sprite _practiceIcon;
	[SerializeField]
	private Sprite _raceIcon;

#if UNITY_EDITOR
	private List<BoatPosition> _lastCrew = new List<BoatPosition>();
#endif

	void Awake()
	{
		_teamSelection = GetComponent<TeamSelection>();
	}

	/// <summary>
	/// Create UI for all of the previous line-ups and one for the next line-up
	/// </summary>
	void Start()
	{
		foreach (var boat in _teamSelection.GetLineUpHistory())
		{
			CreateHistoricalBoat(boat);
		}
		CreateNewBoat();
	}

	/// <summary>
	/// Toggle the raceButton by whether all the positions for this Boat have been filled or not
	/// </summary>
	void Update()
	{
		if (_positionsEmpty > 0 && _raceButton.interactable)
		{
			_raceButton.interactable = false;
		}
		else if (_positionsEmpty == 0 && !_raceButton.interactable)
		{
			_raceButton.interactable = true;
		}
		if ((_teamSelection.QuestionAllowance() < 2 || _teamSelection.CrewEditAllowance() == 0 || !_teamSelection.CanAddCheck()) && _recruitButtons.Count > 0 && _recruitButtons[0].IsInteractable())
		{
			foreach (Button b in _recruitButtons)
			{
				b.interactable = false;
			}
			
		}
		else if (_teamSelection.QuestionAllowance() >= 2 && _teamSelection.CrewEditAllowance() > 0 && _teamSelection.CanAddCheck() && _recruitButtons.Count > 0 && !_recruitButtons[0].IsInteractable())
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

	string SplitName(string original, bool shortName = false)
	{
		string[] splitName = original.Split(' ');
		string name = splitName.Last();
		if (!shortName)
		{
			name += ", ";
		}
		foreach (string split in splitName)
		{
			if (split != splitName.Last())
			{
				if (!shortName)
				{
					name += split + " ";
				}
				else
				{
					name = split[0] + "." + name;
				}
			}
		}
		if (!shortName)
		{
			name = name.Remove(name.Length - 1, 1);
		}
		return name;
	}

	/// <summary>
	/// Instantiate a new UI object for a Boat line-up, adjust container size, position and position of all other similar elements accordingly
	/// </summary>
	public GameObject CreateBoat(Boat boat)
	{
		var position = boat.BoatPositions.Select(p => p.Position).ToList();
		GameObject boatContainer = Instantiate(_boatPrefab);
		boatContainer.transform.SetParent(_boatContainer.transform, false);
		boatContainer.name = _boatPrefab.name;
		boatContainer.GetComponent<LayoutElement>().preferredHeight = Mathf.Abs(_boatContainer.transform.localPosition.y) * 0.2f;
		var stageIcon = boatContainer.transform.Find("Stage").GetComponent<Image>();
		var stageNumber = _teamSelection.GetStage();
		if (stageNumber == _teamSelection.GetSessionLength()) {
			stageIcon.sprite = _raceIcon;
		} else
		{
			stageIcon.sprite = _practiceIcon;
			boatContainer.transform.Find("Race").GetComponentInChildren<Text>().text = "PRACTICE " + stageNumber + "/" + _teamSelection.GetSessionLength();
			boatContainer.transform.Find("Race").GetComponentInChildren<Text>().fontSize = 16;
		}

		for (int i = 0; i < position.Count; i++)
		{
			GameObject positionObject = Instantiate(_positionPrefab);
			positionObject.transform.SetParent(boatContainer.transform.Find("Position Container"), false);
			positionObject.transform.Find("Name").GetComponent<Text>().text = position[i].Name;
			positionObject.transform.Find("Image").GetComponent<Image>().sprite = RoleLogos.FirstOrDefault(mo => mo.Name == position[i].Name).Image;
			positionObject.name = position[i].Name;
			positionObject.GetComponent<PositionUI>().SetUp(this, _positionUI, position[i]);
		}
		_positionsEmpty = position.Count;
		if (_boatContainer.transform.childCount > _teamSelection.GetSessionLength())
		{
			_boatContainerScroll.numberOfSteps = _boatContainer.transform.childCount - _teamSelection.GetSessionLength() + 1;
		}
		return boatContainer;
	}

	/// <summary>
	/// Instantiate CrewMember UI for a new boat (aka, no line-up) and set the position of the raceButton
	/// </summary>
	private void CreateNewBoat()
	{
		var boat = _teamSelection.LoadCrew();
		var boatContainer = CreateBoat(boat);
		_raceButton = boatContainer.transform.Find("Race").GetComponent<Button>();
		if (_teamSelection.GetStage() == _teamSelection.GetSessionLength())
		{
			_raceButton.onClick.AddListener(delegate { ConfirmPopUp(); });
		}
		else
		{
			_raceButton.onClick.AddListener(delegate { ConfirmLineUp(); });
		}
		_currentBoat = boatContainer;
		CreateCrew();
	}

	private void CreateCrew()
	{
		var boat = _teamSelection.LoadCrew();
		var primary = new Color32((byte)boat.TeamColorsPrimary[0], (byte)boat.TeamColorsPrimary[1], (byte)boat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)boat.TeamColorsSecondary[0], (byte)boat.TeamColorsSecondary[1], (byte)boat.TeamColorsSecondary[2], 255);
		var crew = boat.GetAllCrewMembers();
		for (int i = 0; i < crew.Count; i++)
		{
			GameObject crewMember = Instantiate(_crewPrefab);
			crewMember.transform.SetParent(_crewContainer.transform, false);
			crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(crew[i].Name, true);
			crewMember.name = SplitName(crew[i].Name);
			crewMember.GetComponent<CrewMemberUI>().SetUp(_teamSelection, _meetingUI, crew[i], _crewContainer.transform, _roleIcons);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(crew[i].Avatar, crew[i].GetMood(), primary, secondary, true);
			crewMember.transform.Find("Opinion").GetComponent<Image>().enabled = false;
			crewMember.transform.Find("Position").GetComponent<Image>().enabled = false;
		}
		for (int i = 0; i < _teamSelection.CanAddAmount(); i++)
		{
			GameObject recruit = Instantiate(_recruitPrefab);
			recruit.transform.SetParent(_crewContainer.transform, false);
			recruit.name = "zz Recruit";
			recruit.GetComponent<Button>().onClick.AddListener(delegate { _recruitPopUp.SetActive(true); });
			_recruitButtons.Add(recruit.GetComponent<Button>());
		}
		List<Transform> sortedCrew = new List<Transform>();
		foreach (Transform child in _crewContainer.transform)
		{
			sortedCrew.Add(child);
		}
		sortedCrew = sortedCrew.OrderBy(c => c.name).ToList();
		for (int i = 0; i < sortedCrew.Count; i++)
		{
			sortedCrew[i].SetAsLastSibling();
		}
	}

	/// <summary>
	/// Instantiate and position UI for an existing boat (aka, line-up already selected in the past)
	/// </summary>
	public void CreateHistoricalBoat(Boat boat)
	{
		var boatContainer = CreateBoat(boat);
		var teamScore = boat.BoatPositions.Sum(bp => bp.PositionScore);
		var scoreText = boatContainer.transform.Find("Score").GetComponent<Text>();
		var currentBoat = _teamSelection.GetBoat();
		var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
		var idealScore = boat.IdealMatchScore;
		var unideal = boat.BoatPositions.Count - (int)idealScore - ((idealScore % 1) * 10);
		List<string> mistakeList = boat.GetAssignmentMistakes(3);
		foreach (string mistake in mistakeList)
		{
			GameObject mistakeObject = Instantiate(_mistakePrefab);
			mistakeObject.transform.SetParent(boatContainer.transform.Find("Icon Container"), false);
			mistakeObject.name = mistake;
			Sprite mistakeIcon = _mistakeIcons.FirstOrDefault(mo => mo.Name == mistake).Image;
			mistakeObject.GetComponent<Image>().sprite = mistakeIcon;
		}
		boatContainer.transform.Find("Light Container/Green").GetComponentInChildren<Text>().text = ((int)idealScore).ToString();
		boatContainer.transform.Find("Light Container/Yellow").GetComponentInChildren<Text>().text = Mathf.RoundToInt(((idealScore % 1) * 10)).ToString();
		boatContainer.transform.Find("Light Container/Red").GetComponentInChildren<Text>().text = Mathf.RoundToInt(unideal).ToString();
		boatContainer.transform.Find("Light Container/Green").GetComponent<Image>().color = Color.green;
		boatContainer.transform.Find("Light Container/Yellow").GetComponent<Image>().color = Color.yellow;
		boatContainer.transform.Find("Light Container/Red").GetComponent<Image>().color = Color.red;
		for (int i = 0; i < boat.BoatPositions.Count; i++)
		{
			GameObject crewMember = Instantiate(_crewPrefab);
			var nameText = crewMember.transform.Find("Name").GetComponent<Text>();
			nameText.enabled = false;
			if (boat.BoatPositions[i].CrewMember != null)
			{
				crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(boat.BoatPositions[i].CrewMember.Name, true);
			}
			crewMember.name = _crewPrefab.name;
			foreach (var position in boatContainer.GetComponentsInChildren<PositionUI>())
			{
				var boatPosition = position.GetPosition();
				if (boatPosition == boat.BoatPositions[i].Position)
				{
					RectTransform positionTransform = position.gameObject.GetComponent<RectTransform>();
					crewMember.transform.SetParent(positionTransform, false);
					crewMember.GetComponent<RectTransform>().anchorMin = Vector2.zero;
					crewMember.GetComponent<RectTransform>().anchorMax = Vector2.one;
					crewMember.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
					crewMember.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
					position.LinkCrew(crewMember.GetComponent<CrewMemberUI>());
					Destroy(crewMember.transform.Find("Opinion").GetComponent<Image>());
					var positionImage = crewMember.transform.Find("Position").gameObject;
					positionImage.GetComponent<Image>().enabled = true;
					positionImage.GetComponent<Image>().sprite = _roleIcons.FirstOrDefault(mo => mo.Name == boatPosition.Name).Image;
					positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
					positionImage.GetComponent<Button>().onClick.AddListener(delegate { _positionUI.Display(boatPosition); });
					crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(boat.BoatPositions[i].CrewMember.Avatar, boat.BoatPositions[i].CrewMember.GetMood(), primary, secondary, true);
					nameText.enabled = true;
					Destroy(position);
					Destroy(crewMember.GetComponent<CrewMemberUI>());
					Destroy(crewMember.GetComponent<EventTrigger>());
				}
				position.name = "Old Position";
			}
			if (!nameText.enabled)
			{
				Destroy(crewMember);
			}
		}
		_raceButton = boatContainer.transform.Find("Race").GetComponent<Button>();
		/*_raceButton.onClick.RemoveAllListeners();
		_raceButton.GetComponentInChildren<Text>().text = "REPEAT";
		_raceButton.onClick.AddListener(delegate { RepeatLineUp(boat.BoatPositions); });*/
		Destroy(_raceButton.gameObject);
		_boatHistory.Add(boatContainer);
		_teamSelection.ConfirmLineUp(true);
		if (!_teamSelection.IsRace())
		{
			TimeSpan timeTaken = TimeSpan.FromSeconds((1800 - ((teamScore - 20) * 10)));
			scoreText.text = string.Format("{0:D2}:{1:D2}", timeTaken.Minutes, timeTaken.Seconds);
		}
		else
		{
			int position = 1;
			float expected = 7.5f * boat.BoatPositions.Count;
			while (teamScore < expected)
			{
				position++;
				expected -= boat.BoatPositions.Count;
			}
			scoreText.text = "POSITION: " + position;
		}
	}

	/// <summary>
	/// Adjust the number of positions on the currentBoat that has not been given a CrewMember
	/// </summary>
	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
	}

	void ConfirmPopUp()
	{
		_preRacePopUp.SetActive(true);
		if (_teamSelection.QuestionAllowance() > 0)
		{
			_preRacePopUp.GetComponentInChildren<Text>().text = String.Format("You have {0} Talk Time remaining! If you don't use it before the race, they will be lost.\n\nAre you sure you want to race with this line-up now?", _teamSelection.QuestionAllowance());
		} else
		{
			_preRacePopUp.GetComponentInChildren<Text>().text = "Are you sure you want to race with this line-up now?";
		}
		_popUpBlocker.transform.SetAsLastSibling();
		_preRacePopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { CloseConfirmPopUp(); });
	}

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
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			Destroy(position);
		}
		_teamSelection.PostRaceEvent();
		float idealScore = _teamSelection.IdealCheck();
		List<string> mistakeList = _teamSelection.GetAssignmentMistakes(1);
		foreach (string mistake in mistakeList)
		{
			GameObject mistakeObject = Instantiate(_mistakePrefab);
			mistakeObject.transform.SetParent(_currentBoat.transform.Find("Icon Container"), false);
			mistakeObject.name = mistake;
			Sprite mistakeIcon = _mistakeIcons.FirstOrDefault(mo => mo.Name == mistake).Image;
			mistakeObject.GetComponent<Image>().sprite = mistakeIcon;
		}
		var unideal = _teamSelection.GetBoat().BoatPositions.Count - (int)idealScore - ((idealScore % 1) * 10);
		_currentBoat.transform.Find("Light Container/Green").GetComponentInChildren<Text>().text = ((int)idealScore).ToString();
		_currentBoat.transform.Find("Light Container/Yellow").GetComponentInChildren<Text>().text = Mathf.RoundToInt(((idealScore % 1) * 10)).ToString();
		_currentBoat.transform.Find("Light Container/Red").GetComponentInChildren<Text>().text = Mathf.RoundToInt(unideal).ToString();
		_currentBoat.transform.Find("Light Container/Green").GetComponent<Image>().color = Color.green;
		_currentBoat.transform.Find("Light Container/Yellow").GetComponent<Image>().color = Color.yellow;
		_currentBoat.transform.Find("Light Container/Red").GetComponent<Image>().color = Color.red;
		var teamScore = _teamSelection.ConfirmLineUp();
		var scoreText = _currentBoat.transform.Find("Score").GetComponent<Text>();
		if (!_teamSelection.IsRace())
		{
			TimeSpan timeTaken = TimeSpan.FromSeconds((1800 - ((teamScore - 20) * 10)));
			scoreText.text = string.Format("{0:D2}:{1:D2}", timeTaken.Minutes, timeTaken.Seconds);
		}
		else
		{
			int position = 1;
			float expected = 7.5f * _teamSelection.GetBoat().BoatPositions.Count;
			while (teamScore < expected)
			{
				position++;
				expected -= _teamSelection.GetBoat().BoatPositions.Count;
			}
			scoreText.text = "POSITION: " + position;
		}
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			Destroy(crewMember.transform.Find("Opinion").GetComponent<Image>());
			if (crewMember.transform.parent.name == _crewContainer.name)
			{
				Destroy(crewMember.gameObject);
			}
			else
			{
				Destroy(crewMember);
				Destroy(crewMember.GetComponent<EventTrigger>());
				crewMember.transform.Find("Position").GetComponent<Button>().onClick.RemoveAllListeners();
				var position = crewMember.transform.parent.GetComponent<PositionUI>().GetPosition();
				crewMember.transform.Find("Position").GetComponent<Button>().onClick.AddListener(delegate { _positionUI.Display(position); });
			}
		}
		foreach (Button b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();

		/*_raceButton.onClick.RemoveAllListeners();
		_raceButton.GetComponentInChildren<Text>().text = "REPEAT";
		_raceButton.onClick.AddListener(() => RepeatLineUp(currentPositions));*/
#if UNITY_EDITOR
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
		_lastCrew = currentPositions;
#endif
		Destroy(_raceButton.gameObject);

		_boatHistory.Add(_currentBoat);
		CreateNewBoat();
		_positionUI.UpdateDisplay();
		if (_meetingUI.gameObject.activeSelf)
		{
			_meetingUI.ResetDisplay();
		}
	}

	public void RepeatLineUp(List<BoatPosition> boatPositions, bool buttonTriggered = true)
	{
		List<BoatPosition> tempBoatPositions = new List<BoatPosition>();
		tempBoatPositions.AddRange(boatPositions);
		if (buttonTriggered)
		{
			Tracker.T.alternative.Selected("Old Crew Selection Selected", "Repeat", AlternativeTracker.Alternative.Menu);
		}
		List<CrewMemberUI> crewMembers = (FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[]).ToList();
		List<PositionUI> positions = (FindObjectsOfType(typeof(PositionUI)) as PositionUI[]).ToList();
		var sortedPositions = positions.OrderBy(p => p.transform.GetSiblingIndex());
		foreach (var position in sortedPositions)
		{
			BoatPosition boatPosition = tempBoatPositions.FirstOrDefault(bp => bp.Position.Name == position.GetPosition().Name);
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
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			Destroy(crewMember.gameObject);
		}
		foreach (Button b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();
		_positionsEmpty = (FindObjectsOfType(typeof(PositionUI)) as PositionUI[]).Length;
		CreateCrew();
		RepeatLineUp(currentPositions, false);
		_meetingUI.gameObject.SetActive(false);
		_positionUI.ClosePositionPopUp();
	}
}
