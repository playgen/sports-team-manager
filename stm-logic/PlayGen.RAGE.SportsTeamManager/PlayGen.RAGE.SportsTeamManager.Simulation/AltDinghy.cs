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
			BoatPositions = new List<Position>
			{
				Position.Skipper,
				Position.Helmsman,
				Position.MidBowman
			};
		}
	}
}
