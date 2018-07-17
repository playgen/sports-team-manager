namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used in GameConfig to details when and how a boat should change type
	/// </summary>
	internal class BoatPromotionTrigger
	{
		internal string StartType { get; set; }
		internal string NewType { get; set; }
		internal int ScoreRequired { get; set; }
		internal int ScoreMetSinceLast { get; set; }
	}
}
