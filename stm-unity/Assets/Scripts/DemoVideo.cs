using PlayGen.Unity.Utilities.Video;
using UnityEngine;

public class DemoVideo : VideoPlayerUI
{
	[SerializeField]
	private float _demoStartTime = 120;
	private float _lastAction;
	private Vector2 _lastMousePosition;
	private bool _playing;
	[SerializeField]
	private GameObject _checkObj;
	private CanvasGroup _canvasGroup;

	protected override void OnEnable()
	{
		_canvasGroup = GetComponentInParent<CanvasGroup>();
		base.OnEnable();
	}

	private void Update()
	{
		if (GameManagement.DemoMode)
		{
			if (Input.anyKeyDown || Vector2.Distance(Input.mousePosition, _lastMousePosition) > 25 || !_checkObj.activeInHierarchy)
			{
				_lastAction = 0;
				if (_playing)
				{
					_playing = false;
				}
			}
			else
			{
				_lastAction += Time.smoothDeltaTime;
			}
			_lastMousePosition = Input.mousePosition;
			if (!_playing && _lastAction > _demoStartTime)
			{
				_playing = true;
				PlayCurrent();
			}
			if (_playing && _player.isPlaying && _canvasGroup.alpha < 1)
			{
				_canvasGroup.alpha += Time.smoothDeltaTime / 3;
			}
			else if (!_playing && _player.isPlaying && _canvasGroup.alpha > 0)
			{
				_canvasGroup.alpha -= Time.smoothDeltaTime / 3;
				if (_canvasGroup.alpha <= 0)
				{
					Stop();
				}
			}
		}
	}

	public override void Stop()
	{
		_image.texture = null;
		_player.Stop();
		_player.time = 0;
	}
}