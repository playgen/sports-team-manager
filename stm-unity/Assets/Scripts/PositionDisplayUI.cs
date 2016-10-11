using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains all UI logic related to the Position pop-up
/// </summary>
[RequireComponent(typeof(PositionDisplay))]
public class PositionDisplayUI : MonoBehaviour
{
	private PositionDisplay _positionDisplay;
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private Button _popUpBlocker;
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
	private Icon[] _roleSprites;
	[SerializeField]
	private GameObject _historyContainer;
	[SerializeField]
	private GameObject _historyPrefab;

	private void Awake()
	{
		_positionDisplay = GetComponent<PositionDisplay>();
	}

	/// <summary>
	/// Used to rearrange CrewMember names. shortName set to true results in first initial and last name, set to false results in last name, first names
	/// </summary>
	private string SplitName(string original, bool shortName = true)
	{
		var splitName = original.Split(' ');
		var lastName = splitName.Last();
		if (!shortName)
		{
			lastName += ", ";
		}
		foreach (var split in splitName)
		{
			if (split != splitName.Last())
			{
				if (!shortName)
				{
					lastName += split + " ";
				}
				else
				{
					lastName = split[0] + "." + lastName;
				}
			}
		}
		if (!shortName)
		{
			lastName = lastName.Remove(lastName.Length - 1, 1);
		}
		return lastName;
	}

	/// <summary>
	/// If the pop-up is already active, update UI elements to match current information
	/// </summary>
	public void UpdateDisplay()
	{
		if (gameObject.activeSelf)
		{
			Display((Position)Enum.Parse(typeof(Position), _textList[0].text.Replace("-", "")));
		}
	}

	/// <summary>
	/// Activate the pop-up and the blocker
	/// </summary>
	public void SetUpDisplay(Position position)
	{
		Tracker.T.trackedGameObject.Interacted("Viewed Position Information", GameObjectTracker.TrackedGameObject.GameObject);
		gameObject.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		gameObject.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(ClosePositionPopUp);
		Display(position);
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	private void Display(Position position)
	{
		var team = _positionDisplay.GetTeam();
		CrewMember currentCrew = null;
		if (team.Boat.BoatPositionCrew.ContainsKey(position))
		{
			currentCrew = team.Boat.BoatPositionCrew[position];
		}
		_textList[0].text = position.GetName();
		_textList[1].text = position.GetDescription();
		_roleImage.sprite = _roleSprites.First(mo => mo.Name == position.GetName()).Image;
		_currentButton.onClick.RemoveAllListeners();
		_currentAvatar.gameObject.SetActive(currentCrew != null);
		_currentName.gameObject.SetActive(currentCrew != null);
		if (currentCrew != null)
		{
			_currentAvatar.SetAvatar(currentCrew.Avatar, currentCrew.GetMood(), true);
			_currentName.text = currentCrew.Name;
			_currentButton.onClick.AddListener(delegate { _meetingUI.SetUpDisplay(currentCrew); });
		}
		foreach (Transform child in _historyContainer.transform)
		{
			Destroy(child.gameObject);
		}
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
		var positionMembers = new Dictionary<CrewMember, int>();
		foreach (var boat in _positionDisplay.GetLineUpHistory())
		{
			var boatPositions = boat.BoatPositions.Where(bp => bp == position).ToArray();
			foreach (var boatPosition in boatPositions)
			{
				var positionMember = boat.BoatPositionCrew[boatPosition];
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
		foreach (var member in orderedMembers)
		{
			var positionHistory = Instantiate(_historyPrefab);
			positionHistory.transform.SetParent(_historyContainer.transform, false);
			positionHistory.transform.Find("Name").GetComponent<Text>().text = SplitName(member.Key.Name);
			if (team.CrewMembers.ContainsKey(member.Key.Name))
			{
				var current = member.Key;
				positionHistory.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { _meetingUI.SetUpDisplay(current); });
			}
			else
			{
				positionHistory.transform.Find("Button").GetComponent<Button>().interactable = false;
			}
			positionHistory.GetComponentInChildren<AvatarDisplay>().SetAvatar(member.Key.Avatar, member.Key.GetMood(), true);
			positionHistory.transform.Find("Session Back/Sessions").GetComponent<Text>().text = member.Value.ToString();
		}
	}

	/// <summary>
	/// Hide the pop-up for Position details
	/// </summary>
	public void ClosePositionPopUp()
	{
		gameObject.SetActive(false);
		if (_meetingUI.gameObject.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			_meetingUI.gameObject.transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(delegate { _meetingUI.gameObject.SetActive(false); });
		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Adjust blocker position in hierarchy according to another pop-up that uses it is displayed as well
	/// </summary>
	public void ChangeBlockerOrder()
	{
		if (gameObject.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			gameObject.transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(ClosePositionPopUp);

		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
	}
}