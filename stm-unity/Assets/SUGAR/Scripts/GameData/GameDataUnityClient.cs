using PlayGen.SUGAR.Common.Shared;
using PlayGen.SUGAR.Contracts.Shared;
using UnityEngine;

namespace SUGAR.Unity
{
	public class GameDataUnityClient
	{
		public void Send(string key, string value)
		{
			bool success = false;
			if (SUGARManager.CurrentUser != null)
			{
				SaveDataRequest data = new SaveDataRequest
				{
					ActorId = SUGARManager.CurrentUser.Id,
					GameId = SUGARManager.GameId,
					Key = key,
					Value = value,
					SaveDataType = SaveDataType.String
				};
				success = SUGARManager.Client.GameData.Add(data) != null;
			}
			Debug.Log("GameData Sending Success: " + success);
		}

		public void Send(string key, long value)
		{
			bool success = false;
			if (SUGARManager.CurrentUser != null)
			{
				SaveDataRequest data = new SaveDataRequest
				{
					ActorId = SUGARManager.CurrentUser.Id,
					GameId = SUGARManager.GameId,
					Key = key,
					Value = value.ToString(),
					SaveDataType = SaveDataType.Long
				};
				success = SUGARManager.Client.GameData.Add(data) != null;
			}
			Debug.Log("GameData Sending Success: " + success);
		}

		public void Send(string key, float value)
		{
			bool success = false;
			if (SUGARManager.CurrentUser != null)
			{
				SaveDataRequest data = new SaveDataRequest
				{
					ActorId = SUGARManager.CurrentUser.Id,
					GameId = SUGARManager.GameId,
					Key = key,
					Value = value.ToString(),
					SaveDataType = SaveDataType.Float
				};
				success = SUGARManager.Client.GameData.Add(data) != null;
			}
			Debug.Log("GameData Sending Success: " + success);
		}

		public void Send(string key, bool value)
		{
			bool success = false;
			if (SUGARManager.CurrentUser != null)
			{
				SaveDataRequest data = new SaveDataRequest
				{
					ActorId = SUGARManager.CurrentUser.Id,
					GameId = SUGARManager.GameId,
					Key = key,
					Value = value.ToString(),
					SaveDataType = SaveDataType.Boolean
				};
				success = SUGARManager.Client.GameData.Add(data) != null;
			}
			Debug.Log("GameData Sending Success: " + success);
		}
	}
}
