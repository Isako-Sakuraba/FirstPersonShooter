using UnityEngine;
using UnityEngine.UI;

namespace Game.Rigs
{
    public class SensitivitySlider : MonoBehaviour
    {
        private const float DefaultSensitivity = 1f;

        [SerializeField] private Slider _slider;
        [SerializeField] private float _defaultValue = DefaultSensitivity;

        public static float Value { get; private set; } = DefaultSensitivity;

        private void Awake()
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }

            float clamped = Mathf.Max(0f, _defaultValue);
            Value = clamped;

            if (_slider != null)
            {
                _slider.SetValueWithoutNotify(clamped);
                _slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        private static void OnSliderValueChanged(float value)
        {
            Value = Mathf.Max(0f, value);
        }
    }
}
