using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	[Flags]
	public enum Position
	{
		[Name("No Position")]
		[Description("")]
		Null = 0,
		[Description("The team captain. They'll have to be able to lead the crew and handle the pressure of doing so, all the while knowing how to react in tough situations.")]
		[RequiredSkills(CrewMemberSkill.Charisma | CrewMemberSkill.Willpower | CrewMemberSkill.Wisdom)]
		Skipper,
		[Description("Steers the ship. The ability to think ahead and react as soon as possible is needed here.")]
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Quickness)]
		Helmsman,
		[Name("Mid-Bowman")]
		[Description("Stows sails below deck and helps with sail changes. Crew positioned here will need to be physically fit, agile and able to deal with being in this hot, dark and wet position.")]
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Quickness | CrewMemberSkill.Willpower)]
		MidBowman,
		[Description("In charge of manoeuvres. Situational awareness and reactions are key to succeed in this position.")]
		[RequiredSkills(CrewMemberSkill.Perception | CrewMemberSkill.Wisdom)]
		Navigator,
		[Description("Manages sail changes and can also hoist if needed. Strength, awareness and decent communication skills are required to do well in this role.")]
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Charisma | CrewMemberSkill.Perception)]
		Pitman,
		[Description("Responsible for controlling the shape of the sails. Attention to detail and durability will bring success to those in this position.")]
		[RequiredSkills(CrewMemberSkill.Body | CrewMemberSkill.Perception)]
		Trimmer
	}
}
