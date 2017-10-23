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
	private static ReactionSoundControl _instance;

	void Awake ()
	{
		_instance = this;
	}

	void Start () {
		_audio = GetComponent<AudioSource>();
	}
	
	void Update () {
		if (UIStateManager.SoundOn == _audio.mute)
		{
			_audio.mute = !UIStateManager.SoundOn;
		}
	}

	public static void PlaySound(string reaction, bool male, float height, float weight)
	{
		if (!_instance._audio.mute)
		{
			var react = _instance._reactions.FirstOrDefault(r => r.Name == reaction);
			if (react != null && react.Clips.Any())
			{
				_instance._audio.Stop();
				_instance._audio.clip = react.Clips[UnityEngine.Random.Range(0, react.Clips.Length)];
				_instance._audio.pitch = (male ? 1 : 1.1f) - (0.05f * height * weight);
				_instance._audio.Play();
			}
		}
	}
}