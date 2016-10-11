namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class BoatPromotionTrigger
	{
		public string StartType { get; set; }
		public string NewType { get; set; }
		public int ScoreRequired { get; set; }
		public int ScoreMetSinceLast { get; set; }
	}
}
