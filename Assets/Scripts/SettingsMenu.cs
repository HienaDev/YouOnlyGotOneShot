using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string volumeParameter = "Volume";

    [Header("Player")]
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Sensitivity Settings")]
    [SerializeField] private float minSensitivity = 0.01f;
    [SerializeField] private float maxSensitivity = 0.8f;

    // PlayerPrefs keys
    private const string VolumePrefKey = "GameVolume";
    private const string SensitivityPrefKey = "MouseSensitivity";

    private void Start()
    {
        // Clamp and set sensitivity slider range
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;

        // Load saved volume or get current from AudioMixer if none saved
        float savedVolume = PlayerPrefs.HasKey(VolumePrefKey)
            ? PlayerPrefs.GetFloat(VolumePrefKey)
            : GetCurrentVolumeFromMixer();

        volumeSlider.value = savedVolume;
        ApplyVolume(savedVolume);

        // Load saved sensitivity or get current from PlayerCamera if none saved
        float savedSensitivity = PlayerPrefs.HasKey(SensitivityPrefKey)
            ? PlayerPrefs.GetFloat(SensitivityPrefKey)
            : playerCamera != null ? Mathf.Clamp(playerCamera.GetSensitivity(), minSensitivity, maxSensitivity) : minSensitivity;

        sensitivitySlider.value = savedSensitivity;
        ApplySensitivity(savedSensitivity);

        // Add listeners
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
        sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);


        gameObject.SetActive(false);
    }

    private float GetCurrentVolumeFromMixer()
    {
        if (audioMixer.GetFloat(volumeParameter, out float currentVolumeDb))
        {
            return Mathf.Pow(10f, currentVolumeDb / 20f);
        }
        return 1f; // Default volume (max)
    }

    private void ApplyVolume(float value)
    {
        float volumeInDecibels = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
        audioMixer.SetFloat(volumeParameter, volumeInDecibels);

        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }

    private void ApplySensitivity(float value)
    {
        float clampedValue = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        if (playerCamera != null)
        {
            playerCamera.SetSensitivity(clampedValue);
        }

        PlayerPrefs.SetFloat(SensitivityPrefKey, clampedValue);
        PlayerPrefs.Save();
    }
}
