using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoImage : MonoBehaviour
{
    IEnumerator PlayVideo()
    {
        GetComponent<VideoPlayer>().Prepare();
        while (!GetComponent<VideoPlayer>().isPrepared)
        {
            yield return new WaitForSeconds(1);
        }
        GetComponent<RawImage>().texture = GetComponent<VideoPlayer>().texture;
        GetComponent<VideoPlayer>().Play();
    }
}
