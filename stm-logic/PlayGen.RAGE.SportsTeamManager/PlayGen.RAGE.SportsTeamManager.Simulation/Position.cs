using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Position
	{
		public string Name { get; set; }
		public bool RequiresBody { get; set; }
		public bool RequiresCharisma { get; set; }
		public bool RequiresPerception { get; set; }
		public bool RequiresQuickness { get; set; }
		public bool RequiresWisdom { get; set; }
		public bool RequiresWillpower { get; set; }

		public int GetPositionRating(CrewMember crewMember)
		{
			int positionCount = 0;
			int crewScore = 0;
			if (RequiresBody)
			{
				crewScore += crewMember.Body;
				positionCount++;
			}
			if (RequiresCharisma)
			{
				crewScore += crewMember.Charisma;
				positionCount++;
			}
			if (RequiresPerception)
			{
				crewScore += crewMember.Perception;
				positionCount++;
			}
			if (RequiresQuickness)
			{
				crewScore += crewMember.Quickness;
				positionCount++;
			}
			if (RequiresWillpower)
			{
				crewScore += crewMember.Willpower;
				positionCount++;
			}
			if (RequiresWisdom)
			{
				crewScore += crewMember.Wisdom;
				positionCount++;
			}

			crewScore = crewScore / positionCount;

			return crewScore;
		}

		public string GetCrewMember(Boat boat)
		{
			string crewMember = "";
			var boatPosition = boat.BoatPositions.SingleOrDefault(bp => bp.Position == this);
			if (boatPosition != null)
			{
				var currentMember = boatPosition.CrewMember;
				if (currentMember != null)
				{
					crewMember = currentMember.Name;
				}
			}
			return crewMember;
		}
	}
}