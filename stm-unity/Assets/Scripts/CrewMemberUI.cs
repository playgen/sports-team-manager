using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts;
using PlayGen.SUGAR.Unity;

using Color = UnityEngine.Color;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

/// <summary>
/// Contains all logic related to CrewMember prefabs
/// </summary>
public class CrewMemberUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
	private bool _beingClicked;
	private bool _beingDragged;
	private Vector2 _dragPosition;
	private Transform _defaultParent;
	private PositionUI _currentPlacement;
	private Vector2 _currentPositon;
	private string _sortValue;

	public CrewMember CrewMember { get; private set; }
	public bool Usable { get; private set; }
	public bool Current { get; private set; }

	private const float _clickedDistance = 15;

	/// <summary>
	/// Bring in elements that need to be known to this object
	/// </summary>
	public void SetUp(bool usable, bool current, CrewMember crewMember, Transform parent)
	{
		CrewMember = crewMember;
		_defaultParent = parent;
		Usable = usable;
		Current = current;
		transform.FindImage("AvatarIcon").color = 
			Usable ? 
				new Color(0, 1, 1) 
				: Current ? 
					new Color(0, 0.5f, 0.5f) 
					: Color.white;

		GetComponent<Image>().color = 
			Usable || (transform.parent.name == "Crew Container" && transform.parent.parent.name == "Viewport") ?
				AvatarDisplay.MoodColor(AvatarMoodConfig.GetMood(CrewMember.GetMood())) 
				: Current ? 
					Color.grey 
					: Color.black;

		GetComponent<Button>().enabled = Current;
		if (!GameManagement.SeasonOngoing)
		{
			foreach (var button in GetComponentsInChildren<Button>())
			{
				button.enabled = false;
			}
		}
	}

	public void SetSortValue(string value)
	{
		_sortValue = value;
		var isEnabled = !string.IsNullOrEmpty(_sortValue) && transform.parent == _defaultParent;
		var sortImage = transform.FindImage("Sort");
		var sortText = transform.FindText("Sort/Sort Text");

		sortImage.enabled = isEnabled;
		sortText.enabled = isEnabled;
		sortText.text = _sortValue;
	}

	public void NotCurrent()
	{
		Current = false;
		transform.FindImage("AvatarIcon").color = Color.white;
		GetComponent<Button>().enabled = false;
	}

	/// <summary>
	/// When this object is clicked or tapped
	/// </summary>
	public void OnPointerDown(PointerEventData eventData)
	{
		if (CrewMember.RestCount <= 0 && Usable && Current)
		{
			BeginDrag();
		}
	}

	/// <summary>
	/// When this object is clicked or tapped
	/// </summary>
	public void OnPointerClick(PointerEventData eventData)
	{
		if ((CrewMember.RestCount > 0 || !Usable) && Current)
		{
			ShowPopUp();
		}
	}

	/// <summary>
	/// Start the current drag
	/// </summary>
	private void BeginDrag()
	{
		GetComponent<Button>().enabled = false;
		_currentPositon = transform.position;
		_beingDragged = true;
		_beingClicked = true;
		//_dragPosition is used to offset according to where the click occurred
		_dragPosition = Input.mousePosition - transform.position;
		//set as child of parent many levels up so this displays above all other CrewMember objects
		transform.SetParent(GameObject.Find("Drag Canvas").transform, false);
		transform.position = (Vector2)Input.mousePosition - _dragPosition;
		//set as last sibling so this always appears in front of other UI objects (except pop-ups)
		transform.SetAsLastSibling();
		transform.FindImage("AvatarIcon").color = new Color(0, 0.25f, 0.25f);
	}

	/// <summary>
	/// Have this UI element follow the mouse when being dragged, toggle beingClicked to false if dragged too far
	/// </summary>
	private void Update ()
	{
#if UNITY_EDITOR
		if (Input.GetKeyDown("1"))
		{
			ForcedMoodChange("negative");
		}
		if (Input.GetKeyDown("2"))
		{
			ForcedMoodChange("neutral");
		}
		if (Input.GetKeyDown("3"))
		{
			ForcedMoodChange("positive");
		}
		if (Input.GetKeyDown("4"))
		{
			ForcedMoodChange("accurate");
		}
#endif
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
			if (Vector2.Distance(Input.mousePosition, _currentPositon + _dragPosition) > _clickedDistance)
			{
				_beingClicked = false;
			}
		}
	}

	public void ForcedMoodChange(string moodChange)
	{
		var usable  = Usable || (transform.parent.name == "Crew Container" && transform.parent.parent.name == "Viewport");
		string mood;


		switch (moodChange)
		{
			case "negative":
				mood = CrewMember.Name.Length % 2 == 0 ? "StronglyDisagree" : "Disagree";
				break;
			case "positive":
				mood = CrewMember.Name.Length % 2 == 0 ? "StronglyAgree" : "Agree";
				break;
			case "accurate":
				mood = AvatarMoodConfig.GetMood(CrewMember.GetMood());
				break;
			default:
				mood = "Neutral";
				break;
		}
		GetComponentInChildren<AvatarDisplay>().UpdateMood(CrewMember.Avatar, mood);
		GetComponent<Image>().color = Usable || usable ? 
			AvatarDisplay.MoodColor(mood) 
			: Current ? 
				Color.grey 
				: Color.black;
	}

	/// <summary>
	/// MouseUp ends the current drag. Check if the CrewMember has been placed into a position. If beingClicked is true, the CrewMember pop-up is displayed.
	/// </summary>
	private void EndDrag(List<RaycastResult> raycastResults)
	{
		GetComponent<Button>().enabled = true;
		_beingDragged = false;
		CheckPlacement(raycastResults);
		if (_beingClicked) {
			ShowPopUp();
		}
		_beingClicked = false;
		transform.FindImage("AvatarIcon").color = new Color(0, 1, 1);
	}

	/// <summary>
	/// Display the CrewMember pop-up
	/// </summary>
	private void ShowPopUp()
	{
		UIManagement.MemberMeeting.SetUpDisplay(CrewMember, TrackerTriggerSource.TeamManagementScreen.ToString());
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, CrewMember.Name);
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
				TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPositioned", TrackerAsset.Verb.Interacted, new Dictionary<string, string>
				{
					{ TrackerContextKey.CrewMemberName.ToString(), CrewMember.Name },
					{ TrackerContextKey.PositionName.ToString(), pos.ToString()},
					{ TrackerContextKey.PreviousCrewMemberInPosition.ToString(), crewMember != null ? crewMember.CrewMember.Name : "Null"},
					{ TrackerContextKey.PreviousCrewMemberPosition.ToString(), _currentPlacement != null ? _currentPlacement.Position.ToString() : Position.Null.ToString()}
				}, GameObjectTracker.TrackedGameObject.Npc));
				SUGARManager.GameData.Send("Place Crew Member", CrewMember.Name);
				SUGARManager.GameData.Send("Fill Position", pos.ToString());
				Place(result.gameObject);
				break;
			}
			if (result.Equals(raycastResults.Last()))
			{
				//remove this CrewMember from their position if they were in one
				GameManagement.Boat.AssignCrewMember(0, CrewMember);
				OnReset();
			}
		}
		if (raycastResults.Count == 0)
		{
			//remove this CrewMember from their position if they were in one
			GameManagement.Boat.AssignCrewMember(0, CrewMember);
			OnReset();
		}
		//reset the meeting UI
		UIManagement.MemberMeeting.Display();
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
		var positionTransform = position.RectTransform();
		//set size and position
		transform.SetParent(positionTransform, false);
		transform.RectTransform().sizeDelta = positionTransform.sizeDelta;
		transform.RectTransform().anchoredPosition = new Vector2(0, -transform.RectTransform().sizeDelta.y * 0.5f);
		GameManagement.Boat.AssignCrewMember(currentPosition, CrewMember);
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
		var positionImage = transform.FindObject("Position");
		//update current position button
		positionImage.GetComponent<Image>().enabled = true;
		positionImage.GetComponent<Image>().sprite = UIManagement.TeamSelection.RoleIcons.First(mo => mo.Name == currentPosition.ToString()).Image;
		UIManagement.PositionDisplay.UpdateDisplay();
		positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
		positionImage.GetComponent<Button>().onClick.AddListener(() => UIManagement.PositionDisplay.SetUpDisplay(currentPosition, TrackerTriggerSource.CrewMemberPopUp.ToString()));
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, CrewMember.Name);
		SetSortValue(_sortValue);
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
			TrackerEventSender.SendEvent(new TraceEvent("CrewMemberUnpositioned", TrackerAsset.Verb.Interacted, new Dictionary<string, string>
			{
				{ TrackerContextKey.CrewMemberName.ToString(), CrewMember.Name },
				{ TrackerContextKey.PreviousCrewMemberPosition.ToString(), _currentPlacement != null ? _currentPlacement.Position.ToString() : Position.Null.ToString()}
			}, GameObjectTracker.TrackedGameObject.Npc));
		}
		if (_currentPlacement != null)
		{
			_currentPlacement.RemoveCrew();
			_currentPlacement = null;
		}
		var positionImage = transform.FindObject("Position");
		//hide current position button and remove all listeners
		positionImage.GetComponent<Image>().enabled = false;
		positionImage.GetComponent<Button>().onClick.RemoveAllListeners();
		//reset position pop-up if it is currently being shown
		UIManagement.PositionDisplay.UpdateDisplay();
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, CrewMember.Name);
		SetSortValue(_sortValue);
	}
}