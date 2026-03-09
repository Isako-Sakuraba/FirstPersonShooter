using System;
using UnityEngine;

namespace Game.Systems
{
    public static class HitstopSystem
    {
        public static event Action OnHitstopStart = delegate { };
        public static event Action OnHitstopEnd = delegate { };

        private static bool _hitstop = false;
        public static bool Hitstop => _hitstop;

        public static bool Trigger()
        {
            if (_hitstop)
                return false;

            int id = TimeSystem.Slow(0.1f, 0.2f, HitstopEnd);
            _hitstop = id != -1;

            if (_hitstop)
                OnHitstopStart.Invoke();

            return _hitstop;
        }

        private static void HitstopEnd()
        {
            _hitstop = false;
            OnHitstopEnd.Invoke();
        }
    }
}