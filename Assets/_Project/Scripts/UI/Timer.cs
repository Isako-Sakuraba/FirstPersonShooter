using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class TimeStorage {
    private static List<String> storage_time = new List<string>();
    private static List<float> storage_int_time = new List<float>();

  

    public static void AddTime(String _time, float int_time) {
        storage_time.Add(_time);
        storage_int_time.Add(int_time);
    }

    public static void OutPut(Text level1, Text level2, Text level3, Text mark) {
        level1.text = $"Level 1: {storage_time[0]}";
        level2.text = $"Level 2: {storage_time[1]}";
        level3.text = $"Level 3: {storage_time[2]}";

        if ((storage_int_time[0] < 60 && storage_int_time[1] >= 70 && storage_int_time[2] >= 80) ||
            (storage_int_time[0] >= 60 && storage_int_time[1] < 70 && storage_int_time[2] >= 80) || 
            (storage_int_time[0] >= 60 && storage_int_time[1] >= 70 && storage_int_time[2] < 80)) {
            mark.text = "C";
        }
        else if ((storage_int_time[0] < 60 && storage_int_time[1] < 70 && storage_int_time[2] >= 80) ||
            (storage_int_time[0] < 60 && storage_int_time[1] >= 70 && storage_int_time[2] < 80) || 
            (storage_int_time[0] >= 60 && storage_int_time[1] < 70 && storage_int_time[2] < 80)) {
            mark.text = "B";
        }
        else if (storage_int_time[0] < 60 && storage_int_time[1] < 70 && storage_int_time[2] < 80) {
            mark.text = "A";
        }
        else {
            mark.text = "D";
        }

        storage_time.Clear();
        storage_int_time.Clear();
    }
}

public class Timer : MonoBehaviour {

    public Text timer;

    public Text level1;
    public Text level2;
    public Text level3;

    public Text mark;

    private float time_;
    public GameObject Object;

    NextLevel script_next_level; 

    void Start() {
        script_next_level = Object.GetComponent<NextLevel>();
    }

    void Update() {
        time_ += Time.deltaTime;
        int minutes = (int) time_ / 60;
        int seconds = (int) time_ % 60;

        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (script_next_level.ispressed) {
            TimeStorage.AddTime(timer.text, time_);
            if (script_next_level.is_last_level){
                TimeStorage.OutPut(level1, level2, level3, mark);
            }

            script_next_level.ispressed = false;
        }
    }
}
