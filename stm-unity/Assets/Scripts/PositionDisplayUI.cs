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
	private Text[] _textList;
	[SerializeField]
	private Image[] _skillImages;
	[SerializeField]
	private AvatarDisplay _currentAvatar;
	[SerializeField]
	private Text _currentName;
	[SerializeField]
	private Button _currentButton;
	[SerializeField]
	private Button _notesButton;
	[SerializeField]
	private Image _roleImage;
	[SerializeField]
	private GameObject _historyContainer;
	[SerializeField]
	private GameObject _historyPrefab;

	private Position _current;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
	}

	/// <summary>
	/// If the pop-up is already active, update UI elements to match current information
	/// </summary>
	public void UpdateDisplay()
	{
		if (gameObject.activeInHierarchy)
		{
			Display(_current);
		}
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
		var currentCrew = position.Current() ? position.CurrentCrewMember() : null;
		var boatPos = GameManagement.PositionString;
		TrackerEventSender.SendEvent(new TraceEvent("PositionPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKey.PositionName.ToString(), position.ToString() },
			{ TrackerContextKey.PositionCrewMember.ToString(), currentCrew != null ? currentCrew.Name : "None" },
			{ TrackerContextKey.BoatLayout.ToString(), boatPos },
			{ TrackerContextKey.TriggerUI.ToString(), source },
			{ TrackerContextKey.SessionsIncludedCount.ToString(), position.SessionsIncluded().ToString() }
		}, AccessibleTracker.Accessible.Screen));
		SUGARManager.GameData.Send("View Position Screen", position.ToString());
		gameObject.Active(true);
		transform.EnableSmallBlocker(() => ClosePositionPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
		Display(position);
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	private void Display(Position position)
	{
		_current = position;
		CrewMember currentCrew = null;
		//get current CrewMember in this position if any
		if (position.Current())
		{
			currentCrew = position.CurrentCrewMember();
		}
		//set title and description text
		_textList[0].text = Localization.Get(position.ToString());
		_textList[1].text = Localization.Get(position + "_DESCRIPTION");
		//set role image (displayed if no-one is in this position)
		_roleImage.sprite = UIManagement.TeamSelection.RoleLogos.First(mo => mo.Name == position.ToString()).Image;
		_notesButton.onClick.RemoveAllListeners();
		_notesButton.onClick.AddListener(() => UIManagement.Notes.Display(position.ToString()));
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
		//wipe previous position history objects
		foreach (Transform child in _historyContainer.transform)
		{
			Destroy(child.gameObject);
		}
		//display skill images for this position
		foreach (var skill in _skillImages)
		{
			skill.enabled = false;
			foreach (var actualSkill in position.RequiredSkills())
			{
				if (skill.name == actualSkill.ToString())
				{
					skill.enabled = true;
				}
			}
		}
		//gather a count of how many times CrewMembers have been placed in this position
		var positionMembers = position.Placements();
		//for every CrewMember ever placed in this position, create a Position History object displaying their avatar and their number of appearences
		foreach (var member in positionMembers)
		{
			var positionHistory = Instantiate(_historyPrefab);
			positionHistory.transform.SetParent(_historyContainer.transform, false);
			positionHistory.transform.FindText("Name").text = member.Key.FirstName[0] + "." + member.Key.LastName;
			if (member.Key.Current())
			{
				var current = member.Key;
				positionHistory.GetComponentInChildren<Button>().onClick.AddListener(() => UIManagement.MemberMeeting.SetUpDisplay(current, TrackerTriggerSource.PositionPopUp.ToString()));
				positionHistory.transform.FindImage("AvatarIcon").color = new UnityEngine.Color(0, 0.5f, 0.5f);
			}
			else
			{
				positionHistory.GetComponentInChildren<Button>().interactable = false;
				positionHistory.transform.FindImage("AvatarIcon").color = UnityEngine.Color.white;
			}
			positionHistory.GetComponentInChildren<AvatarDisplay>().SetAvatar(member.Key.Avatar, member.Key.GetMood());
			positionHistory.transform.FindText("Session Back/Sessions").text = member.Value.ToString();
		}
	}

	/// <summary>
	/// Hide the pop-up for Position details
	/// </summary>
	public void ClosePositionPopUp(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			TrackerEventSender.SendEvent(new TraceEvent("PositionPopUpClosed", TrackerAsset.Verb.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKey.PositionName.ToString(), _current.ToString() },
				{ TrackerContextKey.TriggerUI.ToString(), source }
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

	private void OnLanguageChange()
	{
		_textList[0].text = Localization.Get(_current.ToString());
		_textList[1].text = Localization.Get(_current + "_DESCRIPTION");
	}
}