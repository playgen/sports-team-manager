using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class NameExtensions
	{
		internal static Name ToName(this string value)
		{
			return Name.BuildName(value);
		}

		/// <summary>
		/// Extension for removing spaces from a string.
		/// </summary>
		public static string NoSpaces(this string value)
		{
			return value.Replace(" ", string.Empty);
		}
	}
}
