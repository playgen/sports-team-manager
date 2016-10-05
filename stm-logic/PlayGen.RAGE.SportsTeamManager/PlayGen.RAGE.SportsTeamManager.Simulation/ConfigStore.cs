using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access values related to functionality
	/// </summary>
	public class ConfigStore
	{
		public Dictionary<string, float> ConfigValues { get; set; }

		public ConfigStore()
		{
			ConfigValues = new Dictionary<string, float>();
			var configText = Templates.ResourceManager.GetString("config");
			ConfigValues = JsonConvert.DeserializeObject<Dictionary<string, float>>(configText);
			foreach (var key in Enum.GetNames(typeof(ConfigKeys)))
			{
				if (!ConfigValues.Keys.Contains(key))
				{
					throw new Exception("Config key " + key + " not included in config!");
				}
			}
		}
	}
}
