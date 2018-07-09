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
		[RequiredSkills(CrewMemberSkill.Charisma | CrewMemberSkill.Willpower | CrewMemberSkill.Wisdom)]
		Skipper = 1,
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Quickness)]
		Helmsman = 2,
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Quickness | CrewMemberSkill.Willpower)]
		MidBowman = 4,
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Wisdom)]
		Navigator = 8,
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Charisma | CrewMemberSkill.Perception)]
		Pitman = 16,
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Perception)]
		Trimmer = 32,
		[RequiredSkills(CrewMemberSkill.Body)]
		Grinder = 64,
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Quickness | CrewMemberSkill.Willpower)]
		Bowman = 128
	}
}
