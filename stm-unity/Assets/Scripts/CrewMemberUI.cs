using System;
using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrewMemberUI : MonoBehaviour {

	private TeamSelection _teamSelection;
	[SerializeField]
	private Text _scoreText;
	private CrewMember _crewMember;
	private bool _beingDragged;
	private Vector2 _dragPosition;

	private Vector2 _defaultPosition;
	private Vector2 _defaultSize;
	private Transform _defaultParent;
	public event EventHandler ReplacedEvent = delegate { };

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
		_defaultSize = GetComponent<RectTransform>().sizeDelta;
		_defaultPosition = GetComponent<RectTransform>().position;
		_defaultParent = transform.parent;
	}

	public void SetUp(TeamSelection teamSelection, CrewMember crewMember)
	{
		_teamSelection = teamSelection;
		_crewMember = crewMember;
	}

	void BeginDrag()
	{
		_beingDragged = true;
		_dragPosition = Input.mousePosition - transform.position;
		transform.SetParent(_defaultParent, false);
		transform.SetAsLastSibling();
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
		ReplacedEvent(this, new EventArgs());
		_beingDragged = false;
		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, raycastResults);
		bool placed = false;
		foreach (var result in raycastResults)
		{
			if (result.gameObject.name == "Position")
			{
				RectTransform positionTransform = result.gameObject.GetComponent<RectTransform>();
				transform.SetParent(positionTransform, false);
				GetComponent<RectTransform>().sizeDelta = positionTransform.sizeDelta;
				GetComponent<RectTransform>().position = positionTransform.position;
				result.gameObject.GetComponent<PositionUI>().LinkCrew(this);
				_teamSelection.AssignCrew(_crewMember.Name, result.gameObject.GetComponent<PositionUI>().GetName());
				placed = true;
				break;
			}
		}
		if (!placed)
		{
			Reset();
			_teamSelection.RemoveCrew(_crewMember.Name);
		}
	}

	public void Reset()
	{
		transform.SetParent(_defaultParent, false);
		GetComponent<RectTransform>().sizeDelta = _defaultSize;
		GetComponent<RectTransform>().position = _defaultPosition;
	}

	public void RevealScore(int score)
	{
		_scoreText.enabled = true;
		_scoreText.text = score.ToString();
	}
}
