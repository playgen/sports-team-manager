using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PlayGen.SUGAR.Common.Shared;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;

public class LeaderboardListInterface : BaseLeaderboardListInterface
{
	/// <summary>
	/// An array of buttons on this GameObject used to display the relevant leaderboard, set in the Inspector.
	/// </summary>
	[Tooltip("An array of buttons on this GameObject used to display the relevant leaderboard, set in the Inspector.")]
	[SerializeField]
	private Button[] _leaderboardButtons;

	/// <summary>
	/// Button used to go to the previous page of results.
	/// </summary>
	[Tooltip("Button used to go to the previous page of results.")]
	[SerializeField]
	private Button _previousButton;

	/// <summary>
	/// Button used to go to the next page of results.
	/// </summary>
	[Tooltip("Button used to go to the next page of results.")]
	[SerializeField]
	private Button _nextButton;

	/// <summary>
	/// Text which displays the current page.
	/// </summary>
	[Tooltip("Text which displays the current page.")]
	[SerializeField]
	private Text _pageNumberText;

	/// <summary>
	/// The current page number.
	/// </summary>
	private int _pageNumber;

	/// <summary>
	/// In addition to base onclick adding, adds listeners for the previous and next buttons.
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		_previousButton.onClick.AddListener(delegate { UpdatePageNumber(-1); });
		_nextButton.onClick.AddListener(delegate { UpdatePageNumber(1); });
	}

	/// <summary>
	/// Trigger DoBestFit method and add event listeners for when resolution and language changes.
	/// </summary>
	private void OnEnable()
	{
		DoBestFit();
		BestFit.ResolutionChange += DoBestFit;
		Localization.LanguageChange += OnLanguageChange;
	}

	/// <summary>
	/// Remove event listeners.
	/// </summary>
	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
		Localization.LanguageChange -= OnLanguageChange;
	}

	/// <summary>
	/// Set the pageNumber to 0 before displaying the UI.
	/// </summary>
	protected override void PreDisplay()
	{
		_pageNumber = 0;
	}

	/// <summary>
	/// Adjust leaderboardButtons pool to display a page of leaderboards.
	/// </summary>
	protected override void Draw()
	{
		var leaderboardList = SUGARManager.GameLeaderboard.Leaderboards[SUGARManager.GameLeaderboard.CurrentActorType].ToList();
		_nextButton.interactable = leaderboardList.Count > (_pageNumber + 1) * _leaderboardButtons.Length;
		leaderboardList = leaderboardList.Skip(_pageNumber * _leaderboardButtons.Length).Take(_leaderboardButtons.Length).ToList();
		if (!leaderboardList.Any() && _pageNumber > 0)
		{
			UpdatePageNumber(-1);
			return;
		}
		if (_pageNumber < 0)
		{
			UpdatePageNumber(1);
			return;
		}
		for (int i = 0; i < _leaderboardButtons.Length; i++)
		{
			if (i >= leaderboardList.Count)
			{
				_leaderboardButtons[i].gameObject.SetActive(false);
			}
			else
			{
				_leaderboardButtons[i].onClick.RemoveAllListeners();
				_leaderboardButtons[i].GetComponentInChildren<Text>().text = leaderboardList[i].Name;
				var token = leaderboardList[i].Token;
				_leaderboardButtons[i].onClick.AddListener(delegate { SUGARManager.Leaderboard.Display(token, SUGARManager.Leaderboard.CurrentFilter); });
				_leaderboardButtons[i].gameObject.SetActive(true);
			}
		}
		_leaderboardType.text = SUGARManager.GameLeaderboard.CurrentActorType == ActorType.Undefined ? Localization.Get("COMBINED") : Localization.Get(SUGARManager.GameLeaderboard.CurrentActorType.ToString());
		_pageNumberText.text = Localization.GetAndFormat("PAGE", false, _pageNumber + 1);
		_previousButton.interactable = _pageNumber > 0;
		DoBestFit();
	}

	/// <summary>
	/// If a user signs in via this panel, refresh the current page (which should be page 1).
	/// </summary>
	protected override void OnSignIn()
	{
		UpdatePageNumber(0);
	}

	/// <summary>
	/// Adjust the current page number and redraw the UI.
	/// </summary>
	private void UpdatePageNumber(int changeAmount)
	{
		_pageNumber += changeAmount;
		Show(true);
	}

	/// <summary>
	/// Set the text of all buttons to be as big as possible and the same size.
	/// </summary>
	private void DoBestFit()
	{
		GetComponentsInChildren<Button>(true).Select(t => t.gameObject).BestFit();
	}

	/// <summary>
	/// Refresh the current page to ensure any text set in code is also translated.
	/// </summary>
	private void OnLanguageChange()
	{
		UpdatePageNumber(0);
	}
}