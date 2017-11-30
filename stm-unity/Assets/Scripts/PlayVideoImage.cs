using System;
using System.Collections;

using PlayGen.Unity.Utilities.Localization;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoImage : MonoBehaviour
{
	[SerializeField]
	private Button _play;
	[SerializeField]
	private Button _pause;
	[SerializeField]
	private Slider _position;
	[SerializeField]
	private Text _timer;

	public void Play()
	{
		gameObject.Active(true);
		StartCoroutine(PlayVideo());
	}

	private IEnumerator PlayVideo()
	{
		_position.value = 0;
		_play.gameObject.Active(false);
		_pause.gameObject.Active(true);
		var player = GetComponent<VideoPlayer>();
		var image = GetComponent<RawImage>();
		image.enabled = false;
		player.Prepare();
		while (!player.isPrepared)
		{
			yield return new WaitForSeconds(0.2f);
		}
		image.texture = player.texture;
		player.Play();
		image.enabled = true;
		var previousValue = 0f;
		var length = TimeSpan.FromSeconds(player.clip.length);
		while (player.clip != null && player.clip.length > player.time)
		{
			if (Mathf.Approximately(_position.value, previousValue))
			{
				_position.value = (float)(player.time / player.clip.length);
			}
			else
			{
				player.time = _position.value * player.clip.length;
			}
			var time = TimeSpan.FromSeconds(player.time);
			_timer.text = (time.Hours > 0 ? time.Hours + ":" + time.Minutes.ToString("00") + ":" : time.Minutes + ":") + time.Seconds.ToString("00");
			_timer.text += " / " + (length.Hours > 0 ? length.Hours + ":" + length.Minutes.ToString("00") + ":" : length.Minutes + ":") + length.Seconds.ToString("00");
			previousValue = _position.value;
			if (!gameObject.activeInHierarchy)
			{
				player.clip = null;
				player.Stop();
			}
			yield return new WaitForSeconds(0.1f);
		}
		Stop();
	}

	public void Continue()
	{
		GetComponent<VideoPlayer>().Play();
		_play.gameObject.Active(false);
		_pause.gameObject.Active(true);
	}

	public void Pause()
	{
		GetComponent<VideoPlayer>().Pause();
		_play.gameObject.Active(true);
		_pause.gameObject.Active(false);
	}

	public void Stop()
	{
		GetComponent<RawImage>().texture = null;
		gameObject.Active(false);
	}
}
