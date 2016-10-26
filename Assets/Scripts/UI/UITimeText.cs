using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UITimeText : MonoBehaviour {
    private Text t;

	// Use this for initialization
	void Start () {
        t = this.gameObject.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        t.text = "Current Time: " + parseTime(GlobalEventManager.runningTime);
	}

    string parseTime(float t)
    {
        int inSecond = (int)Math.Floor(t);
        int hour = inSecond / 3600 + 12;
        int minutes = (inSecond % 3600) / 60;
        int seconds = inSecond % 60;
        return hour + (minutes < 10? ":0" : ":") + minutes 
                    + (seconds < 10? ":0" : ":") + seconds;
    }
}
