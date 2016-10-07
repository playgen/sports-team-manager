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
			BoatPositions = new List<Position>()
			{
				Position.Skipper,
				Position.Navigator,
				Position.Helmsman,
				Position.Trimmer,
				Position.Pitman,
				Position.MidBowman
			};
		}
	}
}
