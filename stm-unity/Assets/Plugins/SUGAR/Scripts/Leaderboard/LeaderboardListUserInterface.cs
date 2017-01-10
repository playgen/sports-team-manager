using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PlayGen.SUGAR.Common.Shared;

namespace PlayGen.SUGAR.Unity
{
	public class LeaderboardListUserInterface : MonoBehaviour
	{
		[SerializeField]
		private Button[] _leaderboardButtons;
		[SerializeField]
		private Text _leaderboardType;
		[SerializeField]
		private Button _previousButton;
		[SerializeField]
		private Button _nextButton;
		[SerializeField]
		private Text _pageNumberText;
		private int _pageNumber;
		private ActorType _actorType;
		[SerializeField]
		private Button _userButton;
		[SerializeField]
		private Button _groupButton;
		[SerializeField]
		private Button _combinedButton;
		[SerializeField]
		private Text _errorText;
		[SerializeField]
		private Button _closeButton;
		[SerializeField]
		private Button _signinButton;

		private void Awake()
		{
			_previousButton.onClick.AddListener(delegate { UpdatePageNumber(-1); });
			_nextButton.onClick.AddListener(delegate { UpdatePageNumber(1); });
			_userButton.onClick.AddListener(delegate { UpdateFilter(1); });
			_groupButton.onClick.AddListener(delegate { UpdateFilter(2); });
			_combinedButton.onClick.AddListener(delegate { UpdateFilter(0); });
			_closeButton.onClick.AddListener(delegate { SUGARManager.Unity.DisableObject(gameObject); });
			_signinButton.onClick.AddListener(delegate { AttemptSignIn(); });
		}

		private void OnEnable()
		{
			SUGARManager.Unity.ButtonBestFit(gameObject);
		}

		internal void Display(ActorType filter, bool loadingSuccess)
		{
			
			_pageNumber = 0;
			_actorType = filter;
			ShowLeaderboards(loadingSuccess);
		}

		private void ShowLeaderboards(bool loadingSuccess)
		{
			SUGARManager.Account.Hide();
			SUGARManager.Achievement.Hide();
			SUGARManager.Unity.EnableObject(gameObject);
			_errorText.text = string.Empty;
			_signinButton.gameObject.SetActive(false);
			_userButton.interactable = true;
			_groupButton.interactable = true;
			_combinedButton.interactable = true;
			var leaderboardList = SUGARManager.GameLeaderboard.Leaderboards[(int)_actorType].ToList();
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
					_leaderboardButtons[i].onClick.AddListener(delegate { SUGARManager.Leaderboard.Display(token); });
					_leaderboardButtons[i].gameObject.SetActive(true);
				}
			}
			_leaderboardType.text = _actorType == ActorType.Undefined ? "Combined" : _actorType.ToString();
			_pageNumberText.text = "Page " + (_pageNumber + 1);
			_previousButton.interactable = _pageNumber > 0;
			if (!loadingSuccess)
			{
				if (SUGARManager.CurrentUser == null)
				{
					_errorText.text = "Error: No user currently signed in.";
					if (SUGARManager.Account.HasInterface)
					{
						_signinButton.gameObject.SetActive(true);
					}
				}
				else
				{
					_errorText.text = "Error: Unable to gather leaderboards for this game.";
				}
				_userButton.interactable = false;
				_groupButton.interactable = false;
				_combinedButton.interactable = false;
			}
			else if (leaderboardList.Count == 0)
			{
				_errorText.text = "No leaderboards are currently available for this game for this filter.";
			}
		}

		private void AttemptSignIn()
		{
			SUGARManager.Account.DisplayPanel(success =>
			{
				if (success)
				{
					UpdatePageNumber(0);
				}
			});
		}

		private void UpdatePageNumber(int changeAmount)
		{
			_pageNumber += changeAmount;
			ShowLeaderboards(true);
		}

		private void UpdateFilter(int filter)
		{
			_pageNumber = 0;
			_actorType = (ActorType)filter;
			ShowLeaderboards(true);
		}
	}
}