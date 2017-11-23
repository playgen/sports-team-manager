using System;
using System.Linq;

using AssetPackage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class TemplateBridge : IBridge, IDataStorage
	{
		public bool Delete(string fileId)
		{
			throw new NotImplementedException();
		}

		public bool Exists(string fileId)
		{
			return true;
		}

		public string[] Files()
		{
			throw new NotImplementedException();
		}

		public string Load(string fileId)
		{
			fileId = fileId.Replace(".", "_").ToLower();
			fileId = fileId.Split(ConfigStore.Platform == Platform.Windows ? '\\' : '/').Last();
			var obj = Templates.ResourceManager.GetString(fileId);
			return obj;
		}

		public void Save(string fileId, string fileData)
		{
			throw new NotImplementedException();
		}
	}
}
