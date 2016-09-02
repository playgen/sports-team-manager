using System.Collections.Generic;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Type of Boat
	/// </summary>
	public class AltLargeDinghy : Boat
	{
		public AltLargeDinghy(ConfigStore config) : base(config)
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
						Name = "Bowman",
						RequiredSkills = new List<CrewMemberSkill>
						{
							CrewMemberSkill.Perception,
							CrewMemberSkill.Quickness,
							CrewMemberSkill.Willpower
						}
					}
				},
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Pitman",
						RequiredSkills = new List<CrewMemberSkill>
						{
							CrewMemberSkill.Body,
							CrewMemberSkill.Charisma,
							CrewMemberSkill.Perception
						}
					}
				}
			};
		}
	}
}
