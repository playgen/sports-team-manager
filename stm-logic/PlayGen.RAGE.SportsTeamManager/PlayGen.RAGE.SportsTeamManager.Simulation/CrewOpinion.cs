namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Storage of a CrewMember's opinion on another Person
	/// </summary>
	public class CrewOpinion
	{
		public Person Person { get; set; }
		public int Opinion { get; set; }
	}
}