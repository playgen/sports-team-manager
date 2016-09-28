using System.IO;
using GAIPS.Rage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class TemplateStorageProvider : BaseStorageProvider
	{
		public TemplateStorageProvider() : base("") { }

		protected override Stream LoadFile(string absoluteFilePath, FileMode mode, FileAccess access)
		{
			absoluteFilePath = absoluteFilePath.Replace("/", "");
			absoluteFilePath = absoluteFilePath.Replace("\\", "");
			absoluteFilePath = absoluteFilePath.Replace(".", "_").ToLower();
			object obj = Templates.ResourceManager.GetObject(absoluteFilePath);
			return new MemoryStream((byte[])obj);
		}

		protected override bool IsDirectory(string path)
		{
			return false;
		}


	}
}
