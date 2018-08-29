namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Enum of possible strings used as a post race event impact
	/// </summary>
	public enum PostRaceEventImpact
	{
		MoodChange,
		ExpectedPosition,
		ExpectedPositionAfter,
		ManagerOpinionWorse,
		ManagerOpinionAllCrewWorse,
		ManagerOpinionBetter,
		ManagerOpinionAllCrewBetter,
		ManagerOpinionMuchBetter,
		ManagerOpinionMuchWorse,
		RevealTwoSkills,
		RevealFourSkills,
		ImproveConflictOpinionGreatly,
		ImproveConflictTeamOpinion,
		ImproveConflictKnowledge,
		CausesSelectionAfter,
		WholeTeamChange
	}
}