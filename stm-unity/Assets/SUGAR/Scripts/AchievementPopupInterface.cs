using System.Collections;

using PlayGen.SUGAR.Client.EvaluationEvents;
using PlayGen.SUGAR.Unity;

using UnityEngine;

public class AchievementPopupInterface : BaseAchievementPopupInterface
{
	/// <summary>
	/// Animation for displaying the achievement notification.
	/// </summary>
	[Tooltip("Animation for displaying the achievement notification")]
	[SerializeField]
	private Animation _animation;

	/// <summary>
	/// If the animation is not playing, start the animation coroutine.
	/// </summary>
	protected override void Display(EvaluationNotification notification)
	{
		if (!_animation.isPlaying)
		{
			StartCoroutine(AnimatePopup());
		}
	}

	/// <summary>
	/// While there are notifications to display, cycle the animation.
	/// </summary>
	private IEnumerator AnimatePopup()
	{
		while (_achievementQueue.Count > 0)
		{
			_name.text = _achievementQueue[0].Name;
			_animation.Play();
			while (_animation.isPlaying)
			{
				yield return null;
			}
			_achievementQueue.RemoveAt(0);
			yield return null;
		}
	}
}
