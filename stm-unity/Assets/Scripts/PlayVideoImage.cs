using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoImage : MonoBehaviour
{
	public void Play()
	{
		gameObject.Active(true);
		StartCoroutine(PlayVideo());
	}

	private IEnumerator PlayVideo()
	{
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
		while (player.isPlaying)
		{
			if (!gameObject.activeInHierarchy)
			{
				player.Stop();
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForSeconds(0.2f);
		}
		image.texture = null;
		gameObject.Active(false);
	}
}
