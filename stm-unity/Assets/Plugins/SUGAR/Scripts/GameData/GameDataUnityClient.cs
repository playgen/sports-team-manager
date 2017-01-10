using System.Globalization;
using System.Collections.Generic;
using PlayGen.SUGAR.Common.Shared;
using PlayGen.SUGAR.Contracts.Shared;
using UnityEngine;

namespace PlayGen.SUGAR.Unity
{
	public class GameDataUnityClient
	{
        public IEnumerable<EvaluationDataResponse> GetHighest(string[] keys, EvaluationDataType dataType)
        {
            var response = SUGARManager.Client.GameData.GetHighest(SUGARManager.CurrentUser.Id, SUGARManager.GameId, keys, dataType);
            return response;
        }

        public void Send(string key, string value)
		{
			Send(key, value, EvaluationDataType.String);
		}

		public void Send(string key, long value)
		{
			Send(key, value.ToString(), EvaluationDataType.Long);
		}

		public void Send(string key, float value)
		{
			Send(key, value.ToString(CultureInfo.InvariantCulture), EvaluationDataType.Float);
		}

		public void Send(string key, bool value)
		{
			Send(key, value.ToString(), EvaluationDataType.Boolean);
		}

		private void Send(string key, string value, EvaluationDataType dataType)
		{
			if (SUGARManager.CurrentUser != null)
			{
				EvaluationDataRequest data = new EvaluationDataRequest
				{
					CreatingActorId = SUGARManager.CurrentUser.Id,
					GameId = SUGARManager.GameId,
					Key = key,
					Value = value,
					EvaluationDataType = dataType
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
