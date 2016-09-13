using System.Collections.Generic;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Type of Boat
	/// </summary>
	public class AltDinghy : Boat
	{
		public AltDinghy(ConfigStore config) : base(config)
		{
			BoatPositions = new List<BoatPosition>()
			{
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Skipper",
						Description = "The team captain. They'll have to be able to lead the crew and handle the pressure of doing so, all the while knowing how to react in tough situations.",
						RequiredSkills = new List<CrewMemberSkill>
						{
							CrewMemberSkill.Charisma,
							CrewMemberSkill.Willpower,
							CrewMemberSkill.Wisdom
						}
					}
				},
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Helmsman",
						Description = "Steers the ship. The ability to think ahead and react as soon as possible is needed here.",
						RequiredSkills = new List<CrewMemberSkill>
						{
							CrewMemberSkill.Perception,
							CrewMemberSkill.Quickness
						}
					}
				},
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Mid-Bowman",
						Description = "Stows sails below deck and helps with sail changes. Crew positioned here will need to be physically fit, agile and able to deal with being in this hot, dark and wet position.",
						RequiredSkills = new List<CrewMemberSkill>
						{
							CrewMemberSkill.Body,
							CrewMemberSkill.Quickness,
							CrewMemberSkill.Willpower
						}
					}
				}
			};
		}
	}
}
