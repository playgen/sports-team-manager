using System;
using System.IO;
using System.Net;
using System.Text;

using AssetPackage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
    internal class BaseBridge : IBridge, IDataStorage, IWebServiceRequest
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

		public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
		{
			var request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);

			var postData = requestSettings.body;
			var data = Encoding.ASCII.GetBytes(postData);

			request.Method = "POST";
			foreach (var header in requestSettings.requestHeaders)
			{
				request.Headers.Add(header.Key, header.Value);
			}
			request.ContentLength = data.Length;

			using (var stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			var response = (HttpWebResponse)request.GetResponse();

			requestResponse = new RequestResponse { responseCode = (int)response.StatusCode };
		}
	}
}
