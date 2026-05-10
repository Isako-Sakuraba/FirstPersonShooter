using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Systems;
using Game.Weapons.Main;

namespace Game.Weapons.Experimental
{
    public class PlayerWeaponSystem : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Transform _fireDirection;
        [SerializeField] private AudioSource _weaponAudio;
        [SerializeField] private Animator _armsAnimator;
        [SerializeField] private LayerMask _hittableLayers;

        [Header("Weapons")]
        [SerializeField] private Fists _fists;
        [SerializeField] private List<WeaponBase> _weaponList;
        [SerializeField] public bool GiveWeaponOnStart = true;

        private WeaponInstance _currentWeapon;
        private bool _hasWeapon;

        private WeaponSlot _currentSlot;
        private int _currentSlotIndex;

        private readonly Dictionary<WeaponSlot, WeaponInstance> _weapons = new(5);
        private List<WeaponSlot> _availableSlots;

        private InputService _inputService;
        private PauseMenu _pauseMenu;

        private void Awake()
        {
            _inputService = InputService.Instance;
            _pauseMenu = FindFirstObjectByType<PauseMenu>();
            _availableSlots = Enum.GetValues(typeof(WeaponSlot))
                .Cast<WeaponSlot>()
                .ToList();
            _availableSlots.Remove(WeaponSlot.Melee);
        }

        private void Start()
        {
            _fists.Construct(_armsAnimator, _weaponAudio);

            foreach (var weapon in _weaponList)
            {
                var instance = new WeaponInstance { weapon = weapon, gameObject = weapon.gameObject };
                weapon.Construct(_armsAnimator, _weaponAudio);
                _weapons[weapon.Slot] = instance;
                instance.gameObject.SetActive(false);
            }

            for (int i = _availableSlots.Count - 1;  i >= 0; i--)
            {
                if (!_weapons.ContainsKey(_availableSlots[i]))
                    _availableSlots.RemoveAt(i);
            }

            if (GiveWeaponOnStart && _availableSlots.Count > 0)
            {
                var slot = _availableSlots[0];
                SetCurrentWeapon(slot);
                _hasWeapon = true;
            }
        }

        private void SetCurrentWeapon(WeaponSlot slot)
        {
            if (!_weapons.TryGetValue(slot, out var instance))
                return;

            _currentWeapon?.gameObject.SetActive(false);
            _currentWeapon = instance;
            _currentWeapon.gameObject.SetActive(true);
            _currentWeapon.weapon.OnEquip();

            int slotIndex = _availableSlots.IndexOf(slot);
            if (slotIndex >= 0)
            {
                _currentSlotIndex = slotIndex;
                _currentSlot = slot;
            }

            _hasWeapon = true;
        }

        private void Update()
        {
            if (_pauseMenu != null && _pauseMenu.IsOpen)
            {
                return;
            }

            UpdatePassiveWeaponStates();

            var context = new FireContext(_fireDirection.forward, _fireDirection.position, _hittableLayers);

            TrySelectWeaponByButtons();

            if (_inputService.PunchPressed)
            {
                if (!TryProjectileBoostParry())
                {
                    _fists.OnFireStart(in context);
                }
            }

            if (!_hasWeapon)
                return;

            TryChangeWeapon();

            if (_inputService.FirePressed)
                _currentWeapon.weapon.OnFireStart(in context);

            if (_inputService.FireHeld)
                _currentWeapon.weapon.OnFireHold(in context);

            if (_inputService.FireReleased)
                _currentWeapon.weapon.OnFireEnd(in context);

            if (_inputService.AltFirePressed)
                _currentWeapon.weapon.OnAltFireStart(in context);

            if (_inputService.AltFireHeld)
                _currentWeapon.weapon.OnAltFireHold(in context);

            if (_inputService.AltFireReleased)
                _currentWeapon.weapon.OnAltFireEnd(in context);
        }

        private void UpdatePassiveWeaponStates()
        {
            int count = _weaponList.Count;
            for (int i = 0; i < count; i++)
            {
                WeaponBase weapon = _weaponList[i];

                Revolver revolver = weapon as Revolver;
                if (revolver == null)
                {
                    SMG smg = weapon as SMG;
                    if (smg != null)
                    {
                        smg.TickBulletRecharge();
                    }

                    continue;
                }

                revolver.TickCoinRecharge(true);
            }
        }

        private bool TryProjectileBoostParry()
        {
            if (!_hasWeapon)
            {
                return false;
            }

            Shotgun shotgun = _currentWeapon?.weapon as Shotgun;
            if (shotgun == null)
            {
                return false;
            }

            if (!shotgun.TryTriggerProjectileBoost())
            {
                return false;
            }

            _armsAnimator.Play("Parry", -1, 0.17f);
            HitstopSystem.Trigger();
            return true;
        }

        private void TryChangeWeapon()
        {
            if (_availableSlots.Count == 0 || _inputService.Scroll == 0)
            {
                return;
            }

            var nextSlotIndex = _currentSlotIndex + _inputService.Scroll;

            if (nextSlotIndex < 0)
                nextSlotIndex = _availableSlots.Count - 1;

            nextSlotIndex %= _availableSlots.Count;

            if (nextSlotIndex != _currentSlotIndex)
            {
                _currentSlotIndex = nextSlotIndex;
                _currentSlot = _availableSlots[_currentSlotIndex];
                SetCurrentWeapon(_currentSlot);
            }
        }

        private void TrySelectWeaponByButtons()
        {
            if (_availableSlots.Count == 0)
            {
                return;
            }

            if (_inputService.Weapon1Pressed)
            {
                TrySelectWeaponByIndex(0);
                return;
            }

            if (_inputService.Weapon2Pressed)
            {
                TrySelectWeaponByIndex(1);
                return;
            }

            if (_inputService.Weapon3Pressed)
            {
                TrySelectWeaponByIndex(2);
            }
        }

        private void TrySelectWeaponByIndex(int index)
        {
            if (index < 0 || index >= _availableSlots.Count)
            {
                return;
            }

            WeaponSlot slot = _availableSlots[index];
            if (_hasWeapon && slot == _currentSlot)
            {
                return;
            }

            SetCurrentWeapon(slot);
        }

        private class WeaponInstance
        {
            public IWeapon weapon;
            public GameObject gameObject;
        }
    }
}
