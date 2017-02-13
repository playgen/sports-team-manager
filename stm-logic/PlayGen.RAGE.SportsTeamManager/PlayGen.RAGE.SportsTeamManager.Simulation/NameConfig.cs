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
			foreach (var key in config.MaleForename.Keys)
			{
				config.MaleForename[key].Sort();
			}
			foreach (var key in config.FemaleForename.Keys)
			{
				config.FemaleForename[key].Sort();
			}
			foreach (var key in config.Surname.Keys)
			{
				config.Surname[key].Sort();
			}
			return config;
		}
	}
}
