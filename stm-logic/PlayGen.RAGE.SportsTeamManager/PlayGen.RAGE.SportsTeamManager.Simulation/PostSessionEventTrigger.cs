namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
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
