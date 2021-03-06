﻿using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;
using TrackerAssetPackage;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI displayed before a session is completed if a repeat line-up is used or the session is a race
/// </summary>
public class PreRaceConfirmUI : MonoBehaviour
{
	[SerializeField]
	private Text _popUpText;
	[SerializeField]
	private Button _yesButton;
	[SerializeField]
	private Button _noButton;
	private string _localizationKey;
	private bool _isRace;

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
	public void ConfirmPopUp(bool race)
	{
		gameObject.Active(true);
		_isRace = race;
		//set different text depending on if this is for a race
		if (_isRace)
		{
			//if the next session is a race, show text depending asking the player to confirm and warn if action allowance is remaining
			if (GameManagement.IsRace)
			{
				_localizationKey = GameManagement.ActionRemaining ? "RACE_CONFIRM_ALLOWANCE_REMAINING" : "RACE_CONFIRM_NO_ALLOWANCE";
				TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance }
				}, AccessibleTracker.Accessible.Screen));
			}
			//if the next session is practice, meaning the player is skipping to the race, show text depending asking the player to confirm and warn if action allowance is remaining
			else
			{
				_localizationKey = GameManagement.ActionRemaining ? "RACE_SKIP_CONFIRM_ALLOWANCE_REMAINING" : "RACE_SKIP_CONFIRM_NO_ALLOWANCE";
				TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceConfirmPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
					{ TrackerContextKey.RemainingSessions, GameManagement.SessionsRemaining }
				}, AccessibleTracker.Accessible.Screen));
			}
		}
		//if the next session is not a race, show this pop-up if the same line-up as the previous session has been selected
		else
		{
			if (GameManagement.PreviousSession != null && GameManagement.Positions.SequenceEqual(GameManagement.PreviousSession.Positions) && GameManagement.PositionCrew.OrderBy(pc => pc.Key.ToString()).SequenceEqual(GameManagement.PreviousSession.PositionCrew.OrderBy(pc => pc.Key.ToString())))
			{
				gameObject.Active(true);
				_localizationKey = "REPEAT_CONFIRM";
				TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpConfirmPopUpOpened", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>(), AccessibleTracker.Accessible.Screen));
			}
			else
			{
				UIManagement.TeamSelection.ConfirmLineUp();
				CloseConfirmPopUp(string.Empty);
				return;
			}
		}
		_yesButton.onClick.RemoveAllListeners();
		_yesButton.onClick.AddListener(ConfirmPopUpYesSelected);
		_noButton.onClick.RemoveAllListeners();
		_noButton.onClick.AddListener(() => CloseConfirmPopUp(TrackerTriggerSource.NoButtonSelected.ToString()));
		OnLanguageChange();
		transform.EnableBlocker(() => CloseConfirmPopUp(TrackerTriggerSource.PopUpBlocker.ToString()));
	}

	/// <summary>
	/// Triggered by Unity event. Player has confirmed and session can proceed
	/// </summary>
	private void ConfirmPopUpYesSelected()
	{
		if (_isRace)
		{
			if (GameManagement.IsRace)
			{
				TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmApproved", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance }
				}, "RaceConfirm", AlternativeTracker.Alternative.Menu));
			}
			else
			{
				TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceApproved", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
					{ TrackerContextKey.RemainingSessions, GameManagement.SessionsRemaining }
				}, "SkipToRace", AlternativeTracker.Alternative.Menu));
				SUGARManager.GameData.Send("Practice Sessions Skipped", GameManagement.SessionsRemaining);
			}
			UIManagement.TeamSelection.SkipToRace();
		}
		else
		{
			UIManagement.TeamSelection.ConfirmLineUp();
			TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpApproved", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>(), "RepeatLineUp", AlternativeTracker.Alternative.Menu));
		}
		CloseConfirmPopUp(string.Empty);
	}

	/// <summary>
	/// triggered by Unity event. Player has declined, so close this pop-up and allow them to continue playing before racing
	/// </summary>
	public void CloseConfirmPopUp(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.Active(false);
			UIManagement.DisableBlocker();
			if (_isRace)
			{
				if (!string.IsNullOrEmpty(source))
				{
					if (GameManagement.IsRace)
					{
						TrackerEventSender.SendEvent(new TraceEvent("RaceConfirmDeclined", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
						{
							{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
							{ TrackerContextKey.TriggerUI, source }
						}, AccessibleTracker.Accessible.Screen));
					}
					else
					{
						TrackerEventSender.SendEvent(new TraceEvent("SkipToRaceDeclined", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
						{
							{ TrackerContextKey.CurrentTalkTime, GameManagement.ActionAllowance },
							{ TrackerContextKey.RemainingSessions, GameManagement.SessionsRemaining },
							{ TrackerContextKey.TriggerUI, source }
						}, AccessibleTracker.Accessible.Screen));
					}
				}
			}
			else
			{
				TrackerEventSender.SendEvent(new TraceEvent("RepeatLineUpDeclined", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.TriggerUI, source }
				}, AccessibleTracker.Accessible.Screen));
			}
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		_popUpText.text = Localization.GetAndFormat(_localizationKey, false, GameManagement.ActionAllowance);
		DoBestFit();
	}

	private void DoBestFit()
	{
		new Component[] {_yesButton, _noButton}.BestFit();
	}
}