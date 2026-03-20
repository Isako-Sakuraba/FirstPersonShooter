using UnityEngine;

namespace Game.Weapons
{
    public enum DamageType
    {
        Melee,
        Piercing,
        Explosion
    }

    public enum DamageSender
    {
        Player,
        Enemy
    }

    public readonly struct DamageResult
    {
        public readonly Vector3? Point;

        public DamageResult(Vector3? point)
        {
            Point = point;
        }
    }

    public readonly struct DamageContext
    {
        public readonly int Damage;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly DamageSender Sender;
        public readonly DamageType DamageType;

        public DamageContext(
            int damage, 
            Vector3 point, 
            Vector3 normal, 
            DamageSender sender, 
            DamageType damageType)
        {
            Damage = damage;
            Point = point;
            Normal = normal;
            Sender = sender;
            DamageType = damageType;
        }
    }

    public interface IDamageable
    {
        public DamageResult TakeDamage(in DamageContext context);
    }
}