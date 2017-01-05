using System;
using PlayGen.SUGAR.Client;
using PlayGen.SUGAR.Contracts.Shared;

namespace PlayGen.SUGAR.Unity
{
	public static class SUGARManager
	{
		internal static SUGARUnityManager Unity { get; set; }

		internal static SUGARClient Client { get; set; }

		public static int GameId { get; internal set; }

		public static ActorResponse CurrentUser { get; internal set; }

	    internal static AccountUnityClient account { get; set; }

	    internal static AchievementUnityClient achievement { get; set; }

		internal static GameDataUnityClient gameData = new GameDataUnityClient();

        internal static LeaderboardListUnityClient gameLeaderboard { get; set; }

		internal static LeaderboardUnityClient leaderboard { get; set; }

        internal static Config config { get; set; }

		public static AccountUnityClient Account
		{
			get
			{
				if (account != null)
				{
					return account;
				} 
				throw new Exception("SUGAR GameObject needs to be active to access Account");
			}
		}

		public static AchievementUnityClient Achievement
		{
			get
			{
				if (achievement != null)
				{
					return achievement;
				}
				throw new Exception("Achievements are currently disabled in the SUGAR Unity Manager");
			}
		}

		public static GameDataUnityClient GameData
		{
			get
			{
				if (gameData != null)
				{
					return gameData;
				}
				throw new Exception("SUGAR GameObject needs to be active to access GameData");
			}
		}

		public static LeaderboardListUnityClient GameLeaderboard
		{
			get
			{
				if (gameLeaderboard != null)
				{
					return gameLeaderboard;
				}
				throw new Exception("Leaderboards are currently disabled in the SUGAR Unity Manager");
			}
		}

		public static LeaderboardUnityClient Leaderboard
		{
			get
			{
				if (leaderboard != null)
				{
					return leaderboard;
				}
				throw new Exception("Leaderboards are currently disabled in the SUGAR Unity Manager");
			}
		}

        internal static bool Register(SUGARUnityManager unityManager)
		{
			return Client == null;
		}
	}
}
