using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Position
	{
		public string Name { get; set; }
		public bool RequiresBody { get; set; }
		public bool RequiresCharisma { get; set; }
		public bool RequiresPerception { get; set; }
		public bool RequiresQuickness { get; set; }
		public bool RequiresWisdom { get; set; }
		public bool RequiresWillpower { get; set; }
	}
}
