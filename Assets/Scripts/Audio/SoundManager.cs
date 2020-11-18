using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("Volume Settings Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider soundEffectSlider;
    public Slider characterSlider;
    public Slider ambienceSlider;

    // Start is called before the first frame update
    void Start()
    {
        // Set sliders to static soundSettings values
        // Turns out this must be called during start and not during awake.
        masterSlider.value = SoundSettings.master;
        masterSlider.onValueChanged.AddListener(delegate { SetLevel(masterSlider, ref SoundSettings.master); });

        musicSlider.value = SoundSettings.music;
        musicSlider.onValueChanged.AddListener(delegate { SetLevel(musicSlider, ref SoundSettings.music); });

        soundEffectSlider.value = SoundSettings.soundEffect;
        soundEffectSlider.onValueChanged.AddListener(delegate { SetLevel(soundEffectSlider, ref SoundSettings.soundEffect); });

        characterSlider.value = SoundSettings.character;
        characterSlider.onValueChanged.AddListener(delegate { SetLevel(characterSlider, ref SoundSettings.character); });

        ambienceSlider.value = SoundSettings.ambience;
        ambienceSlider.onValueChanged.AddListener(delegate { SetLevel(ambienceSlider, ref SoundSettings.ambience); });
    }

    void SetLevel(Slider slider, ref float settingsValue)
    {
        settingsValue = slider.value;
    }
}
