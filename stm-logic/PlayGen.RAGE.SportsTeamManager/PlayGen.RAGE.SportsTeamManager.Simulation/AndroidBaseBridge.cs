using System;
using System.IO;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal class AndroidBaseBridge : BaseBridge
	{
		public override bool Delete(string fileId)
		{
			throw new NotImplementedException();
		}

		public override bool Exists(string fileId)
		{
			fileId = fileId.Replace('\\', '/');
			return File.Exists(fileId);
		}

		public override string[] Files()
		{
			throw new NotImplementedException();
		}

		public override string Load(string fileId)
		{
			fileId = fileId.Replace('\\', '/');
			using (var reader = File.OpenText(fileId))
			{
				return reader.ReadToEnd();
			}
		}

		public override void Save(string fileId, string fileData)
		{
			using (var writer = File.CreateText(fileId))
			{
				writer.Write(fileData);
			}
		}
	}
}
