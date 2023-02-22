using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class HowToVideoController : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeVideoClip(VideoClip clip)
    {
        videoPlayer.clip = clip;
    }
}
