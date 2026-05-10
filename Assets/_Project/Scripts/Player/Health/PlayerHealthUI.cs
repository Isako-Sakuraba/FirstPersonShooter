using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Text _healthText;
        [SerializeField] private string _healthTextFormat = "{0}/{1}";

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            PlayerHealth health = _playerHealth != null ? _playerHealth : PlayerHealth.Instance;
            if (health == null)
            {
                return;
            }

            if (_healthFillImage != null)
            {
                _healthFillImage.fillAmount = health.Health01;
            }

            if (_healthText != null)
            {
                string format = string.IsNullOrEmpty(_healthTextFormat) ? "{0}/{1}" : _healthTextFormat;
                _healthText.text = string.Format(format, health.CurrentHealth, health.MaxHealth);
            }
        }
    }
}
