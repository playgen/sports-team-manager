using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrewMemberUI : MonoBehaviour {

	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private Text _scoreText;
	private CrewMember _crewMember;
	private bool _beingDragged;
	private Vector2 _dragPosition;

	void Start()
	{
		_scoreText.enabled = false;
		EventTrigger trigger = GetComponent<EventTrigger>();
		EventTrigger.Entry drag = new EventTrigger.Entry();
		drag.eventID = EventTriggerType.PointerDown;
		drag.callback.AddListener((data) => { BeginDrag(); });
		trigger.triggers.Add(drag);
		EventTrigger.Entry drop = new EventTrigger.Entry();
		drop.eventID = EventTriggerType.PointerUp;
		drop.callback.AddListener((data) => { EndDrag(); });
		trigger.triggers.Add(drop);
	}

	public void SetUp(TeamSelectionUI tsui, CrewMember crewMember)
	{
		_teamSelectionUI = tsui;
		_crewMember = crewMember;
	}

	void BeginDrag()
	{
		_beingDragged = true;
		_dragPosition = Input.mousePosition - transform.position;
	}

	void Update ()
	{
		if (_beingDragged)
		{
			transform.position = (Vector2)Input.mousePosition - _dragPosition;	
		}
	}

	void EndDrag()
	{
		_beingDragged = false;
		GetComponent<CanvasGroup>().blocksRaycasts = false;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit mouseOver;
		if (Physics.Raycast(ray, out mouseOver))
		{
			print("Hit!");
		}
		GetComponent<CanvasGroup>().blocksRaycasts = true;
	}
}
