using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

//The ultimate time tracker
public class GlobalEventManager : MonoBehaviour {

    public GameObject routeManager;
    public GameObject studentManager;
    private RouteManager routeMan;
    private StudentManager studentMan;
    public GameObject tableManager;
    private TableManager tableMan;

    //Length of a typical run is 2 hours
    //If needs to be done within 20 seconds at fastest speed 
    // -> 1 frame = 1 deltaTime
    // -> the simulated time elapsed should be (deltaTime * 2 * 3600 / 20 = 360 * deltaTime)
    private static int speedFactor;   //0, 1, 10, 100, 1000
    private static bool disabled;     //Should animation run?

    private static List<Event> events;
    private static float runningTime;   //Used for speed control
    public static float currentTime { get; private set; }   //Used by various Event Generators - measured in seconds

	void Start ()
    {
        currentTime = 0;
        runningTime = 0;
        speedFactor = 20;
        events = new List<Event>();
        routeMan = routeManager.GetComponent<RouteManager>();
        studentMan = studentManager.GetComponent<StudentManager>();
        tableMan = tableManager.GetComponent<TableManager>();
        routeMan.initialize();
        studentMan.initialize(tableMan);
        tableMan.initialize(routeMan, this, studentMan);

        addEvent(studentMan.getAnotherGroup());
	}
	
	void Update () {
        int speedForThisFrame = speedFactor;
        float elapsedTime = speedForThisFrame * Time.deltaTime;
        runningTime += elapsedTime;

        //Execute Events in Order
        while(events.Count > 0 && events.First().timeStamp < runningTime)
        {
            currentTime = events.First().timeStamp;
            //Execute the first event - which may add another event
            Event toExecute = events.First();
            if(toExecute.type != Event.EventType.RoamToPoint)
                Debug.Log("Event " + toExecute.ID + " " + toExecute.msg);
            this.addEvent(toExecute.execute());
            events.Remove(toExecute);
        }
        //Finish up by move the students that are en route?
        if(speedForThisFrame < 100)
            studentMan.advanceAllStudents(elapsedTime);
    }

    public void addEvent(Event e)
    {
        if (e == null)
            return;
        if (events.Count == 0)
        {
            events.Add(e);
            return;
        }
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
