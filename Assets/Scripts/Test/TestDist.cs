using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TestDist : MonoBehaviour {

    int counter = 0;
    public GameObject prefab;

	// Use this for initialization
	void Start () {
        GameObject g = Instantiate(prefab);
        g.transform.position = new Vector3(5, 5, 0);
        /*
        List<LOL> lols = new List<LOL>();
        lols.Add(new LOL(1));
        lols.Add(new LOL(2));
        lols.Add(new LOL(3));

        List<Func<int>> fs = new List<Func<int>>();
        foreach (LOL l in lols)
        {
            Func<int> func = () =>
            {
                Debug.Log(l.ID);   //prints 3,3,3 instead of 1,2,3
                return 0;
            };

            ** Change to the following and it works! for no apparent reason!
            LOL another = l;
            Func<int> func = () =>
            {
                Debug.Log(another.ID);
                return 0;
            };


            fs.Add(func);
        }
        foreach (Func<int> f in fs)
        {
            f();
        }*/
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
