using System.Collections.Generic;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Type of Boat
	/// </summary>
	public class BiggestDinghy : Boat
	{
		public BiggestDinghy(ConfigStore config) : base(config)
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
                        Name = "Navigator",
                        Description = "In charge of manoeuvres. Situational awareness and reactions are key to succeed in this position.",
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
                        Name = "Trimmer",
                        Description = "Responsible for controlling the shape of the sails. Attention to detail and durability will bring success to those in this position.",
                        RequiredSkills = new List<CrewMemberSkill>
                        {
                            CrewMemberSkill.Body,
                            CrewMemberSkill.Perception
                        }
                    }
                },
                new BoatPosition
                {
                    Position = new Position
                    {
                        Name = "Pitman",
                        Description = "Manages sail changes and can also hoist if needed. Strength, awareness and decent communication skills are required to do well in this role.",
                        RequiredSkills = new List<CrewMemberSkill>
                        {
                            CrewMemberSkill.Body,
                            CrewMemberSkill.Charisma,
                            CrewMemberSkill.Perception
                        }
                    }
                },
                new BoatPosition
                {
                    Position = new Position
                    {
                        Name = "Mid-Bowman",
                        Description = "Stows sails below deck and helps with sail changes. Crew positioned here will need to be physically fit, agile and able to deal with being in this hot, dark and wet role.",
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
