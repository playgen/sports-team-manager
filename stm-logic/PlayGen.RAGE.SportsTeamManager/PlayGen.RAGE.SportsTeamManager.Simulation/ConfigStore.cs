using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class ConfigStore
	{
		public Dictionary<string, float> ConfigValues { get; set; }

		public ConfigStore()
		{
			ConfigValues = new Dictionary<string, float>();
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			string configText = Templates.ResourceManager.GetString("config");
			ConfigValues = JsonConvert.DeserializeObject<Dictionary<string, float>>(configText);
			foreach (string key in Enum.GetNames(typeof(ConfigKeys)))
			{
				if (!ConfigValues.Keys.Contains(key))
				{
					throw new Exception("Config key " + key + " not included in config!");
				}
			}
		}
	}
}
