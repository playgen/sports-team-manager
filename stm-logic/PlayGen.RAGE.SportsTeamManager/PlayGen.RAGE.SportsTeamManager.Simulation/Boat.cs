using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Boat
	{
		public string Name { get; set; }
		public List<BoatPosition> BoatPositions { get; set; }
		public int BoatScore { get; set; }

		public void AssignCrew(BoatPosition boatPosition, CrewMember crewMember)
		{
			var current = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (current != null)
			{
				RemoveCrew(current);
			}
			boatPosition.CrewMember = crewMember;
			boatPosition.PositionScore = boatPosition.CrewMemberScore();
			BoatScore = this.BoatScore();
		}

		public void RemoveCrew(BoatPosition boatPosition)
		{
			boatPosition.CrewMember = null;
			boatPosition.PositionScore = boatPosition.CrewMemberScore();
			BoatScore = this.BoatScore();
		}
	}
}
