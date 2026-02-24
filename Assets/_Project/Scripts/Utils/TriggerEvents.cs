using System;
using UnityEngine;

namespace Game.Utils
{
    public class TriggerEvents : MonoBehaviour
    {
        public event Action<Collider> TriggerEntered = delegate { };
        public event Action<Collider> TriggerExited = delegate { };
        public event Action<Collider> TriggerStayed = delegate { };

        private void OnTriggerEnter(Collider other)
        {
            TriggerEntered.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExited.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TriggerStayed.Invoke(other);
        }
    }
}