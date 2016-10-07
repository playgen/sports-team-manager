﻿using System.Collections.Generic;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Type of Boat
	/// </summary>
	public class Dinghy : Boat
	{
		public Dinghy(ConfigStore config) : base(config)
		{
			BoatPositions = new List<Position>()
			{
				Position.Skipper,
				Position.Navigator,
				Position.MidBowman
			};
		}
	}
}
