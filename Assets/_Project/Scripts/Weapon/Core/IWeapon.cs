using UnityEngine;

namespace Game.Weapons
{
    public readonly struct FireContext
    {
        public readonly Vector3 Direction;
        public readonly Vector3 Position;
        public readonly LayerMask LayerMask;

        public FireContext(Vector3 direction, Vector3 position, LayerMask layerMask)
        {
            Direction = direction;
            Position = position;
            LayerMask = layerMask;
        }
    }

    public interface IWeapon
    {
        public void Construct(Animator armsAnimator);

        public void OnFireStart(in FireContext context);
        public void OnFireHold(in FireContext context);
        public void OnFireEnd(in FireContext context);

        public void OnAltFireStart(in FireContext context);
        public void OnAltFireHold(in FireContext context);
        public void OnAltFireEnd(in FireContext context);

        public void OnEquip();
        public void OnReload();
    }
}