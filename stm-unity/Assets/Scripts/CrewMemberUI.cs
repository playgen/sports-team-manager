using System;
using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Contains all logic related to CrewMember prefabs
/// </summary>
public class CrewMemberUI : ObservableMonoBehaviour {

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
	public event Action ReplacedEvent = delegate { };

	/// <summary>
	/// Bring in elements that need to be known to this object
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
			var drag = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
			drag.callback.AddListener(data => { BeginDrag(); });
			trigger.triggers.Add(drag);
			var drop = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
			drop.callback.AddListener(data => { EndDrag(); });
			trigger.triggers.Add(drop);
		} else
		{
			GetComponentInChildren<Image>().color = UnityEngine.Color.gray;
			var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
			click.callback.AddListener(data => { ShowPopUp(); });
			trigger.triggers.Add(click);
		}
	}

	public void RemoveEvents()
	{
		GetComponent<EventTrigger>().triggers.Clear();
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
		//set as child of container so this displays above all other CrewMember objects
		transform.SetParent(_defaultParent.parent.parent.parent, false);
		transform.position = (Vector2)Input.mousePosition - _dragPosition;
		//set as last sibling so this always appears in front of other UI objects (except pop-ups)
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
	/// MouseUp ends the current drag. Check if the CrewMember has been placed into a position. If beingClicked is true, the CrewMember pop-up is displayed.
	/// </summary>
	private void EndDrag()
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
	private void ShowPopUp()
	{
		_meetingUI.SetUpDisplay(_crewMember);
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
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
		//reset the meeting UI if it is currently being displayed
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
		ReplacedEvent();
	}

	/// <summary>
	/// Place the CrewMember to be in-line with the Position it is now paired with
	/// </summary>
	public void Place(GameObject position, bool historical = false)
	{
		var positionTransform = (RectTransform)position.gameObject.transform;
		//set size and position
		transform.SetParent(positionTransform, false);
		((RectTransform)transform).sizeDelta = positionTransform.sizeDelta;
		((RectTransform)transform).anchoredPosition = new Vector2(0, -((RectTransform)transform).sizeDelta.y * 0.5f);
		//assign if this is not an historical placement
		if (!historical)
		{
			_teamSelection.AssignCrew(_crewMember, position.gameObject.GetComponent<PositionUI>().Position);
		}
		position.gameObject.GetComponent<PositionUI>().LinkCrew(this);
		var positionImage = transform.Find("Position").gameObject;
		//update current position button
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
		//set back to default parent and position
		transform.SetParent(_defaultParent, true);
		transform.position = _defaultParent.position;
		transform.SetAsLastSibling();
		if (_currentPositon != (Vector2)transform.position)
		{
			Tracker.T.trackedGameObject.Interacted("Unpositioned Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		}
		var positionImage = transform.Find("Position").gameObject;
		//hide current position button and remove all listeners
		positionImage.GetComponent<Image>().enabled = false;
		positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
		//reset position pop-up if it is currently being shown
		_positionUI.UpdateDisplay();
	}
}
