using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TestDist : MonoBehaviour {

    int counter = 0;
    public GameObject prefab;

	// Use this for initialization
	void Start () {
        IntervalGenerator g = GenericDistribution.createInstanceFromFile("input.csv");
    }
	
	// Update is called once per frame
	void Update () {
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
