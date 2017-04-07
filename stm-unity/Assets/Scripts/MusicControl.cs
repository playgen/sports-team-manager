using UnityEngine;

public class MusicControl : MonoBehaviour {

	[SerializeField]
	private AudioClip[] _music;
	private AudioSource _audio;
	private int _currentTrack;

	void Start () {
		_audio = GetComponent<AudioSource>();
		_audio.clip = _music[_currentTrack];
		_audio.Play();
    }
	
	void Update () {
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
