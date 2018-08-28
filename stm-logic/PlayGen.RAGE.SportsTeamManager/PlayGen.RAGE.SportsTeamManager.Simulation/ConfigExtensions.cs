namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class ConfigExtensions
	{
		/// <summary>
		/// Get the float config value for this key
		/// </summary>
		internal static float GetValue(this ConfigKey key)
		{
			return ConfigStore.ConfigValues[key];
		}

		/// <summary>
		/// Get the int config value for this key
		/// </summary>
		internal static int GetIntValue(this ConfigKey key)
		{
			return (int)ConfigStore.ConfigValues[key];
		}
	}
}
