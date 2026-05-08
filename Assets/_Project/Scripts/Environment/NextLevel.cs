using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    private bool isactivate = true;
    public String level;
    public void Press() {
        if (isactivate) {
           SceneManager.LoadScene(level); 
        }
    }
}
