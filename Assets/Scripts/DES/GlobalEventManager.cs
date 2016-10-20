using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

//The ultimate time tracker
public class GlobalEventManager : MonoBehaviour {

    //Length of a typical run is 2 hours
    //If needs to be done within 20 seconds at fastest speed 
    // -> 1 frame = 1 deltaTime
    // -> the simulated time elapsed should be (deltaTime * 2 * 3600 / 20 = 360 * deltaTime)
    private static int speedFactor;   //0, 1, 10, 100, 1000
    private static bool disabled;     //Should animation run?

    private static List<Event> events;
    private static double runningTime;   //Used for speed control
    public static double currentTime { get; private set; }   //Used by various Event Generators - measured in seconds

	void Start () {
        runningTime = 0;
        events = new List<Event>();

        //1. Initialize the states. (Tables, Stall queues, etc.)

        //2. Start the StudentCreatorService


	}
	
	void Update () {
        double elapsedTime = speedFactor * Time.deltaTime;
        runningTime += elapsedTime;

        //Execute Events in Order
        while(events.First().timeStamp < runningTime)
        {
            currentTime = events.First().timeStamp;
            //Execute the first event - which may add another event
            this.addEvent(events.First().execute());
        }

        //TODO: Finish up by move the students that are en route?
	}

    public void addEvent(Event e)
    {
        if (e == null)
            return;
        if (events.Count == 0)
            events.Add(e);
        for (int i = 0; i < events.Count; ++i)
        {
            if(e.timeStamp < events.ElementAt(i).timeStamp)
            {
                events.Insert(i, e);
                return;
            }
        }
        events.Add(e);
    }
}
