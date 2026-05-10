using System;

namespace Game.Weapons.Animation
{
    internal interface IAnimationLayer
    {
        Type AnimationIdType { get; }

        void SetActive(bool active);
        bool Play(Enum id);
        bool PlayOneShot(Enum id);
        void ResetToIdle(float? fadeOverride = null);
        void Tick(float dt);
        void Destroy();
    }
}