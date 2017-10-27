using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.SUGAR.Unity;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;

using RAGE.Analytics.Formats;

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
	/// Used to rearrange CrewMember names. shortName set to true results in first initial and last name, set to false results in last names, first names
	/// </summary>
	private string SplitName(string original, bool shortName = true)
	{
		var splitName = original.Split(' ');
		if (shortName)
		{
			var formattedName = splitName.First()[0] + ".";
			foreach (var split in splitName)
			{
				if (split != splitName.First())
				{
					formattedName += split;
					if (split != splitName.Last())
					{
						formattedName += " ";
					}
				}
			}
			return formattedName;
		}
		var firstName = ",\n" + splitName.First();
		var lastName = string.Empty;
		foreach (var split in splitName)
		{
			if (split != splitName.First())
			{
				lastName += split;
				if (split != splitName.Last())
				{
					lastName += " ";
				}
			}
		}
		return lastName + firstName;
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
		var currentCrew = GameManagement.PositionCrew.ContainsKey(position) ? GameManagement.PositionCrew[position] : null;
		var boatPos = GameManagement.PositionString;
		TrackerEventSender.SendEvent(new TraceEvent("PositionPopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.PositionName.ToString(), position.ToString() },
			{ TrackerContextKeys.PositionCrewMember.ToString(), currentCrew != null ? currentCrew.Name : "None" },
			{ TrackerContextKeys.BoatLayout.ToString(), boatPos },
			{ TrackerContextKeys.TriggerUI.ToString(), source },
			{ TrackerContextKeys.SessionsIncludedCount.ToString(), (GameManagement.LineUpHistory.Sum(boat => boat.Positions.Count(pos => pos == position)) + 1).ToString() },
		}, AccessibleTracker.Accessible.Screen));
		SUGARManager.GameData.Send("View Position Screen", position.ToString());
		gameObject.SetActive(true);
		transform.EnableSmallBlocker(() => ClosePositionPopUp(TrackerTriggerSources.PopUpBlocker.ToString()));
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
		if (GameManagement.PositionCrew.ContainsKey(position))
		{
			currentCrew = GameManagement.PositionCrew[position];
		}
		//set title and description text
		_textList[0].text = Localization.Get(position.ToString());
		_textList[1].text = Localization.Get(position + "_DESCRIPTION");
		//set role image (displayed if no-one is in this position)
		_roleImage.sprite = UIManagement.TeamSelection.RoleLogos.First(mo => mo.Name == position.ToString()).Image;
		_currentButton.onClick.RemoveAllListeners();
		//display avatar and CrewMember name accordingly
		_currentAvatar.gameObject.SetActive(currentCrew != null);
		_currentName.gameObject.SetActive(currentCrew != null);
		//set-up avatar, name and onclick handler if CrewMember is in this position
		if (currentCrew != null)
		{
			_currentAvatar.SetAvatar(currentCrew.Avatar, currentCrew.GetMood(), true);
			_currentName.text = currentCrew.Name;
			_currentButton.onClick.AddListener(() => UIManagement.MemberMeeting.SetUpDisplay(currentCrew, TrackerTriggerSources.PositionPopUp.ToString()));
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
		var positionMembers = new Dictionary<CrewMember, int>();
		foreach (var boat in GameManagement.LineUpHistory)
		{
			var positions = boat.Positions.Where(pos => pos == position).ToArray();
			foreach (var pos in positions)
			{
				var positionMember = boat.PositionCrew[pos];
				if (positionMembers.ContainsKey(positionMember))
				{
					positionMembers[positionMember]++;
				}
				else
				{
					positionMembers.Add(positionMember, 1);
				}
			}
		}
		var orderedMembers = positionMembers.OrderByDescending(pm => pm.Value).ThenBy(pm => SplitName(pm.Key.Name, false));
		//for every CrewMember ever placed in this position, create a Position History object displaying their avatar and their number of appearences
		foreach (var member in orderedMembers)
		{
			var positionHistory = Instantiate(_historyPrefab);
			positionHistory.transform.SetParent(_historyContainer.transform, false);
			positionHistory.transform.Find("Name").GetComponent<Text>().text = SplitName(member.Key.Name);
			if (GameManagement.CrewMembers.ContainsKey(member.Key.Name))
			{
				var current = member.Key;
				positionHistory.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => UIManagement.MemberMeeting.SetUpDisplay(current, TrackerTriggerSources.PositionPopUp.ToString()));
			}
			else
			{
				positionHistory.transform.Find("Button").GetComponent<Button>().interactable = false;
			}
			positionHistory.transform.Find("AvatarIcon").GetComponent<Image>().color = UnityEngine.Color.grey;
			positionHistory.GetComponentInChildren<AvatarDisplay>().SetAvatar(member.Key.Avatar, member.Key.GetMood(), true);
			positionHistory.transform.Find("Session Back/Sessions").GetComponent<Text>().text = member.Value.ToString();
		}
	}

	/// <summary>
	/// Hide the pop-up for Position details
	/// </summary>
	public void ClosePositionPopUp(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			TrackerEventSender.SendEvent(new TraceEvent("PositionPopUpClosed", TrackerVerbs.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKeys.PositionName.ToString(), _current.ToString() },
				{ TrackerContextKeys.TriggerUI.ToString(), source }
			}, AccessibleTracker.Accessible.Screen));
			gameObject.SetActive(false);
			if (UIManagement.MemberMeeting.gameObject.activeInHierarchy)
			{
				UIManagement.MemberMeeting.gameObject.transform.EnableSmallBlocker(() => UIManagement.MemberMeeting.CloseCrewMemberPopUp(TrackerTriggerSources.PopUpBlocker.ToString()));
			}
			else
			{
				UIManagement.SmallBlocker.gameObject.SetActive(false);
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
			transform.EnableSmallBlocker(() => ClosePositionPopUp(TrackerTriggerSources.PopUpBlocker.ToString()));
		}
		else if (!transform.parent.GetChild(transform.parent.childCount - 1).gameObject.activeInHierarchy)
		{
			UIManagement.SmallBlocker.gameObject.SetActive(false);
		}
	}

	private void OnLanguageChange()
	{
		_textList[0].text = Localization.Get(_current.ToString());
		_textList[1].text = Localization.Get(_current + "_DESCRIPTION");
	}
}