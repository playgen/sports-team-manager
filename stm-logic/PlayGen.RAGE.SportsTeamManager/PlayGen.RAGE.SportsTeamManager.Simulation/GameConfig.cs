using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal class GameConfig
	{
		public BoatPromotionTrigger[] PromotionTriggers { get; set; }
		public PostSessionEventTrigger[] EventTriggers { get; set; }

		//get and return values for the game config, including when to change boat types and trigger post-race events.
		internal GameConfig GetConfig()
		{
			var configText = Templates.ResourceManager.GetString("game_config");
			var config = JsonConvert.DeserializeObject<GameConfig>(configText);
			return config;
		}
	}
}
