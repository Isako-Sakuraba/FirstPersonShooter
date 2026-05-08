using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    private bool isactivate = true;
    public String level;
    public void Press() {
        if (isactivate) {
            Invoke("LoadScene", 2.0f);
        }
    }

    void LoadScene() {
        SceneManager.LoadScene(level); 
    }
}
