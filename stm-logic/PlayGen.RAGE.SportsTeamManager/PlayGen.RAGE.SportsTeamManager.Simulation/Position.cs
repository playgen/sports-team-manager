using System.Collections.Generic;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores position details and functionality
	/// </summary>
	public class Position
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public List<CrewMemberSkill> RequiredSkills { get; set; }

		/// <summary>
		/// Get the average skill rating for this CrewMember in this Position
		/// </summary>
		public int GetPositionRating(CrewMember crewMember)
		{
			int positionCount = 0;
			int crewScore = 0;
			foreach (CrewMemberSkill skill in RequiredSkills)
			{
				crewScore += crewMember.Skills[skill];
				positionCount++;
			}

			crewScore = crewScore / positionCount;

			return crewScore;
		}
	}
}