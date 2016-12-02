using PlayGen.SUGAR.Contracts.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace SUGAR.Unity
{
	public class LeaderboardPositionInterface : MonoBehaviour
	{
		[SerializeField]
		private Text _position;

		[SerializeField]
		private Text _playerName;

		[SerializeField]
		private Text _score;

		internal void SetText(LeaderboardStandingsResponse res)
		{
			gameObject.SetActive(true);
			_position.text = res.Ranking.ToString();
			_playerName.text = res.ActorName;
			_score.text = res.Value;
		}

		internal void Disbale()
		{
			gameObject.SetActive(false);
		}
	}
}
