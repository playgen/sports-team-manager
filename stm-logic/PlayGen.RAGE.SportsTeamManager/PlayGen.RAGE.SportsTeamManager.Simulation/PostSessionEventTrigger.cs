namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used by the GameConfig to determine when a post-race event should be triggered.
	/// </summary>
	internal class PostSessionEventTrigger
	{
		public string EventName { get; set; }
		public string StartBoatType { get; set; }
		public string EndBoatType { get; set; }
		public bool Random { get; set; }
		public int RaceTrigger { get; set; }
		public int RepeatEvery { get; set; }
	}
}
