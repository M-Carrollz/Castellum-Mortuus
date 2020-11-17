using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public void PlaySound(AudioSource audio)
    {
        audio.Play();
    }

    public void PlaySoundOneShot(AudioSource audio)
    {
        audio.PlayOneShot(audio.clip);
    }

    public void StopSound(AudioSource audio)
    {
        audio.Stop();
    }
}
