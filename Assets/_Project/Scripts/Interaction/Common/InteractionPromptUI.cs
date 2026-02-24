using TMPro;
using UnityEngine;

namespace Game.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        private Transform _camera;

        private void Start()
        {
            _camera = Camera.main.transform;
        }

        private void LateUpdate()
        {
            var rotation = _camera.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        }

        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}