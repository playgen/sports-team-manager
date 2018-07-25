using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;

using TrackerAssetPackage;

/// <summary>
/// Contains all UI logic related to the Position pop-up
/// </summary>
public class PositionDisplayUI : MonoBehaviour
{
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private Text _descriptionText;
	[SerializeField]
	private Image[] _skillImages;
	[SerializeField]
	private AvatarDisplay _currentAvatar;
	[SerializeField]
	private Text _currentName;
	[SerializeField]
	private Button _currentButton;
	[SerializeField]
	private Image _roleImage;
	[SerializeField]
	private GameObject _historyContainer;
	[SerializeField]
	private CrewMemberUI _historyPrefab;

	private Position _currentPosition;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
	}

	/// <summary>
	/// Activate the pop-up and the blocker
	/// </summary>
	public void SetUpDisplay(Position position, string source)
	{
		if (!GameManagement.SeasonOngoing)
		{
			return;
		}
		_currentPosition = position;
		var currentCrew = position.CurrentCrewMember();
		var boatPos = GameManagement.PositionString;
		TrackerEventSender.SendEvent(new TraceEvent("PositionPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.PositionName, position },
			{ TrackerContextKey.PositionCrewMember, currentCrew?.Name ?? "None" },
			{ TrackerContextKey.BoatLayout, boatPos },
			{ TrackerContextKey.TriggerUI, source },
			{ TrackerContextKey.SessionsIncludedCount, position.SessionsIncluded() }
		}, AccessibleTracker.Accessible.Screen));
		SUGARManager.GameData.Send("View Position Screen", position.ToString());
		gameObject.Active(true);
		transform.EnableSmallBlocker(() => ClosePositionPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
		Display();
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	public void Display()
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		var currentCrew = _currentPosition.CurrentCrewMember();
		//set role image (displayed if no-one is in this position)
		_roleImage.sprite = currentCrew == null ? UIManagement.TeamSelection.RoleLogos[_currentPosition.ToString()] : null;
		_currentButton.onClick.RemoveAllListeners();
		//display avatar and CrewMember name accordingly
		_currentAvatar.gameObject.Active(currentCrew != null);
		_currentName.gameObject.Active(currentCrew != null);
		//set-up avatar, name and onclick handler if CrewMember is in this position
		if (currentCrew != null)
		{
			_currentAvatar.SetAvatar(currentCrew.Avatar, currentCrew.GetMood());
			_currentName.text = currentCrew.Name;
			_currentButton.onClick.AddListener(() => UIManagement.MemberMeeting.SetUpDisplay(currentCrew, TrackerTriggerSource.PositionPopUp.ToString()));
		}
		//display skill images for this position
		_skillImages.ToList().ForEach(image => image.enabled = _currentPosition.RequiredSkills().ToList().Any(s => s.ToString() == image.name));
		//wipe previous position history objects
		foreach (Transform child in _historyContainer.transform)
		{
			Destroy(child.gameObject);
		}
		//gather a count of how many times CrewMembers have been placed in this position
		var positionMembers = _currentPosition.Placements();
		//for every CrewMember ever placed in this position, create a Position History object displaying their avatar and their number of appearences
		foreach (var member in positionMembers)
		{
			var positionHistory = Instantiate(_historyPrefab, _historyContainer.transform, false);
			positionHistory.transform.FindText("Name").text = member.Key.FirstInitialLastName();
			positionHistory.GetComponent<CrewMemberUI>().SetUp(false, member.Key, _historyContainer.transform, TrackerTriggerSource.PositionPopUp);
			positionHistory.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
			var mood = member.Key.GetMood();
			positionHistory.GetComponent<Image>().color = AvatarDisplay.MoodColor(mood);
			positionHistory.GetComponentInChildren<AvatarDisplay>().SetAvatar(member.Key.Avatar, mood);
			positionHistory.transform.FindText("Sort/Sort Text").text = member.Value.ToString();
		}
		OnLanguageChange();
	}

	/// <summary>
	/// Hide the pop-up for Position details
	/// </summary>
	public void ClosePositionPopUp(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			TrackerEventSender.SendEvent(new TraceEvent("PositionPopUpClosed", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.PositionName, _currentPosition },
				{ TrackerContextKey.TriggerUI, source }
			}, AccessibleTracker.Accessible.Screen));
			gameObject.Active(false);
			if (UIManagement.MemberMeeting.gameObject.activeInHierarchy)
			{
				UIManagement.MemberMeeting.gameObject.transform.EnableSmallBlocker(() => UIManagement.MemberMeeting.CloseCrewMemberPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
			}
			else
			{
				UIManagement.DisableSmallBlocker();
			}
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Adjust blocker position in hierarchy according to if another pop-up that uses it is displayed as well
	/// </summary>
	public void ChangeBlockerOrder()
	{
		if (gameObject.activeInHierarchy)
		{
			transform.EnableSmallBlocker(() => ClosePositionPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
		}
		else if (!transform.parent.GetChild(transform.parent.childCount - 1).gameObject.activeInHierarchy)
		{
			UIManagement.DisableSmallBlocker();
		}
	}

	/// <summary>
	/// Triggered by button. Displays the Notes UI for the currently selected crew member.
	/// </summary>
	public void DisplayNotes()
	{
		UIManagement.Notes.Display(_currentPosition.ToString());
	}

	private void OnLanguageChange()
	{
		//set title and description text
		_nameText.text = Localization.Get(_currentPosition.ToString());
		_descriptionText.text = Localization.Get(_currentPosition + "_DESCRIPTION");
	}
}