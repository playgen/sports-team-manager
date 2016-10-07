using System;
using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains all logic related to CrewMember prefabs
/// </summary>
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
	public CrewMember CrewMember
	{
		get { return _crewMember; }
	}
	public bool Usable;
	public bool Current;
	public event EventHandler ReplacedEvent = delegate { };

	/// <summary>
	/// Bring in elements that need to be known to this class
	/// </summary>
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
		var trigger = GetComponent<EventTrigger>();
		if (_crewMember.RestCount <= 0 && Usable)
		{
			SetEventTriggers(trigger, true);
		}
		else
		{
			SetEventTriggers(trigger, false);
		}
	}

	/// <summary>
	/// Set up event triggers depending on if this should be draggable or not
	/// </summary>
	private void SetEventTriggers(EventTrigger trigger, bool isActive)
	{
		trigger.triggers.Clear();
		if (isActive)
		{
			var drag = new EventTrigger.Entry();
			drag.eventID = EventTriggerType.PointerDown;
			drag.callback.AddListener(data => { BeginDrag(); });
			trigger.triggers.Add(drag);
			var drop = new EventTrigger.Entry();
			drop.eventID = EventTriggerType.PointerUp;
			drop.callback.AddListener(data => { EndDrag(); });
			trigger.triggers.Add(drop);
		} else
		{
			GetComponentInChildren<Image>().color = UnityEngine.Color.gray;
			var click = new EventTrigger.Entry();
			click.eventID = EventTriggerType.PointerClick;
			click.callback.AddListener(data => { ShowPopUp(); });
			trigger.triggers.Add(click);
		}
	}

	/// <summary>
	/// MouseDown start the current drag
	/// </summary>
	private void BeginDrag()
	{
		_currentPositon = transform.position;
		_beingDragged = true;
		_beingClicked = true;
		//_dragPosition is used to offset according to where the click occurred
		_dragPosition = Input.mousePosition - transform.position;
		//disable layoutgroup to avoiding jumping to different position
		_defaultParent.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
		//set as child of container so this displays above all other CrewMember objects
		transform.SetParent(_defaultParent.parent.parent.parent, false);
		transform.position = (Vector2)Input.mousePosition - _dragPosition;
		transform.SetAsLastSibling();
	}

	/// <summary>
	/// Have this UI element follow the mouse when being dragged, toggle beingClicked to false if dragged too far
	/// </summary>
	private void Update ()
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
	/// MouseUp ends the current drag. Check if the CrewMember has been placed into a position.If beingClicked is true, the CrewMember pop-up is displayed.
	/// </summary>
	private void EndDrag()
	{
		_beingDragged = false;
		CheckPlacement();
		if (_beingClicked) {
			ShowPopUp();
		}
		_beingClicked = false;
		_defaultParent.parent.GetComponent<HorizontalLayoutGroup>().enabled = true;
	}

	/// <summary>
	/// Display the CrewMember pop-up and reset the UI position
	/// </summary>
	private void ShowPopUp()
	{
		_meetingUI.SetUpDisplay(_crewMember);
	}

	/// <summary>
	/// Check if the drag stopped over a Position UI element.
	/// </summary>
	private void CheckPlacement()
	{
		PlacedEvent();
		var raycastResults = new List<RaycastResult>();
		//gets all UI objects below the cursor
		EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, raycastResults);
		var placed = false;
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
			_meetingUI.Display();
		}
	}

	/// <summary>
	/// Event triggered when the placement of this object is changed
	/// </summary>
	public void PlacedEvent()
	{
		ReplacedEvent(this, new EventArgs());
	}

	/// <summary>
	/// Place the CrewMember to be in-line with the Position it is now paired with
	/// </summary>
	public void Place(GameObject position, bool historical = false)
	{
		var positionTransform = position.gameObject.GetComponent<RectTransform>();
		transform.SetParent(positionTransform, false);
		GetComponent<RectTransform>().sizeDelta = positionTransform.sizeDelta;
		GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -GetComponent<RectTransform>().sizeDelta.y * 0.5f);
		if (!historical)
		{
			_teamSelection.AssignCrew(_crewMember, position.gameObject.GetComponent<PositionUI>().Position);
		}
		position.gameObject.GetComponent<PositionUI>().LinkCrew(this);
		var positionImage = transform.Find("Position").gameObject;
		positionImage.GetComponent<Image>().enabled = true;
		positionImage.GetComponent<Image>().sprite = _roleIcons.First(mo => mo.Name == position.gameObject.GetComponent<PositionUI>().Position.GetName()).Image;
		_positionUI.UpdateDisplay();
		positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
		var currentPosition = position.gameObject.GetComponent<PositionUI>().Position;
		positionImage.GetComponent<Button>().onClick.AddListener(delegate { _positionUI.SetUpDisplay(currentPosition); });
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
