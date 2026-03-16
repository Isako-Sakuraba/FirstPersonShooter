using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons.Experimental
{
    public class ExperimentalBloodParticlesCollisions : MonoBehaviour
    {
        private ExperimentalBloodSplatManager _splatManager;

        private readonly List<ParticleCollisionEvent> _events = new();

        private void Start()
        {
            _splatManager = ExperimentalBloodSplatManager.Instance;
        }

        private void OnParticleCollision(GameObject other)
        {
            var ps = GetComponent<ParticleSystem>();
            int count = ParticlePhysicsExtensions.GetCollisionEvents(ps, other, _events);

            for (int i = 0; i < count; i++)
            {
                Vector3 point = _events[i].intersection;
                Vector3 normal = _events[i].normal;
                
                _splatManager.Spawn(point, normal, gameObject.layer); 
            }
        }
    }
}