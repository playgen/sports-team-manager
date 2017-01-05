using System.Collections;
using System.Collections.Generic;

using PlayGen.SUGAR.Client.EvaluationEvents;
using UnityEngine;
using UnityEngine.UI;


namespace PlayGen.SUGAR.Unity
{
	public class AchievementPopupInterface : MonoBehaviour
	{
		[SerializeField]
		private Text _name;

		[SerializeField]
		private Animation _animation;

		private readonly List<EvaluationNotification> _achievementQueue = new List<EvaluationNotification>();

		internal void Animate(EvaluationNotification notification)
		{
			_achievementQueue.Add(notification);
			if (!_animation.isPlaying)
			{
				StartCoroutine(AnimatePopup());
			}
		}

		private IEnumerator AnimatePopup()
		{
			while (_achievementQueue.Count > 0)
			{
				_name.text = _achievementQueue[0].Name;
				transform.SetAsLastSibling();
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
}
