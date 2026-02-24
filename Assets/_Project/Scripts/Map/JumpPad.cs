using ECM2;
using UnityEngine;

namespace Game.Environment
{
    public class JumpPad : MonoBehaviour
    {
        [SerializeField] private Vector3 direction;
        [SerializeField] private float force;

        [SerializeField] private bool overrideVelocity = true;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + direction.normalized * force);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<CharacterMovement>(out var character))
            {
                character.LaunchCharacter(direction.normalized * force, overrideVelocity, overrideVelocity);
                character.PauseGroundConstraint();
            }
        }
    }
}