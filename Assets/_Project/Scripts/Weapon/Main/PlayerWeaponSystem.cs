using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Weapons.Experimental
{
    public class PlayerWeaponSystem : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Transform _fireDirection;
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

        private void Awake()
        {
            _inputService = InputService.Instance;
            _availableSlots = Enum.GetValues(typeof(WeaponSlot))
                .Cast<WeaponSlot>()
                .ToList();
            _availableSlots.Remove(WeaponSlot.Melee);
        }

        private void Start()
        {
            _fists.Construct(_armsAnimator);

            foreach (var weapon in _weaponList)
            {
                var instance = new WeaponInstance { weapon = weapon, gameObject = weapon.gameObject };
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
            _hasWeapon = true;
        }

        private void Update()
        {
            var context = new FireContext(_fireDirection.forward, _fireDirection.position, _hittableLayers);

            if (_inputService.PunchPressed)
                _fists.OnFireStart(in context);

            if (!_hasWeapon)
                return;

            TryChangeWeapon();

            if (_inputService.FirePressed)
                _currentWeapon.weapon.OnFireStart(in context);

            if (_inputService.FireHeld)
                _currentWeapon.weapon.OnFireHold(in context);

            if (_inputService.FireReleased)
                _currentWeapon.weapon.OnFireEnd(in context);
        }

        private void TryChangeWeapon()
        {
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

        private class WeaponInstance
        {
            public IWeapon weapon;
            public GameObject gameObject;
        }
    }
}