using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	[Flags]
	public enum Position
	{
		Null = 0,
		[RequiredSkills(CrewMemberSkill.Charisma | CrewMemberSkill.Willpower | CrewMemberSkill.Wisdom)]
		Skipper,
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Quickness)]
		Helmsman,
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Quickness | CrewMemberSkill.Willpower)]
		MidBowman,
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Wisdom)]
		Navigator,
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Charisma | CrewMemberSkill.Perception)]
		Pitman,
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Perception)]
		Trimmer,
		[RequiredSkills(CrewMemberSkill.Body)]
		Grinder,
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Quickness | CrewMemberSkill.Willpower)]
		Bowman
	}
}
