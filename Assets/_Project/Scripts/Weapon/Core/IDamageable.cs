using UnityEngine;

namespace Game.Weapons
{
    public interface IDamageable
    {
        public void TakeDamage(int damage, Vector3 point, Vector3 normal);
    }
}