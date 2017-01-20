using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal class NameConfig
	{
		public Dictionary<string, List<string>> MaleForename { get; set; }
		public Dictionary<string, List<string>> FemaleForename { get; set; }
		public Dictionary<string, List<string>> Surname { get; set; }

		//get and return values for avatar configs
		internal NameConfig GetConfig()
		{
			var configText = Templates.ResourceManager.GetString("name_config");
			var config = JsonConvert.DeserializeObject<NameConfig>(configText);
			return config;
		}
	}
}
