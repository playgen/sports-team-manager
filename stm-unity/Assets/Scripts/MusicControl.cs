using UnityEngine;

/// <summary>
/// Music management
/// </summary>
public class MusicControl : MonoBehaviour {

	[SerializeField]
	private AudioClip[] _music;
	private AudioSource _audio;
	private int _currentTrack;

	void Start () {
		_audio = GetComponent<AudioSource>();
		if (_music.Length > 0)
		{
			_audio.clip = _music[_currentTrack];
			_audio.Play();
		}
	}

	/// <summary>
	/// If the current clip has finishing playing, change the clip to next in array and start playing again
	/// </summary>
	void Update () {
		if (_music.Length > 0)
		{
			if (UIStateManager.MusicOn == _audio.mute)
			{
				_audio.mute = !UIStateManager.MusicOn;
			}
			if (!_audio.isPlaying && _music[_currentTrack].loadState != AudioDataLoadState.Loading)
			{
				_audio.Stop();
				_currentTrack++;
				if (_currentTrack >= _music.Length)
				{
					_currentTrack = 0;
				}
				_audio.clip = _music[_currentTrack];
				_audio.Play();
			}
		}
	}
}