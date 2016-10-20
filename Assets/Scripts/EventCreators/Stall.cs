using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Stall : MonoBehaviour {

    private List<Student> queue = new List<Student>();
    private IntervalGenerator g;
    public string ID;

    //Add Student to Queue
    //Start Serving student and Remove student

    public Event addStudent(Student s)
    {
        queue.Add(s);
        //We start serving if this is the only student!
        if (queue.Count == 1)
        {
            return new Event(GlobalEventManager.currentTime + g.next(), Event.EventType.StallDequeue, this.process,
                "Time: " + GlobalEventManager.currentTime + "Stall " + this.ID + " Finished Serving Student " + s.ID);
        }
        return null;
    }

    //Dequeues
    //If has more students in queue
    //  Add Event: StallDequeue (in future)
    public Event process()
    {
        //TODO: Notify Registry that service has ended for this student
        string msg = "Time: " + GlobalEventManager.currentTime + "Stall " + this.ID + " Finished Serving Student " + this.queue.First().ID;
        //If queue is not empty, something is wrong!
        this.queue.RemoveAt(0);
        if (queue.Count > 0)
        {
            //TODO: Notify Registry that service has started for this student
            return new Event(GlobalEventManager.currentTime + g.next(), Event.EventType.StallDequeue, process, msg);
        }
        else return null;
    }
    
	// Use this for initialization
	void Start () {
        g = new Exp(3);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
