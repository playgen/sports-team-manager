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
					Position = Position.Skipper
				},
				new BoatPosition
				{
					Position = Position.Helmsman
				},
				new BoatPosition
				{
					Position = Position.MidBowman
				}
			};
		}
	}
}
