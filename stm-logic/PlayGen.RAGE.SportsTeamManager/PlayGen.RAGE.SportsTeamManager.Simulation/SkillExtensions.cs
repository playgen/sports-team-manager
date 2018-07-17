namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class SkillExtensions
	{
		/// <summary>
		/// Extension method that gets the skills required for a position
		/// </summary>
		internal static bool ContainsSkill(this Skill stateLevel, Skill flag)
		{
			return (stateLevel & flag) == flag;
		}
	}
}
