using Game.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Experimental
{
    public class ExperimentalHitstopVisuals : MonoBehaviour
    {
        [SerializeField] private GameObject _overlay;
        [SerializeField] private AudioSource _parryAudio;

        private void Awake()
        {
            _overlay.SetActive(false);
            HitstopSystem.OnHitstopStart += HitstopStart;
            HitstopSystem.OnHitstopEnd += HitstopEnd;
        }

        private void HitstopStart()
        {
            if (_overlay != null)
                _overlay.SetActive(true);
            _parryAudio.Play();
            _parryAudio.time = 0.05f;
        }

        private void HitstopEnd()
        {
            if (_overlay != null)
            _overlay.SetActive(false);
        }
    }
}
