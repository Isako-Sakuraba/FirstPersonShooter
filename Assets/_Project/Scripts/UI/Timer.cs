using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class TimeStorage {
    private static List<String> storage_time = new List<string>();

    public static void AddTime(String _time) {
        storage_time.Add(_time);
    }

    public static void OutPut(Text level1, Text level2, Text level3) {
        level1.text = $"Level 1: {storage_time[0]}";
        level2.text = $"Level 2: {storage_time[1]}";
        level3.text = $"Level 3: {storage_time[2]}";
        storage_time.Clear();
    }
}

public class Timer : MonoBehaviour {

    public Text timer;

    public Text level1;
    public Text level2;
    public Text level3;
    private float time_;
    public GameObject Object;

    NextLevel script_next_level; 

    void Start() {
        script_next_level = Object.GetComponent<NextLevel>();
    }

    void Update() {
        time_ += Time.deltaTime;
        float minutes = time_ / 60;
        float seconds = time_ % 60;

        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (script_next_level.ispressed) {
            TimeStorage.AddTime(timer.text);
            if (script_next_level.is_last_level){
                TimeStorage.OutPut(level1, level2, level3);
            }

            script_next_level.ispressed = false;
        }
    }
}
