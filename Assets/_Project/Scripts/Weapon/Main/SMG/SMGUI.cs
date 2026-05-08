using TMPro;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Weapons.Main
{
    public class SMGUI : MonoBehaviour
    {
        private enum GrenadeIndicatorState
        {
            Charging,
            Ready,
            NotReady
        }

        [SerializeField] private SMG _smg;
        [SerializeField] private Image _grenadeReadyImage;
        [SerializeField] private Image _grenadeNotReadyImage;
        [SerializeField] private Image _grenadeChargeFill;
        [SerializeField] private TMP_Text _bulletText;
        [SerializeField] private string _bulletTextFormat = "{0}";
        [SerializeField] private float _indicatorBopScale = 1.1f;
        [SerializeField] private float _indicatorBopDuration = 0.08f;
        [SerializeField] private Ease _indicatorBopEase = Ease.OutQuad;

        private Tween _readyBopTween;
        private Tween _notReadyBopTween;
        private float _readyBaseScale = 1f;
        private float _notReadyBaseScale = 1f;
        private bool _hasIndicatorState;
        private GrenadeIndicatorState _indicatorState;

        private void Awake()
        {
            if (_grenadeReadyImage != null)
            {
                _readyBaseScale = _grenadeReadyImage.transform.localScale.x;
            }

            if (_grenadeNotReadyImage != null)
            {
                _notReadyBaseScale = _grenadeNotReadyImage.transform.localScale.x;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_smg == null)
            {
                return;
            }

            bool isCharging = _smg.IsGrenadeCharging;
            bool isReady = _smg.IsGrenadeReady;
            GrenadeIndicatorState state = ResolveIndicatorState(isCharging, isReady);

            if (_grenadeReadyImage != null)
            {
                _grenadeReadyImage.gameObject.SetActive(!isCharging && isReady);
            }

            if (_grenadeNotReadyImage != null)
            {
                _grenadeNotReadyImage.gameObject.SetActive(!isCharging && !isReady);
            }

            if (_grenadeChargeFill != null)
            {
                _grenadeChargeFill.gameObject.SetActive(isCharging);
                _grenadeChargeFill.fillAmount = _smg.CurrentGrenadeCharge01;
            }

            if (_bulletText != null)
            {
                string format = string.IsNullOrEmpty(_bulletTextFormat) ? "{0}" : _bulletTextFormat;
                _bulletText.text = string.Format(format, _smg.CurrentBulletAmount);
            }

            HandleIndicatorStateChanged(state);
        }

        private void OnDisable()
        {
            _readyBopTween.Stop();
            _notReadyBopTween.Stop();

            if (_grenadeReadyImage != null)
            {
                _grenadeReadyImage.transform.localScale = Vector3.one * _readyBaseScale;
            }

            if (_grenadeNotReadyImage != null)
            {
                _grenadeNotReadyImage.transform.localScale = Vector3.one * _notReadyBaseScale;
            }
        }

        private static GrenadeIndicatorState ResolveIndicatorState(bool isCharging, bool isReady)
        {
            if (isCharging)
            {
                return GrenadeIndicatorState.Charging;
            }

            return isReady ? GrenadeIndicatorState.Ready : GrenadeIndicatorState.NotReady;
        }

        private void HandleIndicatorStateChanged(GrenadeIndicatorState currentState)
        {
            if (!_hasIndicatorState)
            {
                _hasIndicatorState = true;
                _indicatorState = currentState;
                return;
            }

            if (_indicatorState == currentState)
            {
                return;
            }

            _indicatorState = currentState;

            if (currentState == GrenadeIndicatorState.Ready)
            {
                PlayIndicatorBop(_grenadeReadyImage, _readyBaseScale, ref _readyBopTween);
                return;
            }

            if (currentState == GrenadeIndicatorState.NotReady)
            {
                PlayIndicatorBop(_grenadeNotReadyImage, _notReadyBaseScale, ref _notReadyBopTween);
            }
        }

        private void PlayIndicatorBop(Image indicatorImage, float baseScale, ref Tween bopTween)
        {
            if (indicatorImage == null)
            {
                return;
            }

            float safeBaseScale = Mathf.Max(baseScale, 0f);
            float targetScale = safeBaseScale * Mathf.Max(1f, _indicatorBopScale);
            float duration = Mathf.Max(0.01f, _indicatorBopDuration);

            indicatorImage.transform.localScale = Vector3.one * safeBaseScale;
            bopTween.Stop();
            bopTween = Tween.Scale(
                indicatorImage.transform,
                targetScale,
                duration,
                ease: _indicatorBopEase,
                cycleMode: CycleMode.Yoyo,
                cycles: 2);
        }
    }
}
