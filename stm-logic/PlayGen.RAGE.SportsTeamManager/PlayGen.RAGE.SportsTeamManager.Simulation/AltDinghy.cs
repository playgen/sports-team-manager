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
						Description = "",
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
						Description = "",
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
						Description = "",
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
