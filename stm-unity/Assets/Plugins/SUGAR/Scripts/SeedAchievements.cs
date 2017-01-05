#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using PlayGen.SUGAR.Client;
using PlayGen.SUGAR.Common.Shared;
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
			window.title = "Seed Game";
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

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "Tutorial Complete!",
				Description = "Complete the tutorial",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "Tutorial_Complete",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Tutorial Finished",
						ComparisonType = ComparisonType.Equals,
						CriteriaQueryType = CriteriaQueryType.Any,
						EvaluationDataType = EvaluationDataType.Boolean,
						Scope = CriteriaScope.Actor,
						Value = "true"
					}
				}
			});

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "Race Winner",
				Description = "Win a race",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "Race_Winner",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Race Position",
						ComparisonType = ComparisonType.Equals,
						CriteriaQueryType = CriteriaQueryType.Any,
						EvaluationDataType = EvaluationDataType.Long,
						Scope = CriteriaScope.Actor,
						Value = "1"
					}
				}
			});

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "Unhappy But Victorious",
				Description = "Win a race with an unhappy team",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "Unhappy_But_Victorious",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Race Position",
						ComparisonType = ComparisonType.Equals,
						CriteriaQueryType = CriteriaQueryType.Latest,
						EvaluationDataType = EvaluationDataType.Long,
						Scope = CriteriaScope.Actor,
						Value = "1"
					},
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Post Race Boat Average Mood",
						ComparisonType = ComparisonType.Less,
						CriteriaQueryType = CriteriaQueryType.Latest,
						EvaluationDataType = EvaluationDataType.Float,
						Scope = CriteriaScope.Actor,
						Value = "0"
					},
				}
			});

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "Five Hours Later...",
				Description = "Race for more than five hours total",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "Five_Hours_Later",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Race Time",
						ComparisonType = ComparisonType.Greater,
						CriteriaQueryType = CriteriaQueryType.Sum,
						EvaluationDataType = EvaluationDataType.Long,
						Scope = CriteriaScope.Actor,
						Value = "18000"
					}
				}
			});

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "Night And Day And So On",
				Description = "Race for more than 24 hours total",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "Night_And_Day_And_So_On",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Race Time",
						ComparisonType = ComparisonType.Greater,
						CriteriaQueryType = CriteriaQueryType.Sum,
						EvaluationDataType = EvaluationDataType.Long,
						Scope = CriteriaScope.Actor,
						Value = "86400"
					}
				}
			});

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "Positive Outlook",
				Description = "Resolve a post-race event with a positive outcome",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "Positive_Outlook",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Post Race Event Positive Outcome",
						ComparisonType = ComparisonType.Equals,
						CriteriaQueryType = CriteriaQueryType.Any,
						EvaluationDataType = EvaluationDataType.Boolean,
						Scope = CriteriaScope.Actor,
						Value = "true"
					}
				}
			});

			achievementClient.Create(new EvaluationCreateRequest
			{
				Name = "No Wasted Time",
				Description = "Have no talk time remaining after a race session",
				ActorType = ActorType.User,
				GameId = gameId,
				Token = "No_Wasted_Time",
				EvaluationCriterias = new List<EvaluationCriteriaCreateRequest>
				{
				new EvaluationCriteriaCreateRequest
					{
						EvaluationDataKey = "Time Remaining",
						ComparisonType = ComparisonType.Equals,
						CriteriaQueryType = CriteriaQueryType.Any,
						EvaluationDataType = EvaluationDataType.Long,
						Scope = CriteriaScope.Actor,
						Value = "0"
					}
				}
			});
		}

		private static void CreateLeaderboards()
		{
			var leaderboardClient = SUGARManager.Client.Leaderboard;
			var gameId = SUGARManager.GameId;

			leaderboardClient.Create(new LeaderboardRequest
			{
				Token = "Questions_Asked",
				GameId = gameId,
				Name = "Questions Asked",
				Key = "Meeting Question Asked",
				ActorType = ActorType.User,
				EvaluationDataType = EvaluationDataType.String,
				CriteriaScope = CriteriaScope.Actor,
				LeaderboardType = LeaderboardType.Count
			});

			leaderboardClient.Create(new LeaderboardRequest
			{
				Token = "Fastest_Time",
				GameId = gameId,
				Name = "Fastest Time",
				Key = "Race Time",
				ActorType = ActorType.User,
				EvaluationDataType = EvaluationDataType.Long,
				CriteriaScope = CriteriaScope.Actor,
				LeaderboardType = LeaderboardType.Lowest
			});

			/*leaderboardClient.Create(new LeaderboardRequest
			{
				Token = "Talk_Time_Used",
				GameId = gameId,
				Name = "Talk Time Used",
				Key = "Time Taken",
				ActorType = ActorType.User,
				EvaluationDataType = EvaluationDataType.Long,
				CriteriaScope = CriteriaScope.Actor,
				LeaderboardType = LeaderboardType.Cumulative
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
