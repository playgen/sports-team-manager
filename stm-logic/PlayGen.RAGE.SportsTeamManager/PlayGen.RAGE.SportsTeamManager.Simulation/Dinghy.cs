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
						RequiresBody = false,
						RequiresCharisma = true,
						RequiresPerception = false,
						RequiresQuickness = false,
						RequiresWillpower = true,
						RequiresWisdom = true
					}
				},
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Navigator",
						RequiresBody = false,
						RequiresCharisma = false,
						RequiresPerception = true,
						RequiresQuickness = false,
						RequiresWillpower = false,
						RequiresWisdom = true
					}
				},
				new BoatPosition
				{
					Position = new Position
					{
						Name = "Mid-Bowman",
						RequiresBody = true,
						RequiresCharisma = false,
						RequiresPerception = false,
						RequiresQuickness = true,
						RequiresWillpower = true,
						RequiresWisdom = false
					}
				}
			};
		}
	}
}
