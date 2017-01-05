using UnityEngine;
using UnityEngine.UI;

namespace PlayGen.SUGAR.Unity
{
	public class AchievementItemInterface : MonoBehaviour
	{
		[SerializeField]
		private Text _achieveName;
		[SerializeField]
		private Image _tick;

		internal void SetText(string achieveName, bool completed)
		{
			gameObject.SetActive(true);
			_achieveName.text = achieveName;
			_tick.enabled = completed;
		}

		internal void Disbale()
		{
			gameObject.SetActive(false);
		}
	}
}
