using System;
using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrewMemberUI : MonoBehaviour {

	private TeamSelection _teamSelection;
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private Text _scoreText;
	private CrewMember _crewMember;
	private bool _beingClicked;
	private bool _beingDragged;
	private Vector2 _dragPosition;

	private Transform _defaultParent;
	private Vector2 _currentPositon;
	public event EventHandler ReplacedEvent = delegate { };

	/// <summary>
	/// Get event listeners for click down and up
	/// </summary>
	void Start()
	{
		_scoreText.enabled = false;
		EventTrigger trigger = GetComponent<EventTrigger>();
		if (_crewMember.restCount <= 0)
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
		_defaultParent = transform.parent;
		_currentPositon = transform.position;
	}

	public void SetUp(TeamSelection teamSelection, TeamSelectionUI teamSelectionUI, CrewMember crewMember)
	{
		_teamSelection = teamSelection;
		_teamSelectionUI = teamSelectionUI;
		_crewMember = crewMember;
	}

	/// <summary>
	/// MouseDown start the current drag
	/// </summary>
	void BeginDrag()
	{
		transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
		_beingDragged = true;
		_beingClicked = true;
		_dragPosition = Input.mousePosition - transform.position;
		transform.SetParent(_defaultParent, false);
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
		transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = true;
		_beingDragged = false;
		if (_beingClicked) {
			ShowPopUp();
		} else
		{
			CheckPlacement();
		}
		_beingClicked = false;
	}

	/// <summary>
	/// Display the CrewMember pop-up and reset the UI position
	/// </summary>
	void ShowPopUp()
	{
		_teamSelectionUI.DisplayCrewPopUp(_crewMember);
		GetComponent<RectTransform>().position = _currentPositon;
		if (transform.parent == _defaultParent)
		{
			Reset();
		}
	}

	/// <summary>
	/// Check if the drag stopped over a Position UI element.
	/// </summary>
	void CheckPlacement()
	{
		ReplacedEvent(this, new EventArgs());
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
				GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -GetComponent<RectTransform>().sizeDelta.y * 0.5f);
				_currentPositon = positionTransform.position;
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

	/// <summary>
	/// Reset this UI back to its defaults.
	/// </summary>
	public void Reset()
	{
		transform.SetParent(_defaultParent, true);
		transform.SetAsLastSibling();
		for (int i = 0; i < transform.parent.childCount; i++)
		{
			if (transform.parent.GetChild(i).name != name && String.Compare(name, transform.parent.GetChild(i).name) < 0)
			{
				transform.SetSiblingIndex(transform.parent.GetChild(i).GetSiblingIndex());
				break;
			}
		}
		_currentPositon = transform.position;
	}

	/// <summary>
	/// Display the score for this CrewMember in the position it is in
	/// </summary>
	public void RevealScore(int score)
	{
		_scoreText.enabled = true;
		_scoreText.text = score.ToString();
	}
}
