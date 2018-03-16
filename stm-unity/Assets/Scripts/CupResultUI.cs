using System.Collections.Generic;
using System.Linq;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;

public class CupResultUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _postRaceCrewPrefab;
	private int _cupPosition;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
	}

	/// <summary>
	/// Display pop-up which shows the cup result
	/// </summary>
	public void Display()
	{
		_cupPosition = 0;
		gameObject.Active(true);
		if (GameManagement.PlatformSettings.Rage)
		{
			transform.EnableBlocker();
		}
		else
		{
			transform.EnableBlocker(() => Close(TrackerTriggerSources.PopUpBlocker.ToString()));
		}

		foreach (Transform child in transform.Find("Crew"))
		{
			Destroy(child.gameObject);
		}
		_cupPosition = GameManagement.GetCupPosition();
		var finalPositionText = Localization.Get("POSITION_" + _cupPosition);
		var crewCount = 0;
		foreach (var crewMember in GameManagement.CrewMembers.Values.ToList())
		{
			var memberObject = Instantiate(_postRaceCrewPrefab);
			memberObject.transform.SetParent(transform.Find("Crew"), false);
			memberObject.name = crewMember.Name;
			memberObject.transform.FindComponentInChildren<AvatarDisplay>("Avatar").SetAvatar(crewMember.Avatar, -(_cupPosition - 3) * 2);
			memberObject.transform.FindImage("Position").enabled = false;
			if (crewCount % 2 != 0)
			{
				var currentScale = memberObject.transform.Find("Avatar").localScale;
				memberObject.transform.Find("Avatar").localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
			}
			crewCount++;
			memberObject.transform.SetAsLastSibling();
		}
		transform.FindText("Result").text = Localization.GetAndFormat("RACE_RESULT_POSITION", false, GameManagement.TeamName, finalPositionText);
		TrackerEventSender.SendEvent(new TraceEvent("CupResultPopUpDisplayed", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.CupFinishingPosition.ToString(), _cupPosition.ToString() }
		}, AccessibleTracker.Accessible.Screen));
		if (!GameManagement.PlatformSettings.Rage)
		{
			transform.FindText("Outro").text = "Thanks for playing!";
		}
		transform.FindObject("Questionnaire").Active(GameManagement.PlatformSettings.Rage);
		transform.FindObject("OK").Active(!GameManagement.PlatformSettings.Rage);
	}

	/// <summary>
	/// Close the promotion pop-up
	/// </summary>
	public void Close(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.Active(false);
			UIManagement.DisableBlocker();
			TrackerEventSender.SendEvent(new TraceEvent("CupResultPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.CupFinishingPosition.ToString(), _cupPosition.ToString() },
				{ TrackerContextKeys.TriggerUI.ToString(), source }
			}, AccessibleTracker.Accessible.Screen));
			if (GameManagement.PlatformSettings.Rage)
			{
				UIStateManager.StaticGoToQuestionnaire();
			}
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		Display();
	}
}