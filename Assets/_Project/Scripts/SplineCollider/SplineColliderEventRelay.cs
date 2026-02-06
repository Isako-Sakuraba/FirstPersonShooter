using UnityEngine;

public class SplineColliderEventRelay : MonoBehaviour
{
    private SplineCollider _owner;
    private bool _ownerValid;

    public void SetOwner(SplineCollider owner)
    {
        _owner = owner;
        _ownerValid = owner != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_ownerValid)
            _owner.OnProxyTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_ownerValid)
            _owner.OnProxyTriggerExit(other);
    }
}
