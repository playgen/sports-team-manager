using UnityEngine;
using UnityEngine.UI;

public class AchievementItemInterface : MonoBehaviour
{
	/// <summary>
	/// Text for displaying achievement name.
	/// </summary>
	[Tooltip("Text for displaying achievement name")]
	[SerializeField]
	private Text _achieveName;

	/// <summary>
	/// Text for showing achievement image. In this case, a tick showing if the achievement has been completed or not.
	/// </summary>
	[Tooltip("Text for showing achievement image. In this case, a tick showing if the achievement has been completed or not")]
	[SerializeField]
	private Image _achieveImage;

	/// <summary>
	/// Enable the GameObject, set the text and enable/disable the image.
	/// </summary>
	internal void SetText(string achieveName, bool completed)
	{
		gameObject.SetActive(true);
		_achieveName.text = achieveName;
		_achieveImage.enabled = completed;
	}

	/// <summary>
	/// Disable the GameObject if it isn't being used.
	/// </summary>
	internal void Disable()
	{
		gameObject.SetActive(false);
	}
}