using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public Text timer;
    private float time_;
    void Start() {

    }

    void Update() {
        time_ += Time.deltaTime;
        float minutes = time_ / 60;
        float seconds = time_ % 60;

        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
