using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class TestDist : MonoBehaviour {

    int counter = 0;
    System.Random rand = new System.Random();


    private float[] sampleCount = new float[10];
    private float[] proba = new float[]
    {
        0.1f,0.2f,0.3f,0.4f,0.5f,0.6f,0.7f,0.8f,0.9f,1
    };

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        double p = rand.NextDouble();
        for (int i = 0; i < proba.Length; ++i)
        {
            if (p < proba[i])
                sampleCount[i]++;
        }
        if(p > sampleCount[proba.Length - 2])
            sampleCount[proba.Length - 1]++;
        counter++;

        //Debug.Log(string.Join(" ", sampleCount.Select(s=>s.ToString()).ToArray()));
        string[] empiricalProba = sampleCount.ToList().Select(pp => (pp / counter).ToString("F2")).ToArray();
        Debug.Log(string.Join(" ", empiricalProba));
    }
}

class LOL
{
    public long ID;
    public LOL(long id)
    {
        ID = id;
    }
}
