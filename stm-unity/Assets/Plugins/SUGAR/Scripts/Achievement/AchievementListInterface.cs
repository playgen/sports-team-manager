using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SUGAR.Unity
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
		private Button _closeButton;

		private void Awake()
		{
			_previousButton.onClick.AddListener(delegate { UpdatePageNumber(-1); });
			_nextButton.onClick.AddListener(delegate { UpdatePageNumber(1); });
			_closeButton.onClick.AddListener(delegate { gameObject.SetActive(false); });
		}

		internal void Display()
		{
			_pageNumber = 0;
			SetAchievementData();
		}

		private void SetAchievementData()
		{
			if (SUGARManager.CurrentUser == null)
			{
				return;
			}
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
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
		}

		private void UpdatePageNumber(int changeAmount)
		{
			_pageNumber += changeAmount;
			SetAchievementData();
		}
	}
}
