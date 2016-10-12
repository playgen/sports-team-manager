namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class PostSessionEventTrigger
	{
		public string EventName { get; set; }
		public string StartBoatType { get; set; }
		public string EndBoatType { get; set; }
		public bool Random { get; set; }
		public int RaceTrigger { get; set; }
		public int SessionTrigger { get; set; }
		public int RepeatEvery { get; set; }
	}
}
