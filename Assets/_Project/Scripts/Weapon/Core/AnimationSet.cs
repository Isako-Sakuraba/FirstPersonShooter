using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    public abstract class AnimationSet<T> : ScriptableObject where T : Enum
    {
        [Serializable]
        public class Entry
        {
            public T Id;

            [Header("Clips")]
            public AnimationClip WeaponClip;
            public AnimationClip ArmsClip;

            [Header("Transitions")]
            [Min(0f)] public float FadeInDuration = 0.05f;
            [Min(0f)] public float FadeOutToIdleDuration = 0.08f;
        }

        [Serializable]
        public class IdleEntry
        {
            [Min(0f)] public float EnterFadeDuration = 0.08f;
            public AnimationClip WeaponClip;
            public AnimationClip ArmsClip;
        }


        [SerializeField] private IdleEntry _idle = new();
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public IdleEntry Idle => _idle;
        public IReadOnlyList<Entry> Entries => _entries;
    }

    public enum RevolverAnimationId
    {
        Equip,
        Dequip,
        Fire,
        AltFire,
        Cooldown
    }
}