using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class BoatPosition
	{
		public Position Position { get; set; }
		public CrewMember CrewMember { get; set; }
		public int PositionScore { get; set; }

		public void UpdateCrewMemberScore(Boat boat)
		{
			if (CrewMember == null || Position == null)
			{
				PositionScore = 0;
				return;
			}
			int positionCount = 0;
			int crewScore = 0;
			if (Position.RequiresBody)
			{
				crewScore += CrewMember.Body;
				positionCount++;
			}
			if (Position.RequiresCharisma)
			{
				crewScore += CrewMember.Charisma;
				positionCount++;
			}
			if (Position.RequiresPerception)
			{
				crewScore += CrewMember.Perception;
				positionCount++;
			}
			if (Position.RequiresQuickness)
			{
				crewScore += CrewMember.Quickness;
				positionCount++;
			}
			if (Position.RequiresWillpower)
			{
				crewScore += CrewMember.Willpower;
				positionCount++;
			}
			if (Position.RequiresWisdom)
			{
				crewScore += CrewMember.Wisdom;
				positionCount++;
			}

			crewScore = crewScore / positionCount;

			int opinion = 0;
			int opinionCount = 0;
			int managerOpinion = 0;
			if (CrewMember.CrewOpinions != null && CrewMember.CrewOpinions.Count > 0)
			{
				foreach (BoatPosition bp in boat.BoatPositions)
				{
					if (bp != this && bp.CrewMember != null)
					{
						var crewMember = CrewMember.CrewOpinions.SingleOrDefault(op => op.Person == bp.CrewMember);
						if (crewMember != null)
						{
							opinion += crewMember.Opinion;
						}
						opinionCount++;
					}
				}
				var manager = CrewMember.CrewOpinions.SingleOrDefault(op => op.Person == boat.Manager);
				if (manager != null)
				{
					managerOpinion += manager.Opinion;
				}
			}

			if (opinionCount > 0)
			{
				opinion = opinion / opinionCount;
			}
			crewScore += opinion;

			crewScore += managerOpinion;

			crewScore += CrewMember.GetMood();

			PositionScore = crewScore;
		}
	}
}
