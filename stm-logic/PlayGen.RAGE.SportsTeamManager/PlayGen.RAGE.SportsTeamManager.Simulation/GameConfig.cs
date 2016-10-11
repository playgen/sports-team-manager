using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class GameConfig
	{
		public BoatPromotionTrigger[] PromotionTriggers { get; set; }
		public EventTrigger[] EventTriggers { get; set; }

		//get and return values for avatar configs
		public GameConfig GetConfig()
		{
			var configText = Templates.ResourceManager.GetString("game_config");
			var config = JsonConvert.DeserializeObject<GameConfig>(configText);
			return config;
		}
	}
}
