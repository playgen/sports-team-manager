namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class ConfigExtensions
	{
		internal static float GetValue(this ConfigKey key)
		{
			return ConfigStore.ConfigValues[key];
		}

		internal static int GetIntValue(this ConfigKey key)
		{
			return (int)ConfigStore.ConfigValues[key];
		}
	}
}
