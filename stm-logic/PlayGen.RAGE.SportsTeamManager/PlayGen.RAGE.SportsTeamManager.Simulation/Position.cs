using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Enum of possible boat positions.
	/// </summary>
	[Flags]
	public enum Position
	{
		Null = 0,
		[RequiredSkills(Skill.Charisma | Skill.Willpower | Skill.Wisdom)]
		Skipper = 1,
		[RequiredSkills(Skill.Perception | Skill.Quickness)]
		Helmsman = 2,
		[RequiredSkills(Skill.Body | Skill.Quickness | Skill.Willpower)]
		MidBowman = 4,
		[RequiredSkills(Skill.Perception | Skill.Wisdom)]
		Navigator = 8,
		[RequiredSkills(Skill.Body | Skill.Charisma | Skill.Perception)]
		Pitman = 16,
		[RequiredSkills(Skill.Body | Skill.Perception)]
		Trimmer = 32,
		[RequiredSkills(Skill.Body)]
		Grinder = 64,
		[RequiredSkills(Skill.Perception | Skill.Quickness | Skill.Willpower)]
		Bowman = 128
	}
}
