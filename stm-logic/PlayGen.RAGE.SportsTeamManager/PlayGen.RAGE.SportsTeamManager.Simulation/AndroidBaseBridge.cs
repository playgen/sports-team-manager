using System;
using System.IO;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	class AndroidBaseBridge : BaseBridge
	{
		public bool Delete(string fileId)
		{
			throw new NotImplementedException();
		}

		public bool Exists(string fileId)
		{
			fileId = fileId.Replace('\\', '/');
			return File.Exists(fileId);
		}

		public string[] Files()
		{
			throw new NotImplementedException();
		}

		public string Load(string fileId)
		{
			fileId = fileId.Replace('\\', '/');
			using (var reader = File.OpenText(fileId))
			{
				return reader.ReadToEnd();
			}
		}

		public void Save(string fileId, string fileData)
		{
			using (var writer = File.CreateText(fileId))
			{
				writer.Write(fileData);
			}
		}
	}
}
