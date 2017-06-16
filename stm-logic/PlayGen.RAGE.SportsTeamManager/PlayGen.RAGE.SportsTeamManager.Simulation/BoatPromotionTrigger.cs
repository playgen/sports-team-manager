namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used in GameConfig to details when and how a boat should change type
	/// </summary>
	internal class BoatPromotionTrigger
	{
		public string StartType { get; set; }
		public string NewType { get; set; }
		public int ScoreRequired { get; set; }
		public int ScoreMetSinceLast { get; set; }
	}
}
