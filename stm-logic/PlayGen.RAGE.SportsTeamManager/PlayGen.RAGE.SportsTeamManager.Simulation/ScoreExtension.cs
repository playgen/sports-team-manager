using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class ScoreExtension
	{
		public static int BoatScore(this Boat boat)
		{
			int score = boat.BoatPositions.Sum(bp => bp.PositionScore);
			return score;
		}

		public static int CrewMemberScore(this BoatPosition bp)
		{
			if (bp.CrewMember == null)
			{
				return 0;
			}
			int positionCount = 0;
			int crewScore = 0;
			if (bp.Position.RequiresBody)
			{
				crewScore += bp.CrewMember.Body;
				positionCount++;
			}
			if (bp.Position.RequiresCharisma)
			{
				crewScore += bp.CrewMember.Charisma;
				positionCount++;
			}
			if (bp.Position.RequiresPerception)
			{
				crewScore += bp.CrewMember.Perception;
				positionCount++;
			}
			if (bp.Position.RequiresQuickness)
			{
				crewScore += bp.CrewMember.Quickness;
				positionCount++;
			}
			if (bp.Position.RequiresWillpower)
			{
				crewScore += bp.CrewMember.Willpower;
				positionCount++;
			}
			if (bp.Position.RequiresWisdom)
			{
				crewScore += bp.CrewMember.Wisdom;
				positionCount++;
			}

			crewScore = crewScore / positionCount;

			return crewScore;
		}
	}
}
