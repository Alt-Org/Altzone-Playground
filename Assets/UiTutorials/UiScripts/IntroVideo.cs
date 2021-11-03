using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Playables;
using UnityEngine.Audio;
using UnityEngine.UI;

public class IntroVideo : MonoBehaviour
{
    //public GameObject loadingImage;

    public GameObject IntroStart; 
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += EndReached;
    }
            /*
    IEnumerator playVideo()
    {

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            loadingImage.SetActive(true);
            Debug.Log("Preparing Video");
            yield return null;
        }

        Debug.Log("Done Preparing Video");
        loadingImage.SetActive(false);

        videoPlayer.Play();
    }
            */

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        Destroy(gameObject);
        IntroStart.SetActive(true);
    }
}