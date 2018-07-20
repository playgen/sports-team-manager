using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.SUGAR.Unity;

using Color = UnityEngine.Color;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

/// <summary>
/// Contains all logic related to CrewMember prefabs
/// </summary>
public class CrewMemberUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
	private static readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

	private bool _beingClicked;
	private bool _beingDragged;
	private Vector2 _dragLocalPosition;
	private Vector2 _dragStartPosition;
	private Transform _defaultParent;
	private TrackerTriggerSource _source;
	private PositionUI _currentPlacement;
	private string _sortValue;

	private Image _backImage;
	private Image _borderImage;
	private Button _button;
	private AvatarDisplay _avatarDisplay;
	private Image _positionImage;
	private Button _positionButton;
	private AspectRatioFitter _aspectFitter;

	public CrewMember CrewMember { get; private set; }
	public bool Usable { get; private set; }
	public bool Current { get; private set; }
	private bool ShowEmotion => Usable || (transform.parent.name == "Crew Container" && transform.parent.parent.name == "Viewport");

	private const float _clickedDistance = 15;

	/// <summary>
	/// Bring in elements that need to be known to this object
	/// </summary>
	public void SetUp(bool usable, bool current, CrewMember crewMember, Transform parent, TrackerTriggerSource source = TrackerTriggerSource.TeamManagementScreen)
	{
		CrewMember = crewMember;
		_defaultParent = parent;
		Usable = usable;
		Current = current;
		_source = source;
		_borderImage = GetComponent<Image>();
		_backImage = transform.FindImage("AvatarIcon");
		_button = GetComponent<Button>();
		_avatarDisplay = GetComponentInChildren<AvatarDisplay>();
		_positionImage = transform.FindImage("Position");
		_positionButton = transform.FindButton("Position");
		_aspectFitter = GetComponent<AspectRatioFitter>();

		_backImage.color = Usable ? new Color(0, 1, 1) : Current ? new Color(0, 0.5f, 0.5f) : Color.white;
		_borderImage.color = ShowEmotion ? AvatarDisplay.MoodColor(CrewMember.GetMood()) : Current ? Color.grey : Color.black;
		_button.enabled = Current;
		_aspectFitter.aspectMode = Usable ? AspectRatioFitter.AspectMode.FitInParent : AspectRatioFitter.AspectMode.WidthControlsHeight;

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
		var isEnabled = !string.IsNullOrEmpty(_sortValue) && _currentPlacement == null;
		var sortImage = transform.FindImage("Sort");
		var sortText = transform.FindText("Sort/Sort Text");

		sortImage.enabled = isEnabled;
		sortText.enabled = isEnabled;
		sortText.text = _sortValue;
	}

	public void NotCurrent()
	{
		Current = false;
		_backImage.color = Color.white;
		_borderImage.color = Color.black;
		_button.enabled = false;
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
		_button.enabled = false;
		_dragStartPosition = transform.position;
		_beingDragged = true;
		_beingClicked = true;
		_aspectFitter.aspectMode = AspectRatioFitter.AspectMode.None;
		//_dragLocalPosition is used to offset according to where the click occurred
		_dragLocalPosition = Input.mousePosition - transform.position;
		//set as child of parent many levels up so this displays above all other CrewMember objects
		transform.SetParent(UIManagement.DragCanvas, false);
		transform.position = (Vector2)Input.mousePosition - _dragLocalPosition;
		transform.RectTransform().anchorMin = Vector3.one * 0.5f;
		transform.RectTransform().anchorMax = Vector3.one * 0.5f;
		transform.RectTransform().sizeDelta = _defaultParent.RectTransform().sizeDelta;
		_backImage.color = new Color(0, 0.25f, 0.25f);
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
		if (_beingClicked)
		{
			if (Vector2.Distance(Input.mousePosition, _dragStartPosition + _dragLocalPosition) > _clickedDistance)
			{
				_beingClicked = false;
			}
		}
		if (_beingDragged)
		{
			transform.position = (Vector2)Input.mousePosition - _dragLocalPosition;
			//gets all UI objects below the cursor
			EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, _raycastResults);
			//end drag if mouse up or currently over a blocker
			if (Input.GetMouseButtonUp(0) || _raycastResults.Any(r => r.gameObject.layer == LayerMask.NameToLayer("Blocker")))
			{
				EndDrag();
			}
		}
	}

	public void ForcedMoodChange(string moodChange)
	{
		var mood = AvatarMood.Neutral;

		switch (moodChange)
		{
			case "negative":
				mood = CrewMember.Name.Length % 2 == 0 ? AvatarMood.StronglyDisagree : AvatarMood.Disagree;
				break;
			case "positive":
				mood = CrewMember.Name.Length % 2 == 0 ? AvatarMood.StronglyAgree : AvatarMood.Agree;
				break;
			case "accurate":
				mood = AvatarDisplay.GetMood(CrewMember.GetMood());
				break;
		}
		_avatarDisplay.UpdateMood(CrewMember.Avatar, mood);
		_borderImage.color = ShowEmotion ? AvatarDisplay.MoodColor(mood) : Current ? Color.grey : Color.black;
	}

	/// <summary>
	/// MouseUp ends the current drag. Check if the CrewMember has been placed into a position. If beingClicked is true, the CrewMember pop-up is displayed.
	/// </summary>
	private void EndDrag()
	{
		_backImage.color = new Color(0, 1, 1);
		_button.enabled = true;
		_beingDragged = false;
		if (_beingClicked)
		{
			ShowPopUp();
		}
		else
		{
			CheckPlacement();
		}
		_beingClicked = false;
	}

	/// <summary>
	/// Display the CrewMember pop-up
	/// </summary>
	private void ShowPopUp()
	{
		UIManagement.MemberMeeting.SetUpDisplay(CrewMember, _source.ToString());
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, CrewMember.Name);
		if (Usable)
		{
			transform.SetParent(_currentPlacement?.RectTransform() ?? _defaultParent, false);
			transform.position = transform.parent.position;
		}
	}

	/// <summary>
	/// Check if the drag stopped over a Position UI element.
	/// </summary>
	private void CheckPlacement()
	{
		foreach (var result in _raycastResults)
		{
			var positionUI = result.gameObject.GetComponent<PositionUI>();
			if (positionUI != null)
			{
				var position = positionUI.Position;
				var crewMember = positionUI.CrewMemberUI;
				TrackerEventSender.SendEvent(new TraceEvent("CrewMemberPositioned", TrackerAsset.Verb.Interacted, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.CrewMemberName, CrewMember.Name },
					{ TrackerContextKey.PositionName, position},
					{ TrackerContextKey.PreviousCrewMemberInPosition, crewMember != null ? crewMember.CrewMember.Name : "Null"},
					{ TrackerContextKey.PreviousCrewMemberPosition, _currentPlacement != null ? _currentPlacement.Position : Position.Null}
				}, GameObjectTracker.TrackedGameObject.Npc));
				SUGARManager.GameData.Send("Place Crew Member", CrewMember.Name);
				SUGARManager.GameData.Send("Fill Position", position.ToString());
				Place(positionUI);
				//reset the position and meeting UIs
				UIManagement.PositionDisplay.Display();
				UIManagement.MemberMeeting.Display();
				return;
			}
		}
		//remove this CrewMember from their position if they were in one
		CrewMember.Assign(Position.Null);
		OnReset();
		//reset the position and meeting UIs
		UIManagement.PositionDisplay.Display();
		UIManagement.MemberMeeting.Display();
	}

	/// <summary>
	/// Place the CrewMember to be in-line with the Position it is now paired with
	/// </summary>
	public void Place(PositionUI positionUI, bool swap = false)
	{
		if (!swap && _currentPlacement)
		{
			_currentPlacement.RemoveCrew();
		}
		var currentPosition = positionUI.Position;
		var currentPositionCrew = positionUI.CrewMemberUI;
		var positionTransform = positionUI.RectTransform();
		transform.SetParent(null, false);
		//set size and position
		CrewMember.Assign(currentPosition);
		positionUI.LinkCrew(this);
		if (!swap)
		{
			if (currentPositionCrew != null)
			{
				if (_currentPlacement != null)
				{
					currentPositionCrew.Place(_currentPlacement, true);
				}
				else
				{
					currentPositionCrew.OnReset();
				}
			}
		}
		transform.SetParent(positionTransform, false);
		_aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
		transform.position = positionTransform.position;
		transform.RectTransform().anchoredPosition = Vector2.zero;
		_currentPlacement = positionUI;
		//update current position button
		_positionImage.enabled = true;
		_positionImage.sprite = UIManagement.TeamSelection.RoleIcons[currentPosition.ToString()];
		_positionButton.onClick.RemoveAllListeners();
		_positionButton.onClick.AddListener(() => UIManagement.PositionDisplay.SetUpDisplay(currentPosition, TrackerTriggerSource.CrewMemberPopUp.ToString()));
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, CrewMember.Name);
		SetSortValue(_sortValue);
	}

	/// <summary>
	/// Reset this UI back to its defaults.
	/// </summary>
	public void OnReset()
	{
		//set back to default parent and position
		transform.SetParent(_defaultParent, false);
		transform.position = _defaultParent.position;
		_aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
		transform.SetAsLastSibling();
		if (_dragStartPosition != (Vector2)transform.position)
		{
			TrackerEventSender.SendEvent(new TraceEvent("CrewMemberUnpositioned", TrackerAsset.Verb.Interacted, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.CrewMemberName, CrewMember.Name },
				{ TrackerContextKey.PreviousCrewMemberPosition, _currentPlacement != null ? _currentPlacement.Position : Position.Null}
			}, GameObjectTracker.TrackedGameObject.Npc));
		}
		if (_currentPlacement != null)
		{
			_currentPlacement.RemoveCrew();
			_currentPlacement = null;
		}
		//hide current position button and remove all listeners
		_positionImage.enabled = false;
		_positionButton.onClick.RemoveAllListeners();
		//reset position pop-up if it is currently being shown
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, CrewMember.Name);
		SetSortValue(_sortValue);
	}
}