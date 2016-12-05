using System;

using UnityEngine;
using System.Collections.Generic;

using PlayGen.SUGAR.Common.Shared;
using PlayGen.SUGAR.Contracts.Shared;
using System.Linq;

using UnityEngine.SceneManagement;

namespace SUGAR.Unity
{
	[DisallowMultipleComponent]
	public class LeaderboardListUnityClient : MonoBehaviour
	{
		private readonly List<List<LeaderboardResponse>> _leaderboards = new List<List<LeaderboardResponse>>();

		internal List<List<LeaderboardResponse>> Leaderboards
		{
			get { return _leaderboards; }
		}

		[SerializeField]
		private LeaderboardListUserInterface _leaderboardListInterface;

		internal void CreateInterface(Canvas canvas)
		{
			bool inScene = _leaderboardListInterface.gameObject.scene == SceneManager.GetActiveScene();
			if (!inScene)
			{
				var newInterface = Instantiate(_leaderboardListInterface.gameObject, canvas.transform, false) as GameObject;
				newInterface.name = _leaderboardListInterface.name;
				_leaderboardListInterface = newInterface.GetComponent<LeaderboardListUserInterface>();
			}
			_leaderboardListInterface.gameObject.SetActive(false);
		}

		public void DisplayList(ActorType filter = ActorType.User)
		{
			GetLeaderboards(success =>
				{
					_leaderboardListInterface.Display(filter, success);
				});
		}

		private void GetLeaderboards(Action<bool> success)
		{
			_leaderboards.Clear();
			if (SUGARManager.CurrentUser != null)
			{
				SUGARManager.Client.Leaderboard.GetAsync(SUGARManager.GameId,
				response =>
				{
					var leaderboards = response.ToList();
					foreach (var actorType in (ActorType[])Enum.GetValues(typeof(ActorType)))
					{
						var at = actorType;
						_leaderboards.Add(leaderboards.Where(lb => lb.ActorType == at).ToList());
					}
					success(true);
				},
				exception =>
				{
					string error = "Failed to get leaderboard list. " + exception.Message;
					Debug.LogError(error);
					for (int i = 0; i < Enum.GetValues(typeof(ActorType)).Length; i++)
					{
						_leaderboards.Add(new List<LeaderboardResponse>());
					}
					success(false);
				});
			}
			else
			{
				for (int i = 0; i < Enum.GetValues(typeof(ActorType)).Length; i++)
				{
					_leaderboards.Add(new List<LeaderboardResponse>());
				}
				success(false);
			}
		}
	}
}
