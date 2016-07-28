using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Dinghy : Boat
	{
		public Dinghy()
		{
			Position skipper = new Position
			{
				Name = "Skipper",
				RequiresBody = false,
				RequiresCharisma = true,
				RequiresPerception = false,
				RequiresQuickness = false,
				RequiresWillpower = true,
				RequiresWisdom = true
			};

			Position navigator = new Position
			{
				Name = "Navigator",
				RequiresBody = false,
				RequiresCharisma = false,
				RequiresPerception = true,
				RequiresQuickness = false,
				RequiresWillpower = false,
				RequiresWisdom = true
			};

			Position midbow = new Position
			{
				Name = "Mid-Bowman",
				RequiresBody = true,
				RequiresCharisma = false,
				RequiresPerception = false,
				RequiresQuickness = true,
				RequiresWillpower = true,
				RequiresWisdom = false
			};
			BoatPositions = new List<BoatPosition>()
			{
				new BoatPosition
				{
					Position = skipper,
				},
				new BoatPosition
				{
					Position = navigator,
				},
				new BoatPosition
				{
					Position = midbow,
				},
			};
		}
	}
}
