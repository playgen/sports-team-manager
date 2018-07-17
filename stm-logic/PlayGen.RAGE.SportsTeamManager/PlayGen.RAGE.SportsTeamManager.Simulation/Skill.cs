using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Enum of Crew Member Skills
	/// </summary>
	[Flags]
	public enum Skill
	{
		Body = 1,
		Charisma = 2,
		Perception = 4,
		Quickness = 8,
		Willpower = 16,
		Wisdom = 32
	}
}
