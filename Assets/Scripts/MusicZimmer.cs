using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZimmer : MonoBehaviour
{
    public AudioSource music;
    public float yThreshold = -10f;

    private bool isPlaying = false;

    private void Update()
    {
        if (!isPlaying && transform.position.y < yThreshold)
        {
            StartMusic();
        }
    }

    private void StartMusic()
    {
        isPlaying = true;
        music.Play();
    }
}
