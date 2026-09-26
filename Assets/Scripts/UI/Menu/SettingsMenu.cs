using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;
    [Tooltip("Exposed parameter names on the mixer (right-click a volume -> Expose to script).")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Video")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle vfxToggle;

    [Tooltip("Hook post-processing / particles on/off here.")]
    [SerializeField] private UnityEvent<bool> onVfxChanged;

    private const string MasterKey = "vol_master";
    private const string SfxKey = "vol_sfx";
    private const string MusicKey = "vol_music";
    private const string QualityKey = "quality";
    private const string ResKey = "resolution";
    private const string VfxKey = "vfx";

    private readonly List<Resolution> resolutions = new();

    private void Awake()
    {
        BuildQualityOptions();
        BuildResolutionOptions();
        LoadValues();

        // Wire up after loading so restoring the values doesn't fire callbacks.
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(SetQuality);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (vfxToggle != null) vfxToggle.onValueChanged.AddListener(SetVfx);
    }

    // AudioMixer.SetFloat is swallowed in Awake (the mixer applies its default snapshot
    // during scene load and overrides it). Push the volumes one frame later so they stick
    // even when the slider is already at its saved position (no drag = no callback).
    private IEnumerator Start()
    {
        yield return null;

        if (masterSlider != null) PushToMixer(masterParam, masterSlider.value);
        if (sfxSlider != null) PushToMixer(sfxParam, sfxSlider.value);
        if (musicSlider != null) PushToMixer(musicParam, musicSlider.value);
    }

    public void SetMasterVolume(float value) => ApplyVolume(masterParam, MasterKey, value);
    public void SetSfxVolume(float value) => ApplyVolume(sfxParam, SfxKey, value);
    public void SetMusicVolume(float value) => ApplyVolume(musicParam, MusicKey, value);

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(QualityKey, index);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count) return;

        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt(ResKey, index);
    }

    public void SetVfx(bool on)
    {
        onVfxChanged?.Invoke(on);
        PlayerPrefs.SetInt(VfxKey, on ? 1 : 0);
    }

    private void ApplyVolume(string param, string key, float value)
    {
        PushToMixer(param, value);
        PlayerPrefs.SetFloat(key, value);
    }

    private void PushToMixer(string param, float value)
    {
        if (mixer != null)
            mixer.SetFloat(param, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }

    private void BuildQualityOptions()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    private void BuildResolutionOptions()
    {
        if (resolutionDropdown == null) return;

        resolutions.Clear();
        List<string> labels = new();
        HashSet<string> seen = new();

        foreach (Resolution r in Screen.resolutions)
        {
            string label = r.width + " x " + r.height;
            if (!seen.Add(label)) continue;

            resolutions.Add(r);
            labels.Add(label);
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
    }

    private void LoadValues()
    {
        LoadSlider(masterSlider, MasterKey);
        LoadSlider(sfxSlider, SfxKey);
        LoadSlider(musicSlider, MusicKey);

        if (qualityDropdown != null)
        {
            int q = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
            qualityDropdown.SetValueWithoutNotify(q);
            QualitySettings.SetQualityLevel(q, true);
        }

        if (resolutionDropdown != null && resolutions.Count > 0)
        {
            int def = CurrentResolutionIndex();
            int res = PlayerPrefs.GetInt(ResKey, def);
            resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(res, 0, resolutions.Count - 1));
        }

        if (vfxToggle != null)
        {
            bool on = PlayerPrefs.GetInt(VfxKey, 1) == 1;
            vfxToggle.SetIsOnWithoutNotify(on);
            onVfxChanged?.Invoke(on);
        }
    }

    private void LoadSlider(Slider slider, string key)
    {
        if (slider == null) return;

        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, 0.8f));
    }

    private int CurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Count; ++i)
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                return i;

        return resolutions.Count - 1;
    }
}
