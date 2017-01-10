using System;
using System.Collections.Generic;
using PlayGen.SUGAR.Client.EvaluationEvents;
using PlayGen.SUGAR.Contracts.Shared;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

namespace PlayGen.SUGAR.Unity
{
	[DisallowMultipleComponent]
	public class AchievementUnityClient : MonoBehaviour
	{
		private List<EvaluationProgressResponse> _progress = new List<EvaluationProgressResponse>();

		internal List<EvaluationProgressResponse> Progress
		{
			get { return _progress; }
		}

		[SerializeField]
		private AchievementListInterface _achievementListInterface;

		[SerializeField]
		private AchievementPopupInterface _achievementPopup;

		[SerializeField]
		[Range(0f, 10f)]
		private float _notificationCheckRate = 2.5f;

		internal void CreateInterface(Canvas canvas)
		{
			if (_achievementListInterface)
			{
				bool inScene = _achievementListInterface.gameObject.scene == SceneManager.GetActiveScene();
				if (!inScene)
				{
					var newInterface = Instantiate(_achievementListInterface.gameObject, canvas.transform, false);
					newInterface.name = _achievementListInterface.name;
					_achievementListInterface = newInterface.GetComponent<AchievementListInterface>();
				}
				_achievementListInterface.gameObject.SetActive(false);
				SUGARManager.Client.Achievement.EnableNotifications(true);
			}
			if (_achievementPopup)
			{
				bool inScenePopUp = _achievementPopup.gameObject.scene == SceneManager.GetActiveScene();
				if (!inScenePopUp)
				{
					var newPopUp = Instantiate(_achievementPopup.gameObject, canvas.transform, false);
					newPopUp.name = _achievementPopup.name;
					_achievementPopup = newPopUp.GetComponent<AchievementPopupInterface>();
				}
				_achievementPopup.gameObject.SetActive(true);
				InvokeRepeating("NotificatonCheck", _notificationCheckRate, _notificationCheckRate);
			}
		}

		public void DisplayList()
		{
			if (_achievementListInterface)
			{
				GetAchievements(success =>
				{
					_achievementListInterface.Display(success);
				});
			}
		}

		public void Hide()
		{
			if (_achievementListInterface.gameObject.activeSelf)
			{
				SUGARManager.Unity.DisableObject(_achievementListInterface.gameObject);
			}
		}

		private void GetAchievements(Action<bool> success)
		{
			_progress.Clear();
			if (SUGARManager.CurrentUser != null)
			{
				SUGARManager.Client.Achievement.GetGameProgressAsync(SUGARManager.GameId, SUGARManager.CurrentUser.Id,
				response =>
				{
					_progress = response.ToList();
					success(true);
				},
				exception =>
				{
					string error = "Failed to get achievements list. " + exception.Message;
					Debug.LogError(error);
					success(false);
				});
			}
			else
			{
				success(false);
			}
		}

		private void NotificatonCheck()
		{
			EvaluationNotification notification;
			if (SUGARManager.Client.Achievement.TryGetPendingNotification(out notification))
			{
				HandleNotification(notification);
			}
		}

		public void ForceNotificationTest()
		{
			HandleNotification(new EvaluationNotification
			{
				Name = "Test Notification"
			});
		}

		private void HandleNotification(EvaluationNotification notification)
		{
			Debug.Log("NOTIFICATION");
			if (_achievementPopup)
			{
				_achievementPopup.Animate(notification);
			}
		}
	}
}
