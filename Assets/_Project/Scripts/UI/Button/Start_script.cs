using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Start_script : MonoBehaviour {
    public Button start_button;

    void Start() {
        start_button.onClick.AddListener(LoadLevel1);
    }

    public void LoadLevel1() {
        SceneManager.LoadScene("Level1");
    }
}