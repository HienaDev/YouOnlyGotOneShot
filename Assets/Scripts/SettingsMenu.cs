using JigglePhysics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider fovSlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string volumeParameter = "Volume";

    [Header("Player")]
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Sensitivity Settings")]
    [SerializeField] private float minSensitivity = 0.01f;
    [SerializeField] private float maxSensitivity = 0.8f;

    [Header("FOV Settings")]
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 100f;

    [Header("Post Processing")]
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera armCamera;
    [SerializeField] private FullScreenPassRendererFeature CRTPass;

    [Header("Toggle Buttons (X-overlays)")]
    [SerializeField] private GameObject postProcessingToggleButton;
    [SerializeField] private GameObject cameraLeanToggleButton;
    [SerializeField] private GameObject jiggleToggleButton;

    private bool postProcessingEnabled = true;
    private bool cameraLeanEnabled = true;
    private bool jiggleEnabled = true;

    private List<JiggleRigBuilder> jiggleRigs = new();
    private PlayerCharacter playerCharacter;
    private Player player;

    // PlayerPrefs keys
    private const string VolumePrefKey = "GameVolume";
    private const string SensitivityPrefKey = "MouseSensitivity";
    private const string FOVPrefKey = "PlayerFOV";
    private const string PostProcessingPrefKey = "PostProcessingEnabled";
    private const string CameraLeanPrefKey = "CameraLeanEnabled";
    private const string JigglePrefKey = "JiggleEnabled";

    private void Start()
    {
        playerCharacter = FindAnyObjectByType<PlayerCharacter>();
        player = FindAnyObjectByType<Player>();

        // --- Sliders setup ---
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        fovSlider.minValue = minFOV;
        fovSlider.maxValue = maxFOV;

        // --- Load saved values ---
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        ApplyVolume(savedVolume);
        volumeSlider.value = savedVolume;

        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityPrefKey,
            playerCamera != null ? playerCamera.GetSensitivity() : 0.2f);
        ApplySensitivity(savedSensitivity);
        sensitivitySlider.value = Mathf.Clamp(savedSensitivity, minSensitivity, maxSensitivity);

        float savedFOV = PlayerPrefs.GetFloat(FOVPrefKey,
            playerCharacter != null ? playerCharacter.GetCurrentFOV() : 60f);
        ApplyFOV(savedFOV);
        fovSlider.value = Mathf.Clamp(savedFOV, minFOV, maxFOV);

        postProcessingEnabled = PlayerPrefs.GetInt(PostProcessingPrefKey, 1) == 1;
        cameraLeanEnabled = PlayerPrefs.GetInt(CameraLeanPrefKey, 1) == 1;
        jiggleEnabled = PlayerPrefs.GetInt(JigglePrefKey, 1) == 1;

        ApplyPostProcessing(postProcessingEnabled);
        ApplyCameraLean(cameraLeanEnabled);
        ApplyJiggle(jiggleEnabled);

        postProcessingToggleButton.SetActive(postProcessingEnabled);
        cameraLeanToggleButton.SetActive(cameraLeanEnabled);
        jiggleToggleButton.SetActive(jiggleEnabled);

        // Setup slider listeners
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
        sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);
        fovSlider.onValueChanged.AddListener(ApplyFOV);

        // Find jiggle rigs
        JiggleRigBuilder[] foundRigs = FindObjectsByType<JiggleRigBuilder>(FindObjectsSortMode.None);
        foreach (var rig in foundRigs)
        {
            if (rig != null)
            {
                rig.enabled = jiggleEnabled;
                jiggleRigs.Add(rig);
            }
        }

        gameObject.SetActive(false);
    }

    // --- Volume ---
    private void ApplyVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
        audioMixer.SetFloat(volumeParameter, db);

        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }

    // --- Sensitivity ---
    private void ApplySensitivity(float value)
    {
        float clamped = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        if (playerCamera != null)
            playerCamera.SetSensitivity(clamped);

        PlayerPrefs.SetFloat(SensitivityPrefKey, clamped);
        PlayerPrefs.Save();
    }

    // --- FOV ---
    private void ApplyFOV(float value)
    {
        float clamped = Mathf.Clamp(value, minFOV, maxFOV);
        if (playerCharacter != null)
            playerCharacter.SetDefaultFOV(clamped);

        PlayerPrefs.SetFloat(FOVPrefKey, clamped);
        PlayerPrefs.Save();
    }

    // --- Post Processing ---
    public void PostProcessingToggle()
    {
        postProcessingEnabled = !postProcessingEnabled;
        postProcessingToggleButton.SetActive(postProcessingEnabled);
        ApplyPostProcessing(postProcessingEnabled);

        PlayerPrefs.SetInt(PostProcessingPrefKey, postProcessingEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyPostProcessing(bool enabled)
    {
        var mainData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        var armData = armCamera.GetComponent<UniversalAdditionalCameraData>();
        if (mainData != null) mainData.renderPostProcessing = enabled;
        if (armData != null) armData.renderPostProcessing = enabled;

        CRTPass.SetActive(enabled);
    }

    // --- Camera Lean ---
    public void CameraLeanToggle()
    {
        cameraLeanEnabled = !cameraLeanEnabled;
        cameraLeanToggleButton.SetActive(cameraLeanEnabled);
        ApplyCameraLean(cameraLeanEnabled);

        PlayerPrefs.SetInt(CameraLeanPrefKey, cameraLeanEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyCameraLean(bool enabled)
    {
        if (playerCharacter != null)
            playerCharacter.ToggleAdaptiveFov(enabled);
        if (player != null)
            player.ToggleCameraLean(enabled);
    }

    // --- Jiggle Physics ---
    public void JiggleToggle()
    {
        jiggleEnabled = !jiggleEnabled;
        jiggleToggleButton.SetActive(jiggleEnabled);
        ApplyJiggle(jiggleEnabled);

        PlayerPrefs.SetInt(JigglePrefKey, jiggleEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyJiggle(bool enabled)
    {
        foreach (var rig in jiggleRigs)
        {
            if (rig != null)
                rig.enabled = enabled;
        }
    }
}
