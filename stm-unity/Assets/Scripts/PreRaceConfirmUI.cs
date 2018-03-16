using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
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
		yesButton.onClick.RemoveAllListeners();
		if (!GameManagement.IsRace)
		{
			GetComponentInChildren<Text>().text = GameManagement.ActionRemaining ? Localization.GetAndFormat("RACE_SKIP_CONFIRM_ALLOWANCE_REMAINING", false, GameManagement.ActionAllowance) : Localization.Get("RACE_SKIP_CONFIRM_NO_ALLOWANCE");
			TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceConfirmPopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
				{ TrackerContextKeys.RemainingSessions.ToString(), GameManagement.SessionsRemaining.ToString() }
			}, AccessibleTracker.Accessible.Screen));
			yesButton.onClick.AddListener(() =>
				TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceApproved", TrackerVerbs.Selected, new Dictionary<string, string>
				{
					{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
					{ TrackerContextKeys.RemainingSessions.ToString(), GameManagement.SessionsRemaining.ToString() }
				}, "SkipToRace", AlternativeTracker.Alternative.Menu))
			);
			yesButton.onClick.AddListener(() => SUGARManager.GameData.Send("Practice Sessions Skipped", GameManagement.SessionsRemaining));
		}
		else
		{
			GetComponentInChildren<Text>().text = GameManagement.ActionRemaining ? Localization.GetAndFormat("RACE_CONFIRM_ALLOWANCE_REMAINING", false, GameManagement.ActionAllowance) : Localization.Get("RACE_CONFIRM_NO_ALLOWANCE");
			TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmPopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() }
			}, AccessibleTracker.Accessible.Screen));
			yesButton.onClick.AddListener(() =>
				TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmApproved", TrackerVerbs.Selected, new Dictionary<string, string>
				{
					{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() }
				}, "RaceConfirm", AlternativeTracker.Alternative.Menu))
			);
		}
		yesButton.onClick.AddListener(() => CloseConfirmPopUp(string.Empty));
		yesButton.onClick.AddListener(UIManagement.TeamSelection.SkipToRace);
		yesButton.onClick.AddListener(UIManagement.TeamSelection.ConfirmLineUp);
		var noButton = transform.FindButton("No");
		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(() => CloseConfirmPopUp(TrackerTriggerSources.NoButtonSelected.ToString()));
		DoBestFit();
	    transform.EnableBlocker(() => CloseConfirmPopUp(TrackerTriggerSources.PopUpBlocker.ToString()));
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
					TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceDeclined", TrackerVerbs.Skipped, new Dictionary<string, string>
				{
					{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
					{ TrackerContextKeys.RemainingSessions.ToString(), GameManagement.SessionsRemaining.ToString() },
					{ TrackerContextKeys.TriggerUI.ToString(), source }
				}, AccessibleTracker.Accessible.Screen));
				}
				else
				{
					TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmDeclined", TrackerVerbs.Skipped, new Dictionary<string, string>
				{
					{ TrackerContextKeys.CurrentTalkTime.ToString(), GameManagement.ActionAllowance.ToString() },
					{ TrackerContextKeys.TriggerUI.ToString(), source }
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
		TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpConfirmPopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>(), AccessibleTracker.Accessible.Screen));
		var yesButton = transform.FindButton("Yes");
		yesButton.onClick.RemoveAllListeners();
		yesButton.onClick.AddListener(UIManagement.TeamSelection.ConfirmLineUp);
		yesButton.onClick.AddListener(() => CloseConfirmPopUp(string.Empty));
		yesButton.onClick.AddListener(() => TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpApproved", TrackerVerbs.Selected, new Dictionary<string, string>(), "RepeatLineUp", AlternativeTracker.Alternative.Menu)));

		var noButton = transform.FindButton("No");
		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(() => CloseRepeatWarning(TrackerTriggerSources.NoButtonSelected.ToString()));
		DoBestFit();
	    transform.EnableBlocker(() => CloseRepeatWarning(TrackerTriggerSources.PopUpBlocker.ToString()));
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
			TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpDeclined", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.TriggerUI.ToString(), source }
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
		GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
	}
}
