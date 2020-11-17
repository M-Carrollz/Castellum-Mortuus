using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public AudioMixer mixer;

    public string groupName = "MasterVol";

    public void SetVolume(float sliderValue)
    {
        mixer.SetFloat(groupName, Mathf.Log10(sliderValue) * 20);
    }
}
