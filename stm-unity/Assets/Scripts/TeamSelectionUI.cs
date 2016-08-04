using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TeamSelection))]
public class TeamSelectionUI : MonoBehaviour {

	private TeamSelection _teamSelection;
	private UIStateManager _stateManager;

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
	private Scrollbar _scrollbar;

	private GameObject _currentBoat;
	private List<GameObject> _boatHistory = new List<GameObject>();
	private int _positionsEmpty;

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_teamSelection = GetComponent<TeamSelection>();
	}

	void Start()
	{
		CreateBoat();
	}

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

	private void CreateBoat()
	{
		var boat = _teamSelection.LoadCrew();
		var crew = boat.GetAllCrewMembers();
		var position = boat.BoatPositions.Select(p => p.Position).ToList();
		for (int i = 0; i < crew.Count; i++)
		{
			GameObject crewMember = Instantiate(_crewPrefab);
			crewMember.transform.SetParent(_crewContainer.transform, false);
			var containerHeight = _crewContainer.GetComponent<RectTransform>().rect.height * 0.8f;
			crewMember.GetComponent<RectTransform>().sizeDelta = new Vector2(containerHeight, containerHeight);
			crewMember.GetComponent<RectTransform>().anchoredPosition = new Vector2((containerHeight * 0.2f) + crewMember.GetComponent<RectTransform>().sizeDelta.x * (0.5f + (i * 1.05f)), 0);
			crewMember.transform.Find("Name").GetComponent<Text>().text = crew[i].Name;
			crewMember.name = _crewPrefab.name;
			crewMember.GetComponent<CrewMemberUI>().SetUp(_teamSelection, crew[i]);
		}

		GameObject boatContainer = Instantiate(_boatPrefab);
		boatContainer.transform.SetParent(_boatContainer.transform, false);
		var boatContainerHeight = _boatContainer.GetComponent<RectTransform>().rect.height * 0.3333f;
		boatContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, boatContainerHeight);
		boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(boatContainer.GetComponent<RectTransform>().sizeDelta.x * 0.5f, boatContainerHeight * 0.5f);
		boatContainer.name = _boatPrefab.name;
		_raceButton.transform.SetParent(boatContainer.transform, false);
		_raceButton.transform.position = new Vector2(_raceButton.transform.position.x, boatContainer.transform.position.y);
		_currentBoat = boatContainer;

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

		var scrollSize = boatContainerHeight * (_boatHistory.Count + 1);
		_scrollbar.size = Mathf.Abs(_boatContainer.transform.parent.GetComponent<RectTransform>().rect.height) / scrollSize;
	}

	public void Scroll()
	{
		var scrollAmount = -Mathf.Abs(_boatContainer.transform.parent.GetComponent<RectTransform>().rect.height) * ((1 / _scrollbar.size) - 1);
		_boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(_boatContainer.GetComponent<RectTransform>().anchoredPosition.x, -_boatContainer.GetComponent<RectTransform>().sizeDelta.y * 0.5f + (scrollAmount * _scrollbar.value));
	}

	public void PositionChange(int change)
	{
		_positionsEmpty -= change;
	}

	public void ConfirmLineUp()
	{
		_scrollbar.value = 0;
		Scroll();
		var teamScore = _teamSelection.ConfirmLineUp();
		var scoreText = _currentBoat.GetComponentInChildren<Text>();
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

		_currentBoat.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, _currentBoat.GetComponent<RectTransform>().sizeDelta.y);
		foreach (var boat in _boatHistory)
		{
			boat.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, boat.GetComponent<RectTransform>().sizeDelta.y);
		}
		_boatHistory.Add(_currentBoat);
		CreateBoat();
	}
}
