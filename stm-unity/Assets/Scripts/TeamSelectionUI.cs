using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TeamSelection))]
public class TeamSelectionUI : MonoBehaviour {

	private TeamSelection _teamSelection;
	[SerializeField]
	private GameObject _boatContainer;
	[SerializeField]
	private GameObject _crewContainer;
	[SerializeField]
	private GameObject _boatPrefab;
	[SerializeField]
	private GameObject _positionPrefab;
	[SerializeField]
	private GameObject _lightPrefab;
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
	private GameObject _positionPopUp;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Text[] _positionPopUpText;
	[SerializeField]
	private Image[] _positionPopUpSkills;
	[SerializeField]
	private GameObject _positionPopUpHistoryContainer;
	[SerializeField]
	private GameObject _positionPopUpHistoryPrefab;

	private GameObject _currentBoat;
	private List<GameObject> _boatHistory = new List<GameObject>();
	[SerializeField]
	private int _positionsEmpty;

	[SerializeField]
	private GameObject _recuritmentPopUp;

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
		var boatContainerHeight = _boatContainer.GetComponent<RectTransform>().rect.height * (1f/_teamSelection.GetSessionLength());
		if (_boatHistory.Count > 0)
		{
			boatContainerHeight = _boatHistory[0].GetComponent<RectTransform>().rect.height;
		}
		boatContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, boatContainerHeight);
		boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(boatContainer.GetComponent<RectTransform>().sizeDelta.x * 0.5f, boatContainerHeight * 0.5f);
		boatContainer.name = _boatPrefab.name;
		var stageText = boatContainer.transform.Find("Stage").GetComponent<Text>();
		var stageNumber = _teamSelection.GetStage();
		if (stageNumber == _teamSelection.GetSessionLength()) {
			stageText.text = "RACE DAY!";
		} else
		{
			stageText.text = "P" + stageNumber;
		}
		_boatContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(_boatContainer.GetComponent<RectTransform>().sizeDelta.x, boatContainerHeight * (_boatHistory.Count - (_teamSelection.GetSessionLength() -1)));
		_boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

		for (int i = 0; i < position.Count; i++)
		{
			GameObject positionObject = Instantiate(_positionPrefab);
			positionObject.transform.SetParent(boatContainer.transform.Find("Position Container"), false);
			positionObject.transform.Find("Name").GetComponent<Text>().text = position[i].Name;
			positionObject.name = position[i].Name;
			positionObject.GetComponent<PositionUI>().SetUp(this, position[i]);
		}
		_positionsEmpty = position.Count;
		foreach (var b in _boatHistory)
		{
			b.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, b.GetComponent<RectTransform>().sizeDelta.y);
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
		_raceButton.onClick.AddListener(delegate { ConfirmLineUp(); });
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
			crewMember.GetComponent<CrewMemberUI>().SetUp(_teamSelection, _meetingUI, crew[i], _crewContainer.transform);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(crew[i].Avatar, crew[i].GetMood(), primary, secondary, true);
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
		_raceButton.onClick.RemoveAllListeners();
		_raceButton.GetComponentInChildren<Text>().text = "REPEAT";
		_raceButton.onClick.AddListener(delegate { RepeatLineUp(boat.BoatPositions); });
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

	/// <summary>
	/// Display and set the information for the pop-up for Position details
	/// </summary>
	public void DisplayPositionPopUp(Position position)
	{
		Tracker.T.trackedGameObject.Interacted("Viewed Position Information", GameObjectTracker.TrackedGameObject.GameObject);
		_positionPopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_positionPopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { ClosePositionPopUp(); });
		var currentBoat = _teamSelection.GetBoat();
		var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
		_positionPopUpText[0].text = position.Name;
		_positionPopUpText[1].text = position.Description;
		int raceCount = 1;
		foreach (Transform child in _positionPopUpHistoryContainer.transform)
		{
			Destroy(child.gameObject);
		}
		foreach (Image skill in _positionPopUpSkills)
		{
			skill.enabled = false;
			foreach (CrewMemberSkill actualSkill in position.RequiredSkills)
			{
				if (skill.name == actualSkill.ToString())
				{
					skill.enabled = true;
				}
			}
		}
		foreach (var boat in _teamSelection.GetLineUpHistory())
		{
			BoatPosition[] boatPositions = boat.BoatPositions.Where(bp => bp.Position.Name == position.Name).ToArray();
			if (boatPositions != null)
			{
				foreach (BoatPosition boatPosition in boatPositions)
				{
					GameObject positionHistory = Instantiate(_positionPopUpHistoryPrefab);
					positionHistory.transform.SetParent(_positionPopUpHistoryContainer.transform, false);
					positionHistory.transform.SetAsFirstSibling();
					positionHistory.transform.Find("Member/Name").GetComponent<Text>().text = boatPosition.CrewMember.Name;
					CrewMember positionMember = boatPosition.CrewMember;
					if (_teamSelection.GetBoat().GetAllCrewMembers().Contains(positionMember))
					{
						positionHistory.transform.Find("Member").GetComponent<Button>().onClick.AddListener(delegate { _meetingUI.Display(positionMember); });
					}
					else
					{
						positionHistory.transform.Find("Member").GetComponent<Button>().interactable = false;
					}
					if (raceCount % _teamSelection.GetSessionLength() == 0)
					{
						positionHistory.transform.Find("Session").GetComponent<Text>().text = "RACE DAY!";
					}
					else
					{
						positionHistory.transform.Find("Session").GetComponent<Text>().text = "PRACTICE " + (raceCount % _teamSelection.GetSessionLength());
					}
					positionHistory.GetComponentInChildren<AvatarDisplay>().SetAvatar(boatPosition.CrewMember.Avatar, boatPosition.CrewMember.GetMood(), primary, secondary, true);
				}
			}
			raceCount++;
		}
	}

	/// <summary>
	/// Hide the pop-up for Position details
	/// </summary>
	public void ClosePositionPopUp()
	{
		_positionPopUp.SetActive(false);
		_popUpBlocker.gameObject.SetActive(false);
	}

	public void ChangeBlockerOrder()
	{
		if (_positionPopUp.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			_positionPopUp.transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(delegate { ClosePositionPopUp(); });

		} else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
	}

	public void ResetPositionPopUp()
	{
		if (_positionPopUp.activeSelf)
		{
			DisplayPositionPopUp(_teamSelection.GetBoat().BoatPositions.Select(bp => bp.Position).FirstOrDefault(p => p.Name == _positionPopUpText[0].text));
		}
	}

	/// <summary>
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUp()
	{
		Tracker.T.completable.Completed("Crew Confirmed", CompletableTracker.Completable.Stage);
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			Destroy(position);
		}
		_teamSelection.PostRaceEvent();
		float idealScore = _teamSelection.IdealCheck();
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
			if (crewMember.transform.parent.name == _crewContainer.name)
			{
				Destroy(crewMember.gameObject);
			}
			else
			{
				Destroy(crewMember);
				Destroy(crewMember.GetComponent<EventTrigger>());
			}
		}
		foreach (Button b in _recruitButtons)
		{
			Destroy(b.gameObject);
		}
		_recruitButtons.Clear();

		_raceButton.onClick.RemoveAllListeners();
		_raceButton.GetComponentInChildren<Text>().text = "REPEAT";
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
		_raceButton.onClick.AddListener(() => RepeatLineUp(currentPositions));

		_boatHistory.Add(_currentBoat);
		CreateNewBoat();
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
	}
}
