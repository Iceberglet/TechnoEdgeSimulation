using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestDist : MonoBehaviour {

    int counter = 0;

	// Use this for initialization
	void Start () {
        IntervalGenerator g = new Normal(10, 5);
        double sum = 0;
        List<double> d = new List<double>();
        while (counter < 10000)
        {
            counter++;
            double another = g.next();
            sum += another;
            d.Add(another);
        }
        double avg = sum / 10000;
        double var = 0;
        foreach(double x in d)
        {
            var += (avg - x) * (avg - x);
        }
        var /= 9999;
        Debug.Log(avg + ", " + var);
    }
	
	// Update is called once per frame
	void Update () {
    }
}
