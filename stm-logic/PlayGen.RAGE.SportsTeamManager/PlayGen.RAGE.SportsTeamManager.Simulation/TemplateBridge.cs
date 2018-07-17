using System;
using System.Linq;

using AssetPackage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class TemplateBridge : IBridge, IDataStorage
	{
		public virtual bool Delete(string fileId)
		{
			throw new NotImplementedException();
		}

		public virtual bool Exists(string fileId)
		{
			return true;
		}

		public virtual string[] Files()
		{
			throw new NotImplementedException();
		}

		public virtual string Load(string fileId)
		{
			fileId = fileId.Replace(".", "_").ToLower();
			fileId = fileId.Split(ConfigStore.Platform == Platform.Windows ? '\\' : '/').Last();
			var obj = Templates.ResourceManager.GetString(fileId);
			return obj;
		}

		public virtual void Save(string fileId, string fileData)
		{
			throw new NotImplementedException();
		}
	}
}
