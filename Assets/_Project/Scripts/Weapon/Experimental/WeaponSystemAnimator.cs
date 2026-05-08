using Game.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game.Weapons.Animation
{
    public sealed class WeaponSystemAnimator : MonoBehaviour
    {
        [Header("Shared Arms")]
        [SerializeField] private Animator _armsAnimator;
        [SerializeField, Min(1)] private int _maxLoadedWeapons = 16;

        private PlayableGraph _armsGraph;
        private AnimationPlayableOutput _armsOutput;
        private AnimationMixerPlayable _armsRootMixer;

        private readonly Dictionary<IWeapon, WeaponAnimationRegistration> _registrations = new();
        private readonly Stack<int> _freeArmsSlots = new();

        private IWeapon _currentWeapon;

        private void Awake()
        {
            BuildArmsGraph();
            InitializeFreeArmsSlots();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            foreach (WeaponAnimationRegistration registration in _registrations.Values)
                registration.Tick(deltaTime);
        }

        private void OnDestroy()
        {
            foreach (WeaponAnimationRegistration registration in _registrations.Values)
                registration.Destroy();

            _registrations.Clear();

            if (_armsGraph.IsValid())
                _armsGraph.Destroy();
        }

        public void Load<T>(
            IWeapon weapon,
            Animator weaponAnimator,
            AnimationSet<T> animationSet)
            where T : Enum
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));

            if (weaponAnimator == null)
                throw new ArgumentNullException(nameof(weaponAnimator));

            if (animationSet == null)
                throw new ArgumentNullException(nameof(animationSet));

            if (_registrations.ContainsKey(weapon))
                throw new InvalidOperationException($"Weapon '{weapon}' is already loaded.");

            if (_freeArmsSlots.Count == 0)
                throw new InvalidOperationException(
                    $"Cannot load more than {_maxLoadedWeapons} weapons into the arms graph.");

            int armsSlotIndex = _freeArmsSlots.Pop();

            IAnimationLayer armsLayer = new ArmsAnimationLayer<T>(
                _armsGraph,
                _armsRootMixer,
                armsSlotIndex,
                animationSet);

            IAnimationLayer weaponPlayer = new WeaponAnimationPlayer<T>(
                weaponAnimator,
                animationSet);

            var registration = new WeaponAnimationRegistration(
                armsSlotIndex,
                typeof(T),
                armsLayer,
                weaponPlayer);

            registration.SetActive(false);
            _registrations.Add(weapon, registration);
        }

        public void Unload(IWeapon weapon)
        {
            if (weapon == null)
                return;

            if (!_registrations.TryGetValue(weapon, out WeaponAnimationRegistration registration))
                return;

            if (ReferenceEquals(_currentWeapon, weapon))
                _currentWeapon = null;

            registration.Destroy();
            _registrations.Remove(weapon);
            _freeArmsSlots.Push(registration.ArmsSlotIndex);
        }

        public void SetCurrentWeapon(IWeapon weapon)
        {
            if (weapon != null && !_registrations.ContainsKey(weapon))
                throw new InvalidOperationException($"Weapon '{weapon}' is not loaded.");

            if (_currentWeapon != null &&
                _registrations.TryGetValue(_currentWeapon, out WeaponAnimationRegistration previous))
            {
                previous.SetActive(false);
            }

            _currentWeapon = weapon;

            if (_currentWeapon != null &&
                _registrations.TryGetValue(_currentWeapon, out WeaponAnimationRegistration current))
            {
                current.SetActive(true);
            }
        }

        public bool Play<T>(IWeapon weapon, T id) where T : Enum
        {
            WeaponAnimationRegistration registration = GetRegistrationOrThrow<T>(weapon);
            return registration.Play(id);
        }

        public bool PlayCurrent<T>(T id) where T : Enum
        {
            if (_currentWeapon == null)
            {
                Debug.LogWarning("PlayCurrent failed. No current weapon is selected.");
                return false;
            }

            return Play(_currentWeapon, id);
        }

        public bool PlayOneShot<T>(IWeapon weapon, T id) where T : Enum
        {
            WeaponAnimationRegistration registration = GetRegistrationOrThrow<T>(weapon);
            return registration.PlayOneShot(id);
        }

        public bool PlayCurrentOneShot<T>(T id) where T : Enum
        {
            if (_currentWeapon == null)
            {
                Debug.LogWarning("PlayCurrentOneShot failed. No current weapon is selected.");
                return false;
            }

            return PlayOneShot(_currentWeapon, id);
        }

        public void ResetToIdle(IWeapon weapon, float? fadeOverride = null)
        {
            WeaponAnimationRegistration registration = GetRegistrationOrThrow(weapon);
            registration.ResetToIdle(fadeOverride);
        }

        public void ResetCurrentToIdle(float? fadeOverride = null)
        {
            if (_currentWeapon == null)
                return;

            ResetToIdle(_currentWeapon, fadeOverride);
        }

        private WeaponAnimationRegistration GetRegistrationOrThrow(IWeapon weapon)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));

            if (!_registrations.TryGetValue(weapon, out WeaponAnimationRegistration registration))
                throw new InvalidOperationException($"Weapon '{weapon}' is not loaded.");

            return registration;
        }

        private WeaponAnimationRegistration GetRegistrationOrThrow<T>(IWeapon weapon) where T : Enum
        {
            WeaponAnimationRegistration registration = GetRegistrationOrThrow(weapon);

            if (registration.AnimationIdType != typeof(T))
            {
                throw new InvalidOperationException(
                    $"Weapon '{weapon}' expects animation id type '{registration.AnimationIdType.Name}', " +
                    $"but '{typeof(T).Name}' was provided.");
            }

            return registration;
        }

        private void BuildArmsGraph()
        {
            _armsGraph = PlayableGraph.Create("WeaponSystem_ArmsGraph");
            _armsGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _armsRootMixer = AnimationMixerPlayable.Create(_armsGraph, _maxLoadedWeapons, true);

            _armsOutput = AnimationPlayableOutput.Create(_armsGraph, "ArmsOutput", _armsAnimator);
            _armsOutput.SetSourcePlayable(_armsRootMixer);

            _armsGraph.Play();
        }

        private void InitializeFreeArmsSlots()
        {
            _freeArmsSlots.Clear();

            for (int i = _maxLoadedWeapons - 1; i >= 0; i--)
                _freeArmsSlots.Push(i);
        }
    }
}