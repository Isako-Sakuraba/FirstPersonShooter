using UnityEngine;

namespace Game.Weapons
{

    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        private static int _equipHash = Animator.StringToHash("Equip");

        [SerializeField] private WeaponSlot _slot;

        protected Animator weaponAnimator;
        protected Animator armsAnimator;

        public WeaponSlot Slot => _slot;

        private void Awake()
        {
            weaponAnimator = GetComponent<Animator>();
            WeaponAwake();
        }

        public virtual void WeaponAwake() { }

        public virtual void Construct(Animator armsAnimator)
        {
            this.armsAnimator = armsAnimator;
        }

        public virtual void OnEquip() 
        {
            weaponAnimator.Play(_equipHash);
        }
        public virtual void OnReload() { }

        public virtual void OnAltFireEnd(in FireContext context) { }
        public virtual void OnAltFireHold(in FireContext context) { }
        public virtual void OnAltFireStart(in FireContext context) { }

        public virtual void OnFireEnd(in FireContext context) { }
        public virtual void OnFireHold(in FireContext context) { }
        public virtual void OnFireStart(in FireContext context) { }
    }
}