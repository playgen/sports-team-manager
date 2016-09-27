﻿using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PositionDisplay))]
/// <summary>
/// Contains all UI logic related to the Position pop-up
/// </summary>
public class PositionDisplayUI : MonoBehaviour
{
	private PositionDisplay _positionDisplay;
	[SerializeField]
	private MemberMeetingUI _meetingUI;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Text[] _positionPopUpText;
	[SerializeField]
	private Image[] _positionPopUpSkills;
	[SerializeField]
	private AvatarDisplay _positionPopUpCurrentCrew;
	[SerializeField]
	private Image _positionPopUpRoleImage;
	[SerializeField]
	private Icon[] _positionPopUpRoleSprites;
	[SerializeField]
	private Text _positionPopUpCurrentName;
	[SerializeField]
	private Button _positionPopUpCurrentButton;
	[SerializeField]
	private GameObject _positionPopUpHistoryContainer;
	[SerializeField]
	private GameObject _positionPopUpHistoryPrefab;

	void Awake()
	{
		_positionDisplay = GetComponent<PositionDisplay>();
	}

	string SplitName(string original, bool shortName = true)
	{
		string[] splitName = original.Split(' ');
		string name = splitName.Last();
		if (!shortName)
		{
			name += ", ";
		}
		foreach (string split in splitName)
		{
			if (split != splitName.Last())
			{
				if (!shortName)
				{
					name += split + " ";
				}
				else
				{
					name = split[0] + "." + name;
				}
			}
		}
		if (!shortName)
		{
			name = name.Remove(name.Length - 1, 1);
		}
		return name;
	}

	public void UpdateDisplay()
	{
		if (gameObject.activeSelf)
		{
			ResetDisplay(_positionDisplay.GetBoat().BoatPositions.Select(bp => bp.Position).FirstOrDefault(p => p.Name == _positionPopUpText[0].text));
		}
	}

	/// <summary>
	/// Display and set the information for the pop-up for Position details
	/// </summary>
	public void Display(Position position)
	{
		Tracker.T.trackedGameObject.Interacted("Viewed Position Information", GameObjectTracker.TrackedGameObject.GameObject);
		gameObject.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		gameObject.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { ClosePositionPopUp(); });
		ResetDisplay(position);
	}

	void ResetDisplay(Position position)
	{
		var currentBoat = _positionDisplay.GetBoat();
		_positionPopUpText[0].text = position.Name;
		_positionPopUpText[1].text = position.Description;
		_positionPopUpRoleImage.sprite = _positionPopUpRoleSprites.FirstOrDefault(mo => mo.Name == position.Name).Image;
		CrewMember currentCrew = null;
		if (currentBoat.BoatPositions.FirstOrDefault(bp => bp.Position.Name == position.Name) != null)
		{
			currentCrew = currentBoat.BoatPositions.Where(bp => bp.Position.Name == position.Name).FirstOrDefault().CrewMember;
		}
		_positionPopUpCurrentButton.onClick.RemoveAllListeners();
		if (currentCrew != null)
		{
			_positionPopUpCurrentCrew.gameObject.SetActive(true);
			_positionPopUpCurrentName.gameObject.SetActive(true);
			_positionPopUpCurrentCrew.SetAvatar(currentCrew.Avatar, currentCrew.GetMood(), true);
			_positionPopUpCurrentName.text = currentCrew.Name;
			_positionPopUpCurrentButton.onClick.AddListener(delegate { _meetingUI.SetUpDisplay(currentCrew); });
		}
		else
		{
			_positionPopUpCurrentCrew.gameObject.SetActive(false);
			_positionPopUpCurrentName.gameObject.SetActive(false);
		}
		foreach (Transform child in _positionPopUpHistoryContainer.transform)
		{
			Destroy(child.gameObject);
		}
		foreach (Image skill in _positionPopUpSkills)
		{
			skill.enabled = false;
			foreach (CrewMemberSkill actualSkill in position.RequiredSkills)
			{
				if (skill.name == actualSkill.ToString())
				{
					skill.enabled = true;
				}
			}
		}
		Dictionary<CrewMember, int> positionMembers = new Dictionary<CrewMember, int>();
		foreach (var boat in _positionDisplay.GetLineUpHistory())
		{
			BoatPosition[] boatPositions = boat.BoatPositions.Where(bp => bp.Position.Name == position.Name).ToArray();
			if (boatPositions != null)
			{
				foreach (BoatPosition boatPosition in boatPositions)
				{
					CrewMember positionMember = boatPosition.CrewMember;
					if (positionMembers.ContainsKey(positionMember))
					{
						positionMembers[positionMember]++;
					} else
					{
						positionMembers.Add(positionMember, 1);
					}
				}
			}
		}
		var orderedMembers = positionMembers.OrderBy(pm => SplitName(pm.Key.Name, false)).OrderByDescending(pm => pm.Value);
		foreach (var member in orderedMembers)
		{
			GameObject positionHistory = Instantiate(_positionPopUpHistoryPrefab);
			positionHistory.transform.SetParent(_positionPopUpHistoryContainer.transform, false);
			positionHistory.transform.Find("Name").GetComponent<Text>().text = SplitName(member.Key.Name);
			if (_positionDisplay.GetBoat().GetAllCrewMembers().Contains(member.Key))
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

	public void ChangeBlockerOrder()
	{
		if (gameObject.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			gameObject.transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(delegate { ClosePositionPopUp(); });

		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
	}
}