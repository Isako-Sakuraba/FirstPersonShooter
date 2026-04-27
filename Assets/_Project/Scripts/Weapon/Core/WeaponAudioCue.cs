using UnityEngine;

namespace Game.Weapons
{
    [System.Serializable]
    public struct WeaponAudioCue
    {
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;

        public WeaponAudioCue(float volume)
        {
            Clip = null;
            Volume = Mathf.Clamp01(volume);
        }

        public float ResolveVolume()
        {
            return Mathf.Clamp01(Volume);
        }
    }
}
