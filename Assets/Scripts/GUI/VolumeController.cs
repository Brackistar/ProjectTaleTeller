using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider slider;
    public UnityEngine.Audio.AudioMixer mixer;
    public string parameterName;
    public Text TextValue;

    void Awake()
    {
        float savedVol = PlayerPrefs.GetFloat(parameterName, slider.maxValue);
        SetVolume(savedVol); //Manually set value & volume before subscribing to ensure it is set even if slider.value happens to start at the same value as is saved
        slider.value = savedVol;
        UpdateValue(savedVol);

        slider.onValueChanged.AddListener((float _) => SetVolume(_)); //UI classes use unity events, requiring delegates (delegate(float _) { SetVolume(_); }) or lambda expressions ((float _) => SetVolume(_))
        slider.onValueChanged.AddListener((float _) => UpdateValue(_));
    }

    void SetVolume(float _value)
    {
        mixer.SetFloat(parameterName, ConvertToDecibel(_value / slider.maxValue)); //Dividing by max allows arbitrary positive slider maxValue
        PlayerPrefs.SetFloat(parameterName, _value);
    }

    /// <summary>
    /// Converts a percentage fraction to decibels,
    /// with a lower clamp of 0.0001 for a minimum of -80dB, same as Unity's Mixers.
    /// </summary>
    public float ConvertToDecibel(float _value)
    {
        return Mathf.Log10(Mathf.Max(_value, 0.0001f)) * 20f;
    }

    void UpdateValue(float _value)
    {
        TextValue.text = "" +
            ConvertToPercent(_value) +
            "/" +
            ConvertToPercent(slider.maxValue);
    }

    int ConvertToPercent(float _value)
    {
        return Mathf.FloorToInt(
            f: _value * 100);
    }
}
