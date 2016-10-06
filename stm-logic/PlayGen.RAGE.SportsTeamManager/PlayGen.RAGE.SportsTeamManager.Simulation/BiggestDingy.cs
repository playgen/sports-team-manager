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
                    Position = Position.Skipper
				},
                new BoatPosition
                {
                    Position = Position.Navigator
				},
                new BoatPosition
                {
                    Position = Position.Helmsman
				},
                new BoatPosition
                {
                    Position = Position.Trimmer
				},
                new BoatPosition
                {
                    Position = Position.Pitman
				},
                new BoatPosition
                {
                    Position = Position.MidBowman
				}
            };
        }
	}
}
