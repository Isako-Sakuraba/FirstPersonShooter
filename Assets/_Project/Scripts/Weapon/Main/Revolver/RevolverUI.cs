using UnityEngine;
using UnityEngine.UI;

namespace Game.Weapons.Main
{
    public class RevolverUI : MonoBehaviour
    {
        [SerializeField] private Revolver _revolver;
        [SerializeField] private Image _coinFill1;
        [SerializeField] private Image _coinFill2;
        [SerializeField] private Image _coinFill3;
        [SerializeField] private Image _coinFill4;

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
            if (_revolver == null)
            {
                return;
            }

            SetFill(_coinFill1, _revolver.GetCoinFill01(0));
            SetFill(_coinFill2, _revolver.GetCoinFill01(1));
            SetFill(_coinFill3, _revolver.GetCoinFill01(2));
            SetFill(_coinFill4, _revolver.GetCoinFill01(3));
        }

        private static void SetFill(Image image, float fill)
        {
            if (image == null)
            {
                return;
            }

            image.fillAmount = Mathf.Clamp01(fill);
        }
    }
}
