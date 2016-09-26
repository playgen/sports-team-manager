using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to store CrewMember for a Position and the current score for that Position
	/// </summary>
	public class BoatPosition
	{
		public Position Position { get; set; }
		public CrewMember CrewMember { get; set; }
		public int PositionScore { get; set; }

		/// <summary>
		/// Get the current score for this Position on this Boat for this CrewMember
		/// </summary>
		public void UpdateCrewMemberScore(Boat boat, ConfigStore config)
		{
            //set score as 0 if no Position or CrewMember is provided
			if (CrewMember == null || Position == null)
			{
				PositionScore = 0;
				return;
			}
            //Get the averafe skill rating for this CrewMember in this Position
			int crewScore = Position.GetPositionRating(CrewMember);

			int opinion = 0;
			int opinionCount = 0;
			int managerOpinion = 0;

            //get the average opinion of every other positioned crew member and the manager
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

            //add average opinion, manager opinion and current mood to score
			crewScore += (int)(opinion * config.ConfigValues[ConfigKeys.OpinionRatingWeighting.ToString()]);

			crewScore += (int)(managerOpinion * config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting.ToString()]);

			crewScore += (int)(CrewMember.GetMood() * config.ConfigValues[ConfigKeys.MoodRatingWeighting.ToString()]);

			PositionScore = crewScore;
		}
	}
}
