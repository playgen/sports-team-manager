using System.Globalization;

using PlayGen.SUGAR.Common.Shared;
using PlayGen.SUGAR.Contracts.Shared;
using UnityEngine;

namespace SUGAR.Unity
{
	public class GameDataUnityClient
	{
		public void Send(string key, string value)
		{
			Send(key, value, SaveDataType.String);
		}

		public void Send(string key, long value)
		{
			Send(key, value.ToString(), SaveDataType.Long);
		}

		public void Send(string key, float value)
		{
			Send(key, value.ToString(CultureInfo.InvariantCulture), SaveDataType.Float);
		}

		public void Send(string key, bool value)
		{
			Send(key, value.ToString(), SaveDataType.Boolean);
		}

		private void Send(string key, string value, SaveDataType dataType)
		{
			if (SUGARManager.CurrentUser != null)
			{
				SaveDataRequest data = new SaveDataRequest
				{
					ActorId = SUGARManager.CurrentUser.Id,
					GameId = SUGARManager.GameId,
					Key = key,
					Value = value,
					SaveDataType = dataType
				};

				SUGARManager.Client.GameData.AddAsync(data,
				response =>
				{
					Debug.Log("GameData Sending Success: True");
				},
				exception =>
				{
					Debug.LogError("GameData Sending Success: False. Exception: " + exception);
				});
			}
		}
	}
}
