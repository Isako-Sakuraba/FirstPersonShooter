using System.Runtime.CompilerServices;

namespace Game.Movement
{
    public struct Pending<T>
    {
        public bool IsPresent { get; private set; }
        private T _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(in T payload)
        {
            _value = payload;
            IsPresent = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryConsume(out T payload)
        {
            if (!IsPresent)
            {
                payload = default;
                return false;
            }

            payload = _value;
            _value = default;
            IsPresent = false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _value = default;
            IsPresent = false;
        }
    }
}