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
		player.Prepare();
		while (!player.isPrepared)
		{
			yield return new WaitForSeconds(1);
		}
		GetComponent<RawImage>().texture = player.texture;
		player.Play();
		while (player.isPlaying)
		{
			if (!gameObject.activeInHierarchy)
			{
				player.Stop();
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForSeconds(1);
		}
		gameObject.Active(false);
	}
}
