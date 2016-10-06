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
		public Dictionary<ConfigKeys, float> ConfigValues { get; set; }

		public ConfigStore()
		{
			ConfigValues = new Dictionary<ConfigKeys, float>();
			var configText = Templates.ResourceManager.GetString("config");
			ConfigValues = JsonConvert.DeserializeObject<Dictionary<ConfigKeys, float>>(configText);
			foreach (var key in Enum.GetValues(typeof(ConfigKeys)) as ConfigKeys[])
			{
				if (!ConfigValues.ContainsKey(key))
				{
					throw new Exception("Config key " + key + " not included in config!");
				}
			}
		}
	}
}
