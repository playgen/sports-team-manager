using System;
using System.IO;
using AssetPackage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
    class BaseBridge : IBridge, IDataStorage
    {
        public bool Delete(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileId)
        {
            return File.Exists(fileId);
        }

        public string[] Files()
        {
            throw new NotImplementedException();
        }

        public string Load(string fileId)
        {
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
