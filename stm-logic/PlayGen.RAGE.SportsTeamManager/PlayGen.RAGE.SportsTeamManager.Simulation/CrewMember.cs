﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class CrewMember
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public int Body { get; set; }
		public int Charisma { get; set; }
		public int Perception { get; set; }
		public int Quickness { get; set; }
		public int Wisdom { get; set; }
		public int Willpower { get; set; }
		public List<CrewOpinion> CrewOpinions { get; set; } = new List<CrewOpinion>();
	}
}
