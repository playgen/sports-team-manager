using System.Linq;

using UnityEngine;
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

	private GameObject _currentBoat;

	void Awake()
	{
		_stateManager = FindObjectOfType(typeof(UIStateManager)) as UIStateManager;
		_teamSelection = GetComponent<TeamSelection>();
	}

	void OnEnable()
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
		var boatContainerHeight = _boatContainer.GetComponent<RectTransform>().rect.height * 0.3f;
		boatContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0, boatContainerHeight);
		boatContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(boatContainer.GetComponent<RectTransform>().sizeDelta.x * 0.5f, boatContainerHeight * 0.5f);
		boatContainer.name = _boatPrefab.name;

		for (int i = 0; i < position.Count; i++)
		{
			GameObject positionObject = Instantiate(_positionPrefab);
			positionObject.transform.SetParent(boatContainer.transform, false);
			var containerHeight = boatContainer.GetComponent<RectTransform>().rect.height * 0.8f;
			positionObject.GetComponent<RectTransform>().sizeDelta = new Vector2(containerHeight, containerHeight);
			positionObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(positionObject.GetComponent<RectTransform>().sizeDelta.x * (0.5f + ((i * 1.05f) - (position.Count * 0.5f))), 0);
			positionObject.transform.Find("Name").GetComponent<Text>().text = position[i].Name;
			positionObject.name = _positionPrefab.name;
			positionObject.GetComponent<PositionUI>().SetUp(_teamSelection, position[i]);
		}
	}
}
