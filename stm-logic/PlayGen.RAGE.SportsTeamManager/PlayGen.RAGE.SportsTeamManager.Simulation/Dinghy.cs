using System.Collections.Generic;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Type of Boat
	/// </summary>
	public class Dinghy : Boat
	{
		public Dinghy(ConfigStore config) : base(config)
		{
			BoatPositions = new List<BoatPosition>()
			{
				new BoatPosition
				{
					Position = Position.Skipper
				},
				new BoatPosition
				{
					Position = Position.Navigator
				},
				new BoatPosition
				{
					Position = Position.MidBowman
				}
			};
		}
	}
}
