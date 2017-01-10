#if UNITY_EDITOR
using System;
using System.Linq;

using PlayGen.SUGAR.Client;
using PlayGen.SUGAR.Contracts.Shared;
using UnityEditor;
using UnityEngine;

namespace PlayGen.SUGAR.Unity
{
	public static class SeedAchievements
	{
		[MenuItem("Tools/SUGAR/Seed Game")]
		public static void SeedAchivements()
		{
			AdminLogIn window = ScriptableObject.CreateInstance<AdminLogIn>();
			window.titleContent.text = "Seed Game";
			window.Show();
		}

		public static void LogInUser(string username, string password)
		{
			var unityManager = GameObject.FindObjectsOfType(typeof(SUGARUnityManager)).FirstOrDefault() as SUGARUnityManager;
			if (unityManager == null)
			{
				return;
			}
			SUGARManager.Client = new SUGARClient(unityManager.baseAddress);
			var response = LoginAdmin(username, password);
			if (response != null)
			{
				Debug.Log("Admin Login SUCCESS");
				var game = SUGARManager.Client.Game.Get(unityManager.gameToken).FirstOrDefault();
				if (game != null)
				{
					Debug.Log("Game Found");
					unityManager.gameId = game.Id;
					SUGARManager.GameId = game.Id;
				}
				else
				{
					Debug.Log("Creating Game");
					var gameResponse = SUGARManager.Client.Game.Create(new GameRequest()
					{
						Name = unityManager.gameToken
					});
					if (gameResponse != null)
					{
						unityManager.gameId = gameResponse.Id;
						SUGARManager.GameId = gameResponse.Id;
					}
					else
					{
						Debug.LogError("Unable to create game");
						return;
					}

				}
				CreateAchievements();
				CreateLeaderboards();
				SUGARManager.Client.Session.Logout();
			}
		}

		private static void CreateAchievements()
		{
			var achievementClient = SUGARManager.Client.Achievement;
			var gameId = SUGARManager.GameId;

			/*achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "",
				Description = "",
				ActorType = ActorType.,
				GameId = gameId,
				Token = "",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						Key = "",
						ComparisonType = ComparisonType.,
						CriteriaQueryType = CriteriaQueryType.,
						DataType = EvaluationDataType.,
						Scope = CriteriaScope.,
						Value = ""
					}
				}
			});*/
		}

		private static void CreateLeaderboards()
		{
			var leaderboardClient = SUGARManager.Client.Leaderboard;
			var gameId = SUGARManager.GameId;

			/*leaderboardClient.Create(new LeaderboardRequest
			{
				Token = "",
				GameId = gameId,
				Name = "",
				Key = "",
				ActorType = ActorType.,
				EvaluationDataType = EvaluationDataType.,
				CriteriaScope = CriteriaScope.,
				LeaderboardType = LeaderboardType.
			});*/
		}

		private static AccountResponse LoginAdmin(string username, string password)
		{
			try
			{
				return SUGARManager.Client.Session.Login(new AccountRequest()
				{
					Name = username,
					Password = password,
					SourceToken = "SUGAR"
				});
			}
			catch (Exception ex)
			{
				Debug.Log("Error Logging in Admin");
				Debug.Log(ex.Message);
				return null;
			}
		}
	}

	public class AdminLogIn : EditorWindow
	{
		string username;
		string password;

		void OnGUI()
		{
			username = EditorGUILayout.TextField("Username", username, EditorStyles.textField);
			password = EditorGUILayout.PasswordField("Password", password);
			if (GUILayout.Button("Sign-in"))
			{
				SeedAchievements.LogInUser(username, password);
			}
		}
	}
}
#endif
