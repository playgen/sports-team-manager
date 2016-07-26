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
		public List<BoatPosition> BoatPositions { get; set; } = new List<BoatPosition>();
		public int BoatScore { get; set; }

		public void AssignCrew(BoatPosition boatPosition, CrewMember crewMember)
		{
			var current = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (current != null)
			{
				RemoveCrew(current);
			}
			if (boatPosition != null)
			{
				boatPosition.CrewMember = crewMember;
			}
			UpdateBoatScore();
		}

		public void RemoveCrew(BoatPosition boatPosition)
		{
			boatPosition.CrewMember = null;
		}

		public void UpdateBoatScore()
		{
			foreach (BoatPosition bp in BoatPositions)
			{
				UpdateCrewMemberScore(bp);
			}
			BoatScore = BoatPositions.Sum(bp => bp.PositionScore);
		}

		public void UpdateCrewMemberScore(BoatPosition boatPosition)
		{
			if (boatPosition.CrewMember == null)
			{
				boatPosition.PositionScore = 0;
				return;
			}
			int positionCount = 0;
			int crewScore = 0;
			if (boatPosition.Position.RequiresBody)
			{
				crewScore += boatPosition.CrewMember.Body;
				positionCount++;
			}
			if (boatPosition.Position.RequiresCharisma)
			{
				crewScore += boatPosition.CrewMember.Charisma;
				positionCount++;
			}
			if (boatPosition.Position.RequiresPerception)
			{
				crewScore += boatPosition.CrewMember.Perception;
				positionCount++;
			}
			if (boatPosition.Position.RequiresQuickness)
			{
				crewScore += boatPosition.CrewMember.Quickness;
				positionCount++;
			}
			if (boatPosition.Position.RequiresWillpower)
			{
				crewScore += boatPosition.CrewMember.Willpower;
				positionCount++;
			}
			if (boatPosition.Position.RequiresWisdom)
			{
				crewScore += boatPosition.CrewMember.Wisdom;
				positionCount++;
			}

			crewScore = crewScore / positionCount;

			int opinion = 0;
			int opinionCount = 0;
			if (boatPosition.CrewMember.CrewOpinions != null && boatPosition.CrewMember.CrewOpinions.Count > 0)
			{
				foreach (BoatPosition bp in BoatPositions)
				{
					if (bp != boatPosition && bp.CrewMember != null)
					{
						opinion += boatPosition.CrewMember.CrewOpinions.SingleOrDefault(op => op.CrewMember == bp.CrewMember).Opinion;
						opinionCount++;
					}
				}
			}

			if (opinionCount > 0)
			{
				opinion = opinion / opinionCount;
			}
			crewScore += opinion;

			boatPosition.PositionScore = crewScore;
		}
	}
}