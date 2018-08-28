using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// NameConfig json is converted into class of this type. Stores all possible names for each available language.
	/// </summary>
	internal class NameConfig
	{
		public Dictionary<string, List<string>> MaleForename { get; set; }
		public Dictionary<string, List<string>> FemaleForename { get; set; }
		public Dictionary<string, List<string>> Surname { get; set; }

		/// <summary>
		/// Get and return possible names for each language
		/// </summary>
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
