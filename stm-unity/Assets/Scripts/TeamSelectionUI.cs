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
	private GameObject _opinionContainer;
	[SerializeField]
	private GameObject _boatPrefab;
	[SerializeField]
	private GameObject _positionPrefab;
	[SerializeField]
	private GameObject _crewPrefab;
	[SerializeField]
	private GameObject _opinionPrefab;
	[SerializeField]
	private Button _raceButton;
	[SerializeField]
	private Button _recruitButton;
	[SerializeField]
	private GameObject _crewPopUp;
	[SerializeField]
	private Text[] _crewPopUpText;
	[SerializeField]
	private Image[] _crewPopUpBars;
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
	private GameObject _fireWarningPopUp;
	[SerializeField]
	private Button _fireButton;
	[SerializeField]
	private Button _meetingButton;
	private CrewMember _currentDisplayedCrewMember;

	[SerializeField]
	private GameObject _recuritmentPopUp;

	void Awake()
	{
		_fireWarningPopUp.SetActive(false);
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
			stageText.text = "Race\nDay!";
		} else
		{
			stageText.text = "Practice\n" + stageNumber;
		}
		_boatContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(_boatContainer.GetComponent<RectTransform>().sizeDelta.x, boatContainerHeight * (_boatHistory.Count - 2));
		_boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

		for (int i = 0; i < position.Count; i++)
		{
			GameObject positionObject = Instantiate(_positionPrefab);
			positionObject.transform.SetParent(boatContainer.transform.Find("Position Container"), false);
			positionObject.transform.Find("Name").GetComponent<Text>().text = position[i].Name;
			positionObject.name = _positionPrefab.name;
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
		_raceButton.transform.SetParent(boatContainer.transform, false);
		_raceButton.transform.position = new Vector2(_raceButton.transform.position.x, boatContainer.transform.position.y);
		_currentBoat = boatContainer;
		CreateCrew();
	}

	private void CreateCrew()
	{
		var boat = _teamSelection.LoadCrew();
		var crew = boat.GetAllCrewMembers();
		for (int i = 0; i < crew.Count; i++)
		{
			GameObject crewMember = Instantiate(_crewPrefab);
			crewMember.transform.SetParent(_crewContainer.transform, false);
			crewMember.transform.Find("Name").GetComponent<Text>().text = SplitName(crew[i].Name, true);
			crewMember.name = SplitName(crew[i].Name);
			crewMember.GetComponent<CrewMemberUI>().SetUp(_teamSelection, this, crew[i]);
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
				var boatPosition = position.GetName();
				if (boatPosition == boat.BoatPositions[i].Position.Name)
				{
					RectTransform positionTransform = position.gameObject.GetComponent<RectTransform>();
					crewMember.transform.SetParent(positionTransform, false);
					crewMember.GetComponent<RectTransform>().anchorMin = Vector2.zero;
					crewMember.GetComponent<RectTransform>().anchorMax = Vector2.one;
					crewMember.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
					crewMember.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
					position.LinkCrew(crewMember.GetComponent<CrewMemberUI>());
					crewMember.GetComponent<CrewMemberUI>().RevealScore(boat.BoatPositions[i].PositionScore);
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
		_crewPopUp.SetActive(true);
		_crewPopUpText[0].text = "Name: " + SplitName(crewMember.Name);
		_crewPopUpText[1].text = "";
		_crewPopUpText[2].text = "Age: " + crewMember.Age;
		_crewPopUpText[3].text = "Role: " + _teamSelection.GetCrewMemberPosition(crewMember);
		_crewPopUpBars[0].fillAmount = crewMember.RevealedSkills[CrewMemberSkill.Body] * 0.1f;
		_crewPopUpBars[1].fillAmount = crewMember.RevealedSkills[CrewMemberSkill.Charisma] * 0.1f;
		_crewPopUpBars[2].fillAmount = crewMember.RevealedSkills[CrewMemberSkill.Perception] * 0.1f;
		_crewPopUpBars[3].fillAmount = crewMember.RevealedSkills[CrewMemberSkill.Quickness] * 0.1f;
		_crewPopUpBars[4].fillAmount = crewMember.RevealedSkills[CrewMemberSkill.Wisdom] * 0.1f;
		_crewPopUpBars[5].fillAmount = crewMember.RevealedSkills[CrewMemberSkill.Willpower] * 0.1f;
		_currentDisplayedCrewMember = crewMember;
		_fireButton.interactable = true;
		_meetingButton.interactable = true;
		if (_teamSelection.QuestionAllowance() < 2 || _teamSelection.CrewEditAllowance() == 0 || !_teamSelection.CanRemoveCheck())
		{
			_fireButton.interactable = false;
		}
		if (_teamSelection.QuestionAllowance() < 1 && _teamSelection.CrewEditAllowance() > 0 && _teamSelection.CanRemoveCheck())
		{
			_meetingButton.interactable = false;
		}
		foreach (Transform child in _opinionContainer.transform)
		{
			Destroy(child.gameObject);
		}
		foreach (CrewOpinion opinion in crewMember.RevealedCrewOpinions)
		{
			GameObject knownOpinion = Instantiate(_opinionPrefab);
			knownOpinion.transform.SetParent(_opinionContainer.transform, false);
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
		_positionPopUp.SetActive(true);
		_positionPopUpText[0].text = position.Name;
		_positionPopUpText[1].text = "";
		_positionPopUpText[2].text = _teamSelection.GetPositionCrewMember(position);
	}

	/// <summary>
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUp()
	{
		Tracker.T.completable.Completed("Crew Confirmed", CompletableTracker.Completable.Stage);
		var teamScore = _teamSelection.ConfirmLineUp();
		var scoreText = _currentBoat.transform.Find("Score").GetComponent<Text>();
		scoreText.text = teamScore.ToString();
		//float correctCount = _teamSelection.IdealCheck();
		//scoreText.text = correctCount.ToString();
		foreach (var position in FindObjectsOfType(typeof(PositionUI)) as PositionUI[])
		{
			var boatPosition = position.GetName();
			var score = _teamSelection.GetPositionScore(boatPosition);
			position.LockPosition(score);
			Destroy(position);
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
		_boatHistory.Add(_currentBoat);
		_teamSelection.PostRaceEvent();
		CreateNewBoat();
	}

	public void FireCrewWarning()
	{
		Tracker.T.alternative.Selected("Crew Member", "Fire", AlternativeTracker.Alternative.Menu);
		_fireWarningPopUp.SetActive(true);
	}

	public void FireCrew()
	{
		Tracker.T.trackedGameObject.Interacted("Fired Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		_teamSelection.FireCrewMember(_currentDisplayedCrewMember);
		ResetCrew();
	}

	public void ResetCrew()
	{
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			Destroy(crewMember.gameObject);
		}
		CreateCrew();
	}

	public CrewMember GetCurrentCrewMember()
	{
		return _currentDisplayedCrewMember;
	}
}
