namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used by the GameConfig to determine when a post-race event should be triggered.
	/// </summary>
	internal class PostSessionEventTrigger
	{
		internal string EventName { get; set; }
		internal string StartBoatType { get; set; }
		internal string EndBoatType { get; set; }
		internal bool Random { get; set; }
		internal int RaceTrigger { get; set; }
		internal int RepeatEvery { get; set; }
	}
}
