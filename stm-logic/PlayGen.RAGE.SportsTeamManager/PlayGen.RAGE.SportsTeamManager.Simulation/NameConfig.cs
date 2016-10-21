using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class NameConfig
	{
		public Dictionary<string, List<string>> MaleForename;
		public Dictionary<string, List<string>> FemaleForename;
		public Dictionary<string, List<string>> Surname;

		//get and return values for avatar configs
		public NameConfig GetConfig()
		{
			var configText = Templates.ResourceManager.GetString("name_config");
			var config = JsonConvert.DeserializeObject<NameConfig>(configText);
			return config;
		}
	}
}
