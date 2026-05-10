using UnityEngine;
using UnityEngine.UI;

public class Exit_script : MonoBehaviour {
    public Button exit_button;

    void Update() {
        exit_button.onClick.AddListener(Exit);
    }

    public void Exit() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
