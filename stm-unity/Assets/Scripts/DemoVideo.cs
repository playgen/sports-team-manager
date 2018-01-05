using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DemoVideo : MonoBehaviour
{
	[SerializeField]
	private float _demoStartTime = 120;
	private float _lastAction;
	private Vector2 _lastMousePosition;
	private bool _playing;

	private void Update()
	{
		if (GameManagement.PlatformSettings.DemoMode)
		{
			if (Input.anyKeyDown || Vector2.Distance(Input.mousePosition, _lastMousePosition) > 25)
			{
				_lastAction = 0;
			}
			else
			{
				_lastAction += Time.smoothDeltaTime;
			}
			_lastMousePosition = Input.mousePosition;
			if (!_playing && _lastAction > _demoStartTime)
			{
				_playing = true;
				StartCoroutine(PlayVideo());
			}
		}
	}

	private IEnumerator FadeIn()
	{
		var canvasGroup = GetComponentInParent<CanvasGroup>();
		while (_playing && canvasGroup.alpha < 1)
		{
			canvasGroup.alpha += Time.smoothDeltaTime / 3;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator FadeOut()
	{
		_playing = false;
		var canvasGroup = GetComponentInParent<CanvasGroup>();
		while (!_playing && canvasGroup.alpha > 0)
		{
			canvasGroup.alpha -= Time.smoothDeltaTime / 3;
			yield return new WaitForEndOfFrame();
		}
		var player = GetComponent<VideoPlayer>();
		player.Stop();
		GetComponent<RawImage>().texture = null;
	}

	private IEnumerator PlayVideo()
	{
		var player = GetComponent<VideoPlayer>();
		var image = GetComponent<RawImage>();
		player.Prepare();
		while (!player.isPrepared)
		{
			yield return new WaitForSeconds(0.2f);
		}
		image.texture = player.texture;
		player.Play();
		StartCoroutine(FadeIn());
		while (_lastAction > _demoStartTime)
		{
			yield return new WaitForSeconds(0.1f);
		}
		StartCoroutine(FadeOut());
	}
}