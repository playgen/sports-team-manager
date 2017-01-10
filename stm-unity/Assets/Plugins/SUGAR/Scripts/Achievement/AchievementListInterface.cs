using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PlayGen.SUGAR.Unity
{
	public class AchievementListInterface : MonoBehaviour
	{
		[SerializeField]
		private AchievementItemInterface[] _achievementItems;
		[SerializeField]
		private Button _previousButton;
		[SerializeField]
		private Button _nextButton;
		[SerializeField]
		private Text _pageNumberText;
		private int _pageNumber;
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
			_closeButton.onClick.AddListener(delegate { SUGARManager.unity.DisableObject(gameObject); });
			_signinButton.onClick.AddListener(AttemptSignIn);
		}

		private void OnEnable()
		{
			SUGARManager.unity.ButtonBestFit(gameObject);
		}

		internal void Display(bool loadingSuccess)
		{
			
			_pageNumber = 0;
			SetAchievementData(loadingSuccess);
		}

		private void SetAchievementData(bool loadingSuccess)
		{
			SUGARManager.Account.Hide();
			SUGARManager.GameLeaderboard.Hide();
			SUGARManager.Leaderboard.Hide();
			SUGARManager.unity.EnableObject(gameObject);
			_errorText.text = string.Empty;
			_signinButton.gameObject.SetActive(false);
			var achievementList = SUGARManager.Achievement.Progress.Skip(_pageNumber * _achievementItems.Length).Take(_achievementItems.Length).ToList();
			if (!achievementList.Any() && _pageNumber > 0)
			{
				UpdatePageNumber(-1);
				return;
			}
			if (_pageNumber < 0)
			{
				UpdatePageNumber(1);
				return;
			}
			for (int i = 0; i < _achievementItems.Length; i++)
			{
				if (i >= achievementList.Count)
				{
					_achievementItems[i].Disbale();
				}
				else
				{
					_achievementItems[i].SetText(achievementList[i].Name, Mathf.Approximately(achievementList[i].Progress, 1.0f));
				}
			}
			_pageNumberText.text = "Page " + (_pageNumber + 1);
			_previousButton.interactable = _pageNumber > 0;
			_nextButton.interactable = SUGARManager.Achievement.Progress.Count > (_pageNumber + 1) * _achievementItems.Length;
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
					_errorText.text = "Error: Unable to gather current achievement progress.";
				}
			}
			else if (achievementList.Count == 0)
			{
				_errorText.text = "No achievements are currently available for this game.";
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
			SetAchievementData(true);
		}
	}
}
