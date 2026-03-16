using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalVerletCable : MonoBehaviour
    {
        [SerializeField] private VerletRope _rope;
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _end;

        private int _endIndex = 0;

        private void Start()
        {
            _endIndex = _rope.SegmentsCount - 1;
            _rope.TryPin(0);
            _rope.TryPin(_endIndex);
        }

        private void FixedUpdate()
        {
            _rope.TrySetPosition(0, _start.position);
            _rope.TrySetPosition(_endIndex, _end.position);
        }
    }
}