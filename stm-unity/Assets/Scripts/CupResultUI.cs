using System.Collections.Generic;

using PlayGen.Unity.Utilities.Extensions;
using PlayGen.Unity.Utilities.Localization;

using TrackerAssetPackage;

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
		if (GameManagement.RageMode)
		{
			transform.EnableBlocker();
		}
		else
		{
			transform.EnableBlocker(() => Close(TrackerTriggerSource.PopUpBlocker.ToString()));
		}

		var crewTransform = transform.Find("Crew");

		foreach (Transform child in crewTransform)
		{
			Destroy(child.gameObject);
		}
		_cupPosition = GameManagement.GetCupPosition();
		var finalPositionText = Localization.Get("POSITION_" + _cupPosition);
		var crewCount = 0;
		foreach (var crewMember in GameManagement.CrewMemberList)
		{
			var memberObject = Instantiate(_postRaceCrewPrefab);
			memberObject.transform.SetParent(crewTransform, false);
			memberObject.name = crewMember.Name;
			memberObject.transform.FindComponentInChildren<AvatarDisplay>("Avatar").SetAvatar(crewMember.Avatar, -(_cupPosition - 3) * 2);
			memberObject.transform.FindImage("Position").enabled = false;
			if (crewCount % 2 != 0)
			{
				memberObject.transform.localScale = Vector3.Reflect(memberObject.transform.localScale, Vector3.left);
			}
			crewCount++;
			memberObject.transform.SetAsLastSibling();
		}
		transform.FindText("Result").text = Localization.GetAndFormat("RACE_RESULT_POSITION", false, GameManagement.TeamName, finalPositionText);
		TrackerEventSender.SendEvent(new TraceEvent("CupResultPopUpDisplayed", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.CupFinishingPosition, _cupPosition }
		}, AccessibleTracker.Accessible.Screen));
		if (!GameManagement.RageMode)
		{
			transform.FindText("Outro").text = "Thanks for playing!";
		}
		transform.FindObject("Questionnaire").Active(GameManagement.RageMode);
		transform.FindObject("OK").Active(!GameManagement.RageMode);
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
			TrackerEventSender.SendEvent(new TraceEvent("CupResultPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.CupFinishingPosition, _cupPosition },
				{ TrackerContextKey.TriggerUI, source }
			}, AccessibleTracker.Accessible.Screen));
			if (GameManagement.RageMode)
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