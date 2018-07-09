using System;
using System.IO;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal class IOSBaseBridge : BaseBridge
	{
		public new bool Delete(string fileId)
		{
			throw new NotImplementedException();
		}

		public new bool Exists(string fileId)
		{
			fileId = fileId.Replace('\\', '/');
			return File.Exists(fileId);
		}

		public new string[] Files()
		{
			throw new NotImplementedException();
		}

		public new string Load(string fileId)
		{
			fileId = fileId.Replace('\\', '/');
			using (var reader = File.OpenText(fileId))
			{
				return reader.ReadToEnd();
			}
		}

		public new void Save(string fileId, string fileData)
		{
			using (var writer = File.CreateText(fileId))
			{
				writer.Write(fileData);
			}
		}
	}
}
