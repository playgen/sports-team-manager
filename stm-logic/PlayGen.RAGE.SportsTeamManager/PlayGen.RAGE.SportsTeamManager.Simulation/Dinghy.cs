using System.Collections.Generic;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Type of Boat
	/// </summary>
	public class Dinghy : Boat
	{
		public Dinghy()
		{
			BoatPositions = new List<BoatPosition>()
			{
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Skipper",
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
						Name = "Navigator",
						RequiredSkills = new List<CrewMemberSkill>
						{
							CrewMemberSkill.Perception,
							CrewMemberSkill.Wisdom
						}
					}
				},
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Mid-Bowman",
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
