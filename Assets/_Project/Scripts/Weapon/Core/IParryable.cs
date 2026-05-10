using UnityEngine;

namespace Game.Weapons
{
    public readonly struct ParryContext
    {
        public readonly Vector3 Direction;

        public ParryContext(Vector3 direction)
        {
            Direction = direction;
        }
    }

    public interface IParryable
    {
        public void OnParry(in ParryContext context);
    }
}