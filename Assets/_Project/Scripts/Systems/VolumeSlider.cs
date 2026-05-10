using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Game.Systems
{
    public class VolumeSlider : MonoBehaviour
    {
        private const string SfxVolumePrefKey = "volume.sfx";
        private const string MusicVolumePrefKey = "volume.music";
        private const float MinSliderValue = 0.0001f;

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private string _sfxExposedParameter = "SFXVolume";
        [SerializeField] private string _musicExposedParameter = "MusicVolume";
        [SerializeField] private float _defaultSfxVolume = 1f;
        [SerializeField] private float _defaultMusicVolume = 1f;

        public static float SfxVolume01 { get; private set; } = 1f;
        public static float MusicVolume01 { get; private set; } = 1f;

        private void Awake()
        {
            InitializeSlider(_sfxVolumeSlider);
            InitializeSlider(_musicVolumeSlider);

            float sfxValue = PlayerPrefs.GetFloat(SfxVolumePrefKey, Mathf.Clamp01(_defaultSfxVolume));
            float musicValue = PlayerPrefs.GetFloat(MusicVolumePrefKey, Mathf.Clamp01(_defaultMusicVolume));

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.SetValueWithoutNotify(sfxValue);
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.SetValueWithoutNotify(musicValue);
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            ApplyVolume(_sfxExposedParameter, sfxValue);
            ApplyVolume(_musicExposedParameter, musicValue);
            SfxVolume01 = Mathf.Clamp01(sfxValue);
            MusicVolume01 = Mathf.Clamp01(musicValue);
        }

        private void OnDestroy()
        {
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            }
        }

        private void OnSfxVolumeChanged(float value)
        {
            float clamped = Mathf.Clamp01(value);
            ApplyVolume(_sfxExposedParameter, clamped);
            SfxVolume01 = clamped;
            PlayerPrefs.SetFloat(SfxVolumePrefKey, clamped);
            PlayerPrefs.Save();
        }

        private void OnMusicVolumeChanged(float value)
        {
            float clamped = Mathf.Clamp01(value);
            ApplyVolume(_musicExposedParameter, clamped);
            MusicVolume01 = clamped;
            PlayerPrefs.SetFloat(MusicVolumePrefKey, clamped);
            PlayerPrefs.Save();
        }

        private void ApplyVolume(string exposedParameter, float sliderValue)
        {
            if (_audioMixer == null || string.IsNullOrEmpty(exposedParameter))
            {
                return;
            }

            float clamped = Mathf.Clamp(sliderValue, MinSliderValue, 1f);
            float decibels = Mathf.Log10(clamped) * 20f;
            _audioMixer.SetFloat(exposedParameter, decibels);
        }

        private static void InitializeSlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 1f;
        }
    }
}
