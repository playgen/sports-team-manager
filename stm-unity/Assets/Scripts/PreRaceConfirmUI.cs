using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Extensions;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;

using TrackerAssetPackage;

using UnityEngine;
using UnityEngine.UI;

public class PreRaceConfirmUI : MonoBehaviour
{
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
	/// Display a pop-up before a race, with different text depending on if the player has ActionAllowance remaining
	/// </summary>
	public void ConfirmPopUp()
	{
		gameObject.Active(true);
		var yesButton = transform.FindButton("Yes");
		var noButton = transform.FindButton("No");
		yesButton.onClick.RemoveAllListeners();
		if (!GameManagement.IsRace)
		{
			GetComponentInChildren<Text>().text = GameManagement.ActionRemaining ? Localization.GetAndFormat("RACE_SKIP_CONFIRM_ALLOWANCE_REMAINING", false, GameManagement.ActionAllowance) : Localization.Get("RACE_SKIP_CONFIRM_NO_ALLOWANCE");
			TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceConfirmPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKey.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
				{ TrackerContextKey.RemainingSessions.ToString(), GameManagement.SessionsRemaining.ToString() }
			}, AccessibleTracker.Accessible.Screen));
			yesButton.onClick.AddListener(() =>
				TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceApproved", TrackerAsset.Verb.Selected, new Dictionary<string, string>
				{
					{ TrackerContextKey.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
					{ TrackerContextKey.RemainingSessions.ToString(), GameManagement.SessionsRemaining.ToString() }
				}, "SkipToRace", AlternativeTracker.Alternative.Menu))
			);
			yesButton.onClick.AddListener(() => SUGARManager.GameData.Send("Practice Sessions Skipped", GameManagement.SessionsRemaining));
		}
		else
		{
			GetComponentInChildren<Text>().text = GameManagement.ActionRemaining ? Localization.GetAndFormat("RACE_CONFIRM_ALLOWANCE_REMAINING", false, GameManagement.ActionAllowance) : Localization.Get("RACE_CONFIRM_NO_ALLOWANCE");
			TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKey.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() }
			}, AccessibleTracker.Accessible.Screen));
			yesButton.onClick.AddListener(() =>
				TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmApproved", TrackerAsset.Verb.Selected, new Dictionary<string, string>
				{
					{ TrackerContextKey.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() }
				}, "RaceConfirm", AlternativeTracker.Alternative.Menu))
			);
		}
		yesButton.onClick.AddListener(() => CloseConfirmPopUp(string.Empty));
		yesButton.onClick.AddListener(UIManagement.TeamSelection.SkipToRace);
		yesButton.onClick.AddListener(UIManagement.TeamSelection.ConfirmLineUp);
		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(() => CloseConfirmPopUp(TrackerTriggerSource.NoButtonSelected.ToString()));
		DoBestFit();
	    transform.EnableBlocker(() => CloseConfirmPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
	}

	/// <summary>
	/// Close the race confirm pop-up
	/// </summary>
	public void CloseConfirmPopUp(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.Active(false);
		    UIManagement.DisableBlocker();
			if (!string.IsNullOrEmpty(source))
			{
				if (!GameManagement.IsRace)
				{
					TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceDeclined", TrackerAsset.Verb.Skipped, new Dictionary<string, string>
				{
					{ TrackerContextKey.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
					{ TrackerContextKey.RemainingSessions.ToString(), GameManagement.SessionsRemaining.ToString() },
					{ TrackerContextKey.TriggerUI.ToString(), source }
				}, AccessibleTracker.Accessible.Screen));
				}
				else
				{
					TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmDeclined", TrackerAsset.Verb.Skipped, new Dictionary<string, string>
				{
					{ TrackerContextKey.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
					{ TrackerContextKey.TriggerUI.ToString(), source }
				}, AccessibleTracker.Accessible.Screen));
				}
			}
		}
	}

	/// <summary>
	/// Confirm the current line-up
	/// </summary>
	public void ConfirmLineUpCheck()
	{
		var lastRace = GameManagement.LineUpHistory.LastOrDefault();
		if (lastRace != null)
		{
			if (GameManagement.Positions.SequenceEqual(lastRace.Positions) && GameManagement.PositionCrew.OrderBy(pc => pc.Key.ToString()).SequenceEqual(lastRace.PositionCrew.OrderBy(pc => pc.Key.ToString())))
			{
				DisplayRepeatWarning();
				return;
			}
		}
	    UIManagement.TeamSelection.ConfirmLineUp();
		CloseConfirmPopUp(string.Empty);
	}

	/// <summary>
	/// Display a pop-up before a race if the player is using the line-up as the previous race
	/// </summary>
	public void DisplayRepeatWarning()
	{
		gameObject.Active(true);
		GetComponentInChildren<Text>().text = Localization.Get("REPEAT_CONFIRM");
		TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpConfirmPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<string, string>(), AccessibleTracker.Accessible.Screen));
		var yesButton = transform.FindButton("Yes");
		var noButton = transform.FindButton("No");
		yesButton.onClick.RemoveAllListeners();
		yesButton.onClick.AddListener(UIManagement.TeamSelection.ConfirmLineUp);
		yesButton.onClick.AddListener(() => CloseConfirmPopUp(string.Empty));
		yesButton.onClick.AddListener(() => TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpApproved", TrackerAsset.Verb.Selected, new Dictionary<string, string>(), "RepeatLineUp", AlternativeTracker.Alternative.Menu)));

		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(() => CloseRepeatWarning(TrackerTriggerSource.NoButtonSelected.ToString()));
		DoBestFit();
	    transform.EnableBlocker(() => CloseRepeatWarning(TrackerTriggerSource.PopUpBlocker.ToString()));
	}

	/// <summary>
	/// Close the repeat line-up warning
	/// </summary>
	public void CloseRepeatWarning(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.Active(false);
		    UIManagement.DisableBlocker();
			TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpDeclined", TrackerAsset.Verb.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKey.TriggerUI.ToString(), source }
			}, AccessibleTracker.Accessible.Screen));
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		GetComponentInChildren<Text>().text = GameManagement.IsRace ? GameManagement.ActionRemaining ? Localization.GetAndFormat("RACE_CONFIRM_ALLOWANCE_REMAINING", false, GameManagement.ActionAllowance) : Localization.Get("RACE_CONFIRM_NO_ALLOWANCE") : Localization.Get("REPEAT_CONFIRM");
		DoBestFit();
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Button>().ToList().BestFit();
	}
}
