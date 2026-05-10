using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    private bool isactivate = true;

    public bool is_last_level;
    [HideInInspector] public bool ispressed = false;
    public String level;
    public Canvas results;

    public Canvas loading_screen;

    public GameObject gameObject;

    public Light light;

    private Count_Enemies_script count_Enemies_Script;

    void Start() {
        count_Enemies_Script = gameObject.GetComponent<Count_Enemies_script>();
    }

    void Update() {
        if (count_Enemies_Script.isfree) {
            light.color = Color.green;
        }
    }

    public void Press() {
        if (count_Enemies_Script.isfree) {
            ispressed = true;
            if (is_last_level) {
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                results.gameObject.SetActive(true);
            }
            else {
                loading_screen.gameObject.SetActive(true);
                Invoke("LoadScene", 2.0f);
            }
        }
    }

    void LoadScene() {
        SceneManager.LoadScene(level); 
    }
}
