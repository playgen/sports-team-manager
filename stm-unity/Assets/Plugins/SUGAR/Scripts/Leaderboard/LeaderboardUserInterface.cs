using System.Collections.Generic;
using System.Linq;

using PlayGen.SUGAR.Common.Shared;
using PlayGen.SUGAR.Contracts.Shared;

using UnityEngine;
using UnityEngine.UI;

namespace SUGAR.Unity
{
	public class LeaderboardUserInterface : MonoBehaviour
	{
		[SerializeField]
		private Text _leaderboardName;
		[SerializeField]
		private Text _leaderboardType;
		[SerializeField]
		private LeaderboardPositionInterface[] _leaderboardPositions;
		[SerializeField]
		private Button _previousButton;
		[SerializeField]
		private Button _nextButton;
		[SerializeField]
		private Text _pageNumberText;
		private int _pageNumber;
		private LeaderboardFilterType _filter;
		[SerializeField]
		private Button _topButton;
		[SerializeField]
		private Button _nearButton;
		[SerializeField]
		private Button _friendsButton;
		[SerializeField]
		private Button _closeButton;

		private void Awake()
		{
			_previousButton.onClick.AddListener(delegate { UpdatePageNumber(-1); });
			_nextButton.onClick.AddListener(delegate { UpdatePageNumber(1); });
			_topButton.onClick.AddListener(delegate { UpdateFilter(0); });
			_nearButton.onClick.AddListener(delegate { UpdateFilter(1); });
			_friendsButton.onClick.AddListener(delegate { UpdateFilter(2); });
			_closeButton.onClick.AddListener(delegate { gameObject.SetActive(false); });
		}

		internal void Display(LeaderboardFilterType filter, IEnumerable<LeaderboardStandingsResponse> standings)
		{
			_pageNumber = 0;
			_filter = filter;
			ShowLeaderboard(standings);
		}

		private void ShowLeaderboard(IEnumerable<LeaderboardStandingsResponse> standings)
		{
			if (SUGARManager.CurrentUser == null || standings == null)
			{
				return;
			}
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
			var standingsList = standings.ToList();
			if (!standingsList.Any() && _pageNumber > 0)
			{
				UpdatePageNumber(-1);
				return;
			}
			if (_pageNumber < 0)
			{
				UpdatePageNumber(1);
				return;
			}
			for (int i = 0; i < _leaderboardPositions.Length; i++)
			{
				if (i >= standingsList.Count)
				{
					_leaderboardPositions[i].Disbale();
				} else
				{
					_leaderboardPositions[i].SetText(standingsList[i]);
				}
			}
			_leaderboardName.text = SUGARManager.Leaderboard.CurrentLeaderboard.Name;
			_leaderboardType.text = _filter.ToString();
			_pageNumberText.text = "Page " + (_pageNumber + 1);
			_previousButton.interactable = _pageNumber > 0;
			_nextButton.interactable = standingsList.Count > _leaderboardPositions.Length;
			_nearButton.interactable = SUGARManager.CurrentUser != null && SUGARManager.Leaderboard.CurrentLeaderboard.ActorType == ActorType.User;
			_friendsButton.interactable = SUGARManager.CurrentUser != null && SUGARManager.Leaderboard.CurrentLeaderboard.ActorType == ActorType.User;
		}

		private void UpdatePageNumber(int changeAmount)
		{
			_pageNumber += changeAmount;
			SUGARManager.Leaderboard.GetLeaderboardStandings(_filter, _pageNumber, result =>
			{
				var standings = result.ToList();
				if (standings.Count != 0)
				{
					ShowLeaderboard(standings);
				}
			});
		}

		private void UpdateFilter(int filter)
		{
			_pageNumber = 0;
			_filter = (LeaderboardFilterType)filter;
			SUGARManager.Leaderboard.GetLeaderboardStandings(_filter, _pageNumber, result =>
			{
				var standings = result.ToList();
				if (standings.Count != 0)
				{
					ShowLeaderboard(standings);
				}
			});
		}

		internal int GetPossiblePositionCount()
		{
			return _leaderboardPositions.Length;
		}

	}
}
