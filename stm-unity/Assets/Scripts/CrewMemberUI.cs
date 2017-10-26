﻿using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PlayGen.SUGAR.Unity;

using RAGE.Analytics.Formats;

/// <summary>
/// Contains all logic related to CrewMember prefabs
/// </summary>
public class CrewMemberUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
	private MemberMeetingUI _meetingUI;
	private PositionDisplayUI _positionUI;
	private CrewMember _crewMember;
	private bool _beingClicked;
	private bool _beingDragged;
	private Vector2 _dragPosition;
	private Icon[] _roleIcons;
	private Transform _defaultParent;
	private PositionUI _currentPlacement;
	private Vector2 _currentPositon;
	public CrewMember CrewMember
	{
		get { return _crewMember; }
	}
	public bool Usable;
	public bool Current;

	/// <summary>
	/// Bring in elements that need to be known to this object
	/// </summary>
	public void SetUp(bool usable, bool current, CrewMember crewMember, Transform parent, Icon[] roleIcons)
	{
		_meetingUI = transform.root.GetComponentsInChildren<MemberMeetingUI>(true).First();
		_positionUI = transform.root.GetComponentsInChildren<PositionDisplayUI>(true).First();
		_crewMember = crewMember;
		_defaultParent = parent;
		_roleIcons = roleIcons;
		Usable = usable;
		Current = current;
		transform.Find("AvatarIcon").GetComponent<Image>().color = Usable ? UnityEngine.Color.white : UnityEngine.Color.grey;
	}

	/// <summary>
	/// When this object is clicked or tapped
	/// </summary>
	public void OnPointerDown(PointerEventData eventData)
	{
		if (_crewMember.RestCount <= 0 && Usable && Current)
		{
			BeginDrag();
		}
	}

	/// <summary>
	/// When this object is clicked or tapped
	/// </summary>
	public void OnPointerClick(PointerEventData eventData)
	{
		if ((_crewMember.RestCount > 0 || !Usable) && Current)
		{
			ShowPopUp();
		}
	}

	/// <summary>
	/// Start the current drag
	/// </summary>
	private void BeginDrag()
	{
		_currentPositon = transform.position;
		_beingDragged = true;
		_beingClicked = true;
		//_dragPosition is used to offset according to where the click occurred
		_dragPosition = Input.mousePosition - transform.position;
		//set as child of parent many levels up so this displays above all other CrewMember objects
		transform.SetParent(transform.root, false);
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
			var raycastResults = new List<RaycastResult>();
			//gets all UI objects below the cursor
			EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, raycastResults);
			if (Input.GetMouseButtonUp(0))
			{
				EndDrag(raycastResults);
			}
			//end drag if currently under a blocker
			foreach (var result in raycastResults)
			{
				if (result.gameObject.layer == 8)
				{
					EndDrag(raycastResults);
				}
			}
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
	private void EndDrag(List<RaycastResult> raycastResults)
	{
		_beingDragged = false;
		CheckPlacement(raycastResults);
		if (_beingClicked) {
			ShowPopUp();
		}
		_beingClicked = false;
	}

	/// <summary>
	/// Display the CrewMember pop-up
	/// </summary>
	private void ShowPopUp()
	{
		_meetingUI.SetUpDisplay(_crewMember, TrackerTriggerSources.TeamManagementScreen.ToString());
		TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _crewMember.Name);
	}

	/// <summary>
	/// Check if the drag stopped over a Position UI element.
	/// </summary>
	private void CheckPlacement(List<RaycastResult> raycastResults)
	{
		foreach (var result in raycastResults)
		{
			if (result.gameObject.GetComponent<PositionUI>())
			{
				var pos = result.gameObject.GetComponent<PositionUI>().Position;
				var crewMember = result.gameObject.GetComponent<PositionUI>().CrewMemberUI;
				TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPositioned", TrackerVerbs.Interacted, new Dictionary<string, string>
				{
					{ TrackerContextKeys.CrewMemberName.ToString(), _crewMember.Name },
					{ TrackerContextKeys.PositionName.ToString(), pos.ToString()},
					{ TrackerContextKeys.PreviousCrewMemberInPosition.ToString(), crewMember != null ? crewMember.CrewMember.Name : "Null"},
					{ TrackerContextKeys.PreviousCrewMemberPosition.ToString(), _currentPlacement != null ? _currentPlacement.Position.ToString() : Position.Null.ToString()},
				}, GameObjectTracker.TrackedGameObject.Npc));
				SUGARManager.GameData.Send("Place Crew Member", _crewMember.Name);
				SUGARManager.GameData.Send("Fill Position", pos.ToString());
				Place(result.gameObject);
				break;
			}
			if (result.Equals(raycastResults.Last()))
			{
				//remove this CrewMember from their position if they were in one
				GameManagement.Boat.AssignCrewMember(0, _crewMember);
				OnReset();
			}
		}
		//reset the meeting UI if it is currently being displayed
		if (_meetingUI.gameObject.activeInHierarchy)
		{
			_meetingUI.Display();
		}
	}

	/// <summary>
	/// Place the CrewMember to be in-line with the Position it is now paired with
	/// </summary>
	public void Place(GameObject position, bool swap = false)
	{
		if (!swap && _currentPlacement)
		{
			_currentPlacement.RemoveCrew();
		}
		var positionComp = position.gameObject.GetComponent<PositionUI>();
		var currentPosition = positionComp.Position;
		var currentPositionCrew = positionComp.CrewMemberUI;
		var positionTransform = (RectTransform)position.gameObject.transform;
		//set size and position
		transform.SetParent(positionTransform, false);
		((RectTransform)transform).sizeDelta = positionTransform.sizeDelta;
		((RectTransform)transform).anchoredPosition = new Vector2(0, -((RectTransform)transform).sizeDelta.y * 0.5f);
		GameManagement.Boat.AssignCrewMember(currentPosition, _crewMember);
		positionComp.LinkCrew(this);
		if (!swap)
		{
			if (currentPositionCrew != null)
			{
				if (_currentPlacement != null)
				{
					var currentPos = _currentPlacement.gameObject;
					currentPositionCrew.Place(currentPos, true);
				}
				else
				{
					currentPositionCrew.OnReset();
				}
			}
		}
		_currentPlacement = positionComp;
		var positionImage = transform.Find("Position").gameObject;
		//update current position button
		positionImage.GetComponent<Image>().enabled = true;
		positionImage.GetComponent<Image>().sprite = _roleIcons.First(mo => mo.Name == currentPosition.ToString()).Image;
		_positionUI.UpdateDisplay();
		positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
		positionImage.GetComponent<Button>().onClick.AddListener(() => _positionUI.SetUpDisplay(currentPosition, TrackerTriggerSources.CrewMemberPopUp.ToString()));
		TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _crewMember.Name);
	}

	/// <summary>
	/// Reset this UI back to its defaults.
	/// </summary>
	public void OnReset()
	{
		//set back to default parent and position
		transform.SetParent(_defaultParent, true);
		transform.position = _defaultParent.position;
		transform.SetAsLastSibling();
		if (_currentPositon != (Vector2)transform.position)
		{
			TrackerEventSender.SendEvent(new TraceEvent("CrewMemberUnpositioned", TrackerVerbs.Interacted, new Dictionary<string, string>
			{
				{ TrackerContextKeys.CrewMemberName.ToString(), _crewMember.Name },
				{ TrackerContextKeys.PreviousCrewMemberPosition.ToString(), _currentPlacement != null ? _currentPlacement.Position.ToString() : Position.Null.ToString()},
			}, GameObjectTracker.TrackedGameObject.Npc));
		}
		if (_currentPlacement != null)
		{
			_currentPlacement.RemoveCrew();
			_currentPlacement = null;
		}
		var positionImage = transform.Find("Position").gameObject;
		//hide current position button and remove all listeners
		positionImage.GetComponent<Image>().enabled = false;
		positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
		//reset position pop-up if it is currently being shown
		_positionUI.UpdateDisplay();
		TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _crewMember.Name);
	}
}