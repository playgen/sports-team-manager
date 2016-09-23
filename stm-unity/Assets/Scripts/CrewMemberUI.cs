using System;
using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CrewMemberUI : MonoBehaviour {

	private TeamSelection _teamSelection;
	private MemberMeetingUI _meetingUI;
    private PositionDisplayUI _positionUI;
    private CrewMember _crewMember;
	private bool _beingClicked;
	private bool _beingDragged;
	private Vector2 _dragPosition;
	private Icon[] _roleIcons;

	private Transform _defaultParent;
	private Vector2 _currentPositon;
    public bool Usable;
    public bool Current;
	public event EventHandler ReplacedEvent = delegate { };

    public void SetUp(bool usable, bool current, TeamSelection teamSelection, MemberMeetingUI meetingUI, PositionDisplayUI positionUI, CrewMember crewMember, Transform parent, Icon[] roleIcons)
    {
        _teamSelection = teamSelection;
        _meetingUI = meetingUI;
        _positionUI = positionUI;
        _crewMember = crewMember;
        _defaultParent = parent;
        _roleIcons = roleIcons;
        Usable = usable;
        Current = current;
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (_crewMember.restCount <= 0 && Usable)
        {
            SetEventTriggers(trigger, true);
        }
        else
        {
            SetEventTriggers(trigger, false);
        }
    }

    public void SetEventTriggers(EventTrigger trigger, bool isActive)
	{
		trigger.triggers.Clear();
		if (isActive)
		{
			EventTrigger.Entry drag = new EventTrigger.Entry();
			drag.eventID = EventTriggerType.PointerDown;
			drag.callback.AddListener((data) => { BeginDrag(); });
			trigger.triggers.Add(drag);
			EventTrigger.Entry drop = new EventTrigger.Entry();
			drop.eventID = EventTriggerType.PointerUp;
			drop.callback.AddListener((data) => { EndDrag(); });
			trigger.triggers.Add(drop);
		} else
		{
			GetComponentInChildren<Image>().color = Color.gray;
			EventTrigger.Entry click = new EventTrigger.Entry();
			click.eventID = EventTriggerType.PointerClick;
			click.callback.AddListener((data) => { ShowPopUp(); });
			trigger.triggers.Add(click);
		}
	}

	public CrewMember CrewMember()
	{
		return _crewMember;
	}

	/// <summary>
	/// MouseDown start the current drag
	/// </summary>
	void BeginDrag()
	{
		_currentPositon = transform.position;
		_beingDragged = true;
		_beingClicked = true;
		_dragPosition = Input.mousePosition - transform.position;
		transform.SetParent(_defaultParent, false);
		transform.position = (Vector2)Input.mousePosition - _dragPosition;
		transform.SetAsLastSibling();
	}

	/// <summary>
	/// Have this UI element follow the mouse when being dragged, toggle beingClicked to false if dragged too far
	/// </summary>
	void Update ()
	{
		if (_beingDragged)
		{
			transform.position = (Vector2)Input.mousePosition - _dragPosition;	
		}
		if (_beingClicked)
		{
			if (Vector2.Distance(Input.mousePosition, _currentPositon + _dragPosition) > 15)
			{
				_beingClicked = false;
			}
		}
	}

	/// <summary>
	/// MouseUp ends the current drag. If beingClicked is true, the CrewMember pop-up is displayed. Otherwise, check if the Crewmember has been placed into a position.
	/// </summary>
	void EndDrag()
	{
		_beingDragged = false;
		CheckPlacement();
		if (_beingClicked) {
			ShowPopUp();
		}
		_beingClicked = false;
	}

	/// <summary>
	/// Display the CrewMember pop-up and reset the UI position
	/// </summary>
	void ShowPopUp()
	{
		_meetingUI.Display(_crewMember);
	}

	/// <summary>
	/// Check if the drag stopped over a Position UI element.
	/// </summary>
	void CheckPlacement()
	{
		PlacedEvent();
		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, raycastResults);
		bool placed = false;
		foreach (var result in raycastResults)
		{
			if (result.gameObject.GetComponent<PositionUI>())
			{
				Tracker.T.trackedGameObject.Interacted("Positioned Crew Member", GameObjectTracker.TrackedGameObject.Npc);
				Place(result.gameObject);
				placed = true;
				break;
			}
		}
		if (!placed)
		{
            _teamSelection.RemoveCrew(_crewMember);
            Reset();
		}
		if (_meetingUI.gameObject.activeSelf)
		{
			_meetingUI.ResetDisplay();
		}
	}

	public void PlacedEvent()
	{
		ReplacedEvent(this, new EventArgs());
	}

	public void Place(GameObject position)
	{
		RectTransform positionTransform = position.gameObject.GetComponent<RectTransform>();
		transform.SetParent(positionTransform, false);
		GetComponent<RectTransform>().sizeDelta = positionTransform.sizeDelta;
		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -GetComponent<RectTransform>().sizeDelta.y * 0.5f);
		_teamSelection.AssignCrew(_crewMember, position.gameObject.GetComponent<PositionUI>().GetPosition());
		position.gameObject.GetComponent<PositionUI>().LinkCrew(this);
		var positionImage = transform.Find("Position").gameObject;
		positionImage.GetComponent<Image>().enabled = true;
		positionImage.GetComponent<Image>().sprite = _roleIcons.FirstOrDefault(mo => mo.Name == position.gameObject.GetComponent<PositionUI>().GetPosition().Name).Image;
        _positionUI.UpdateDisplay();
        positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
        positionImage.GetComponent<Button>().onClick.AddListener(delegate { _positionUI.Display(position.gameObject.GetComponent<PositionUI>().GetPosition()); });
    }

	/// <summary>
	/// Reset this UI back to its defaults.
	/// </summary>
	public void Reset()
	{
		transform.SetParent(_defaultParent, true);
        transform.position = _defaultParent.position;
        transform.SetAsLastSibling();
		if (_currentPositon != (Vector2)transform.position)
		{
			Tracker.T.trackedGameObject.Interacted("Unpositioned Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		}
		var positionImage = transform.Find("Position").gameObject;
		positionImage.GetComponent<Image>().enabled = false;
        positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
        _positionUI.UpdateDisplay();
    }
}
