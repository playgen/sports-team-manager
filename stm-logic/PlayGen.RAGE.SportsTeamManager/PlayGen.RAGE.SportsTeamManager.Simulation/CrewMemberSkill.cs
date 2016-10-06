using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	[Flags]
	public enum CrewMemberSkill
	{
		Body = 1,
		Charisma = 2,
		Perception = 4,
		Quickness = 8,
		Willpower = 16,
		Wisdom = 32,
	}

	public static class CrewMemberSkillsExtensions
	{
		public static bool RequiresSkills(this CrewMemberSkill stateLevel, CrewMemberSkill flag)
		{
			return (stateLevel & flag) == flag;
		}
	}
}
