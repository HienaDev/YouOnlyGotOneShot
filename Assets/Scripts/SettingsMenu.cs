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

    private void Start()
    {
        // Clamp and set sensitivity slider range
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;

        // Get current volume from AudioMixer
        if (audioMixer.GetFloat(volumeParameter, out float currentVolumeDb))
        {
            float linearVolume = Mathf.Pow(10f, currentVolumeDb / 20f);
            volumeSlider.value = linearVolume;
        }

        // Get current sensitivity from PlayerCamera
        if (playerCamera != null)
        {
            float clampedSens = Mathf.Clamp(playerCamera.GetSensitivity(), minSensitivity, maxSensitivity);
            sensitivitySlider.value = clampedSens;
        }

        // Add listeners
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
        sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);
    }

    private void ApplyVolume(float value)
    {
        float volumeInDecibels = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
        audioMixer.SetFloat(volumeParameter, volumeInDecibels);
    }

    private void ApplySensitivity(float value)
    {
        float clampedValue = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        if (playerCamera != null)
        {
            playerCamera.SetSensitivity(clampedValue);
        }
    }
}
