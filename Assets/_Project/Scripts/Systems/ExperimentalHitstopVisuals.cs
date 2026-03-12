using Game.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Experimental
{
    public class ExperimentalHitstopVisuals : MonoBehaviour
    {
        [SerializeField] private GameObject _overlay;
        [SerializeField] private AudioSource _parryAudio;
        [SerializeField] private AudioSource _daamnAudio;

        private void Awake()
        {
            _overlay.SetActive(false);
            HitstopSystem.OnHitstopStart += HitstopStart;
            HitstopSystem.OnHitstopEnd += HitstopEnd;
        }

        private void HitstopStart()
        {
            _overlay.SetActive(true);
            _parryAudio.Play();
            _daamnAudio.Play();
            _parryAudio.time = 0.05f;
        }

        private void HitstopEnd()
        {
            _overlay.SetActive(false);
        }
    }
}