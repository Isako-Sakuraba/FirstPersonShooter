using UnityEngine;

namespace Game.Experimental
{
    public class HitstopAudio : MonoBehaviour
    {
        private static HitstopAudio _instance;

        public static HitstopAudio Instance => _instance;

        [SerializeField] private AudioSource _audioSource;
        public AudioSource AudioSource => _audioSource;

        private void Awake()
        {
            _instance = this;
        }
    }
}