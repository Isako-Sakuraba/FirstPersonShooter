using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class To_the_menu_script : MonoBehaviour {
    public Button menu_button;
    void Start()
    {
        menu_button.onClick.AddListener(Load_Menu);
    }

    public void Load_Menu() {
        SceneManager.LoadScene("Menu");
    }
}
