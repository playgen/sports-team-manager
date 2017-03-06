using PlayGen.SUGAR.Contracts.Shared;

using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPositionInterface : MonoBehaviour
{
	/// <summary>
	/// Text used to display the numbered position this user is on this leaderboard.
	/// </summary>
	[Tooltip("Text used to display the numbered position this user is on this leaderboard")]
	[SerializeField]
	private Text _position;

	/// <summary>
	/// Text used to display the name of the user in this position.
	/// </summary>
	[Tooltip("Text used to display the name of the user in this position")]
	[SerializeField]
	private Text _playerName;

	/// <summary>
	/// Text used to show their score for this leaderboard.
	/// </summary>
	[Tooltip("Text used to show their score for this leaderboard")]
	[SerializeField]
	private Text _score;

	/// <summary>
	/// Set the text to match values passed from the LeaderboardStandingResponse.
	/// </summary>
	internal void SetText(LeaderboardStandingsResponse res)
	{
		gameObject.SetActive(true);
		_position.text = res.Ranking.ToString();
		_playerName.text = res.ActorName;
		_score.text = res.Value;
	}

	/// <summary>
	/// Disable the GameObject if it isn't being used.
	/// </summary>
	internal void Disable()
	{
		gameObject.SetActive(false);
	}
}