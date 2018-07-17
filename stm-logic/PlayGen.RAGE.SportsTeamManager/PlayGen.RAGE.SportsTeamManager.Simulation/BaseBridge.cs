using System;
using System.IO;
using System.Net;
using System.Text;

using AssetPackage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal class BaseBridge : IBridge, IDataStorage, IWebServiceRequest
	{
		public virtual bool Delete(string fileId)
		{
			throw new NotImplementedException();
		}

		public virtual bool Exists(string fileId)
		{
			return File.Exists(fileId);
		}

		public virtual string[] Files()
		{
			throw new NotImplementedException();
		}

		public virtual string Load(string fileId)
		{
			using (var reader = File.OpenText(fileId))
			{
				return reader.ReadToEnd();
			}
		}

		public virtual void Save(string fileId, string fileData)
		{
			using (var writer = File.CreateText(fileId))
			{
				writer.Write(fileData);
			}
		}

		public virtual void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
		{
			var result = new RequestResponse(requestSettings);

			try
			{
				var request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);
				request.Method = requestSettings.method;
				if (requestSettings.requestHeaders.ContainsKey("Accept"))
				{
					request.Accept = requestSettings.requestHeaders["Accept"];
				}
				if (!string.IsNullOrEmpty(requestSettings.body))
				{
					var data = Encoding.UTF8.GetBytes(requestSettings.body);
					if (requestSettings.requestHeaders.ContainsKey("Content-Type"))
					{
						request.ContentType = requestSettings.requestHeaders["Content-Type"];
					}
					foreach (var kvp in requestSettings.requestHeaders)
					{
						if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
						{
							continue;
						}
						request.Headers.Add(kvp.Key, kvp.Value);
					}
					request.ContentLength = data.Length;
					request.ServicePoint.Expect100Continue = false;
					var stream = request.GetRequestStream();
					stream.Write(data, 0, data.Length);
					stream.Close();
				}
				else
				{
					foreach (var kvp in requestSettings.requestHeaders)
					{
						if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
						{
							continue;
						}
						request.Headers.Add(kvp.Key, kvp.Value);
					}
				}

				var response = request.GetResponse();
				if (response.Headers.HasKeys())
				{
					foreach (var key in response.Headers.AllKeys)
					{
						result.responseHeaders.Add(key, response.Headers.Get(key));
					}
				}
				result.responseCode = (int)((HttpWebResponse)response).StatusCode;
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					result.body = reader.ReadToEnd();
				}
			}
			catch (Exception e)
			{
				result.responsMessage = e.Message;

				throw new Exception($"{e.GetType().Name} - {e.Message}");
			}

			requestResponse = result;
		}
	}
}
