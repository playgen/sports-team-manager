using System;
using System.Linq;
using UnityEngine;

public class ReactionSoundControl : MonoBehaviour {
	[Serializable]
	class Reaction
	{
		public string Name;
		public AudioClip[] Clips;
	}

	private AudioSource _audio;
	[SerializeField]
	private Reaction[] _reactions;

	void Start () {
		_audio = GetComponent<AudioSource>();
	}
	
	void Update () {
		if (UIStateManager.SoundOn == _audio.mute)
		{
			_audio.mute = !UIStateManager.SoundOn;
		}
	}

	public void PlaySound(string reaction, bool male, float height, float weight)
	{
		if (!_audio.mute)
		{
			var react = _reactions.FirstOrDefault(r => r.Name == reaction);
			if (react != null && react.Clips.Any())
			{
				_audio.Stop();
				_audio.clip = react.Clips[UnityEngine.Random.Range(0, react.Clips.Length)];
				_audio.pitch = (male ? 1 : 1.1f) - (0.05f * height * weight);
				_audio.Play();
			}
		}
	}
}