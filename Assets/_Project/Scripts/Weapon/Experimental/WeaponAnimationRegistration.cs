using System;

namespace Game.Weapons.Animation
{
    internal sealed class WeaponAnimationRegistration
    {
        public readonly int ArmsSlotIndex;
        public readonly Type AnimationIdType;
        public readonly IAnimationLayer ArmsLayer;
        public readonly IAnimationLayer WeaponPlayer;

        public WeaponAnimationRegistration(
            int armsSlotIndex,
            Type animationIdType,
            IAnimationLayer armsLayer,
            IAnimationLayer weaponPlayer)
        {
            ArmsSlotIndex = armsSlotIndex;
            AnimationIdType = animationIdType;
            ArmsLayer = armsLayer;
            WeaponPlayer = weaponPlayer;
        }

        public void SetActive(bool active)
        {
            ArmsLayer.SetActive(active);
            WeaponPlayer.SetActive(active);
        }

        public bool Play(Enum id)
        {
            bool armsPlayed = ArmsLayer.Play(id);
            bool weaponPlayed = WeaponPlayer.Play(id);
            return armsPlayed || weaponPlayed;
        }

        public bool PlayOneShot(Enum id)
        {
            bool armsPlayed = ArmsLayer.PlayOneShot(id);
            bool weaponPlayed = WeaponPlayer.PlayOneShot(id);
            return armsPlayed || weaponPlayed;
        }

        public void ResetToIdle(float? fadeOverride = null)
        {
            ArmsLayer.ResetToIdle(fadeOverride);
            WeaponPlayer.ResetToIdle(fadeOverride);
        }

        public void Tick(float deltaTime)
        {
            ArmsLayer.Tick(deltaTime);
            WeaponPlayer.Tick(deltaTime);
        }

        public void Destroy()
        {
            ArmsLayer.Destroy();
            WeaponPlayer.Destroy();
        }
    }
}