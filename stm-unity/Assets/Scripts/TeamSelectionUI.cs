﻿using System;
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
	private GameObject _crewPrefab;
	[SerializeField]
	private GameObject _opinionPrefab;
	private Button _raceButton;
	[SerializeField]
	private Button _recruitButton;
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private GameObject _positionPopUp;
	[SerializeField]
	private Text[] _positionPopUpText;
	[SerializeField]
	private GameObject _positionPopUpHistoryContainer;
	[SerializeField]
	private GameObject _positionPopUpHistoryPrefab;

	private GameObject _currentBoat;
	private List<GameObject> _boatHistory = new List<GameObject>();
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
		if ((_teamSelection.QuestionAllowance() < 2 || _teamSelection.CrewEditAllowance() == 0 || !_teamSelection.CanAddCheck()) && _recruitButton.IsInteractable())
		{
			_recruitButton.interactable = false;
		}
		else if (_teamSelection.QuestionAllowance() >= 2 && _teamSelection.CrewEditAllowance() > 0 && _teamSelection.CanAddCheck() && !_recruitButton.IsInteractable())
		{
			_recruitButton.interactable = true;
		}
	}

	string SplitName(string original, bool newLine = false)
	{
		string[] splitName = original.Split(' ');
		string name = splitName.Last() + ",";
		if (newLine)
		{
			name += "\n";
		} else
		{
			name += " ";
		}
		foreach (string split in splitName)
		{
			if (split != splitName.Last())
			{
				name += split + " ";
			}
		}
		name = name.Remove(name.Length - 1, 1);
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
			stageText.text = "RACE\nDAY!";
		} else
		{
			stageText.text = "PRACTICE\n" + stageNumber;
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
			crewMember.GetComponent<CrewMemberUI>().SetUp(_teamSelection, _meetingUI, crew[i]);
			crewMember.GetComponentInChildren<AvatarDisplay>().SetAvatar(crew[i].Avatar, crew[i].GetMood(), primary, secondary, true);
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
		scoreText.text = teamScore.ToString();
		var currentBoat = _teamSelection.GetBoat();
		var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
		//scoreText.text = boat.IdealMatchScore.ToString();
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
	}

	/// <summary>
	/// Adjust the number of positions on the currentBoat that has not been given a CrewMember
	/// </summary>
	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
	}

	/// <summary>
	/// Display and set the information for the pop-up for CrewMember details
	/// </summary>
	public void DisplayCrewPopUp(CrewMember crewMember)
	{
		foreach (CrewOpinion opinion in crewMember.RevealedCrewOpinions)
		{
			GameObject knownOpinion = Instantiate(_opinionPrefab);
			knownOpinion.transform.Find("Member/Name").GetComponent<Text>().text = SplitName(opinion.Person.Name);
			CrewMember opinionMember = _teamSelection.PersonToCrewMember(opinion.Person);
			if (opinionMember != null)
			{
				knownOpinion.transform.Find("Member").GetComponent<Button>().onClick.AddListener(delegate { DisplayCrewPopUp(opinionMember); });
			} else
			{
				knownOpinion.transform.Find("Member").GetComponent<Button>().interactable = false;
			}
			Image firstLight = knownOpinion.transform.Find("First").GetComponent<Image>();
			Image secondLight = knownOpinion.transform.Find("Second").GetComponent<Image>();
			firstLight.color = Color.yellow;
			secondLight.color = new Color(0, 0, 0, 0);
			if (opinion.Opinion > 1)
			{
				firstLight.color = Color.green;
				if (opinion.Opinion > 3)
				{
					secondLight.color = Color.green;
				}
			}
			if (opinion.Opinion < -1)
			{
				firstLight.color = Color.red;
				if (opinion.Opinion < -3)
				{
					secondLight.color = Color.red;
				}
			}
		}
	}

	/// <summary>
	/// Display and set the information for the pop-up for Position details
	/// </summary>
	public void DisplayPositionPopUp(Position position)
	{
		Tracker.T.trackedGameObject.Interacted("Viewed Position Information", GameObjectTracker.TrackedGameObject.GameObject);
		_positionPopUp.SetActive(true);
		var currentBoat = _teamSelection.GetBoat();
		var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
		_positionPopUpText[0].text = position.Name;
		_positionPopUpText[1].text = "";
		int raceCount = 1;
		foreach (Transform child in _positionPopUpHistoryContainer.transform)
		{
			Destroy(child.gameObject);
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
					positionHistory.transform.Find("Member/Name").GetComponent<Text>().text = SplitName(boatPosition.CrewMember.Name);
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
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUp()
	{
		Tracker.T.completable.Completed("Crew Confirmed", CompletableTracker.Completable.Stage);
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			Destroy(position);
		}
		var teamScore = _teamSelection.ConfirmLineUp();
		var scoreText = _currentBoat.transform.Find("Score").GetComponent<Text>();
		scoreText.text = teamScore.ToString();
		//float correctCount = _teamSelection.IdealCheck();
		//scoreText.text = correctCount.ToString();
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

		_raceButton.onClick.RemoveAllListeners();
		_raceButton.GetComponentInChildren<Text>().text = "Repeat";
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
		_teamSelection.PostRaceEvent();
		CreateNewBoat();
	}

	public void RepeatLineUp(List<BoatPosition> boatPositions)
	{
		List<BoatPosition> tempBoatPositions = new List<BoatPosition>();
		tempBoatPositions.AddRange(boatPositions);
		Tracker.T.alternative.Selected("Old Crew Selection Selected", "Repeat", AlternativeTracker.Alternative.Menu);
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
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			Destroy(crewMember.gameObject);
		}
		_positionsEmpty = (FindObjectsOfType(typeof(PositionUI)) as PositionUI[]).Length;
		CreateCrew();
	}
}
