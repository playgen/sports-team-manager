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
	private Button _raceButton;
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
	}

	/// <summary>
	/// Instantiate a new UI object for a Boat line-up, adjust container size, position and position of all other similar elements accordingly
	/// </summary>
	public GameObject CreateBoat(Boat boat)
	{
		var position = boat.BoatPositions.Select(p => p.Position).ToList();
		GameObject boatContainer = Instantiate(_boatPrefab);
		boatContainer.transform.SetParent(_boatContainer.transform, false);
		var boatContainerHeight = _boatContainer.GetComponent<RectTransform>().rect.height * 0.3333f;
		if (_boatHistory.Count > 0)
		{
			boatContainerHeight = _boatHistory[0].GetComponent<RectTransform>().rect.height;
		}
		boatContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, boatContainerHeight);
		boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(boatContainer.GetComponent<RectTransform>().sizeDelta.x * 0.5f, boatContainerHeight * 0.5f);
		boatContainer.name = _boatPrefab.name;
		var stageText = boatContainer.transform.Find("Stage").GetComponent<Text>();
		var stageNumber = _teamSelection.GetStage();
		if (stageNumber == 5) {
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
			positionObject.transform.SetParent(boatContainer.transform, false);
			var containerHeight = boatContainer.GetComponent<RectTransform>().rect.height * 0.8f;
			positionObject.GetComponent<RectTransform>().sizeDelta = new Vector2(containerHeight, containerHeight);
			positionObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(positionObject.GetComponent<RectTransform>().sizeDelta.x * (0.5f + ((i * 1.05f) - (position.Count * 0.5f))), 0);
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
		var crew = boat.GetAllCrewMembers();
		for (int i = 0; i < crew.Count; i++)
		{
			GameObject crewMember = Instantiate(_crewPrefab);
			crewMember.transform.SetParent(_crewContainer.transform, false);
			var containerHeight = _crewContainer.GetComponent<RectTransform>().rect.height * 0.8f;
			crewMember.GetComponent<RectTransform>().sizeDelta = new Vector2(containerHeight, containerHeight);
			crewMember.GetComponent<RectTransform>().anchoredPosition = new Vector2((containerHeight * 0.2f) + crewMember.GetComponent<RectTransform>().sizeDelta.x * (0.5f + (i * 1.05f)), 0);
			crewMember.transform.Find("Name").GetComponent<Text>().text = crew[i].Name;
			crewMember.name = _crewPrefab.name;
			crewMember.GetComponent<CrewMemberUI>().SetUp(_teamSelection, this, crew[i]);
		}
		var boatContainer = CreateBoat(boat);
		_raceButton.transform.SetParent(boatContainer.transform, false);
		_raceButton.transform.position = new Vector2(_raceButton.transform.position.x, boatContainer.transform.position.y);
		_currentBoat = boatContainer;
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
		for (int i = 0; i < boat.BoatPositions.Count; i++)
		{
			GameObject crewMember = Instantiate(_crewPrefab);
			var nameText = crewMember.transform.Find("Name").GetComponent<Text>();
			nameText.enabled = false;
			if (boat.BoatPositions[i].CrewMember != null)
			{
				crewMember.transform.Find("Name").GetComponent<Text>().text = boat.BoatPositions[i].CrewMember.Name;
			}
			crewMember.name = _crewPrefab.name;
			foreach (var position in boatContainer.GetComponentsInChildren<PositionUI>())
			{
				var boatPosition = position.GetName();
				if (boatPosition == boat.BoatPositions[i].Position.Name)
				{
					RectTransform positionTransform = position.gameObject.GetComponent<RectTransform>();
					crewMember.transform.SetParent(positionTransform, false);
					crewMember.GetComponent<RectTransform>().sizeDelta = positionTransform.sizeDelta;
					crewMember.GetComponent<RectTransform>().position = positionTransform.position;
					position.LinkCrew(crewMember.GetComponent<CrewMemberUI>());
					crewMember.GetComponent<CrewMemberUI>().RevealScore(boat.BoatPositions[i].PositionScore);
					nameText.enabled = true;
					Destroy(position);
					Destroy(crewMember.GetComponent<CrewMemberUI>());
					Destroy(crewMember.GetComponent<EventTrigger>());
				}
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
		_crewPopUpText[0].text = "Name: " + crewMember.Name;
		_crewPopUpText[1].text = "";
		_crewPopUpText[2].text = "Age: " + crewMember.Age;
		_crewPopUpText[3].text = "Role: " + _teamSelection.GetCrewMemberPosition(crewMember);
		_crewPopUpBars[0].fillAmount = crewMember.Body * 0.1f;
		_crewPopUpBars[1].fillAmount = crewMember.Charisma * 0.1f;
		_crewPopUpBars[2].fillAmount = crewMember.Perception * 0.1f;
		_crewPopUpBars[3].fillAmount = crewMember.Quickness * 0.1f;
		_crewPopUpBars[4].fillAmount = crewMember.Willpower * 0.1f;
		_crewPopUpBars[5].fillAmount = crewMember.Wisdom * 0.1f;
		_crewPopUpBars[6].fillAmount = -crewMember.GetMood() * 0.1f;
		_crewPopUpBars[7].fillAmount = crewMember.GetMood() * 0.1f;
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
		var teamScore = _teamSelection.ConfirmLineUp();
		var scoreText = _currentBoat.transform.Find("Score").GetComponent<Text>();
		scoreText.text = teamScore.ToString();
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
		CreateNewBoat();
	}
}
