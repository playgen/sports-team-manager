using System.Collections.Generic;
using System.Linq;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
using RAGE.Analytics.Formats;
using UnityEngine;
using UnityEngine.UI;

public class CupResultUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _postRaceCrewPrefab;
	private int _cupPosition;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Display pop-up which shows the cup result
	/// </summary>
	public void Display()
	{
		_cupPosition = 0;
		gameObject.Active(true);
		transform.EnableBlocker();

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
			memberObject.transform.Find("Avatar").GetComponentInChildren<AvatarDisplay>().SetAvatar(crewMember.Avatar, -(_cupPosition - 3) * 2);
			memberObject.transform.Find("Position").GetComponent<Image>().enabled = false;
			if (crewCount % 2 != 0)
			{
				var currentScale = memberObject.transform.Find("Avatar").localScale;
				memberObject.transform.Find("Avatar").localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
			}
			crewCount++;
			memberObject.transform.SetAsLastSibling();
		}
		transform.Find("Result").GetComponent<Text>().text = Localization.GetAndFormat("RACE_RESULT_POSITION", false, GameManagement.TeamName, finalPositionText);
		DoBestFit();
		TrackerEventSender.SendEvent(new TraceEvent("CupResultPopUpDisplayed", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.CupFinishingPosition.ToString(), _cupPosition.ToString() }
		}, AccessibleTracker.Accessible.Screen));
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
			UIStateManager.StaticGoToQuestionnaire();
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		Display();
		DoBestFit();
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Text>().Where(t => t.transform.parent == transform).BestFit();
	}
}