using UnityEngine;

namespace Game.Weapons
{
    public readonly struct FireContext
    {
        public readonly Transform Direction;

        public FireContext(Transform direction)
        {
            Direction = direction;
        }
    }

    public interface IWeapon
    {
        public void OnFireStart(in FireContext context);
        public void OnFireHold(in FireContext context);
        public void OnFireEnd(in FireContext context);

        public void OnAltFireStart(in FireContext context);
        public void OnAltFireHold(in FireContext context);
        public void OnAltFireEnd(in FireContext context);

        public void OnReload();
    }
}