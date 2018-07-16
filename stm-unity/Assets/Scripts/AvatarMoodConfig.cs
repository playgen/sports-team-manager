using System;
using System.Collections.Generic;

public class AvatarMoodConfig
{
	public enum AvatarMood
	{
		StronglyAgree,
		Agree,
		Neutral,
		Disagree,
		StronglyDisagree
	}

	private static Dictionary<AvatarMood, int> AvatarMoodMapping = new Dictionary<AvatarMood, int>
	{
		{AvatarMood.StronglyAgree, 3},
		{AvatarMood.Agree, 1},
		{AvatarMood.Neutral, 0},
		{AvatarMood.Disagree, -1},
		{AvatarMood.StronglyDisagree, -3},
	};

	public static int GetMood(string mood)
	{
		var avatarMood = (AvatarMood)Enum.Parse(typeof(AvatarMood), mood);
		return GetMood(avatarMood);
	}

	public static int GetMood(AvatarMood mood)
	{
		if (AvatarMoodMapping.ContainsKey(mood))
		{
			return AvatarMoodMapping[mood];
		}
		UnityEngine.Debug.LogWarning($"{mood} not found, returning 0");
		return 0;
	}

	public static string GetMood(float mood)
	{
		if (mood == AvatarMoodMapping[AvatarMood.Neutral])
		{
			return AvatarMood.Neutral.ToString();
		}

		if (mood > AvatarMoodMapping[AvatarMood.Neutral])
		{
			// Positive
			return mood < AvatarMoodMapping[AvatarMood.StronglyAgree] ? AvatarMood.Agree.ToString() : AvatarMood.StronglyAgree.ToString();
		}
		else
		{
			// Negative
			return mood > AvatarMoodMapping[AvatarMood.StronglyDisagree] ? AvatarMood.Disagree.ToString() : AvatarMood.StronglyDisagree.ToString();
		}
	}
}