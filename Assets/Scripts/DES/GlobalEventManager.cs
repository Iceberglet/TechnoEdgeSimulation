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
    private static int speedFactor = 1;   //0, 1, 10, 100, 1000
    private static bool isRunning = false;     //Should animation run?

    private static List<Event> events;
    public static float runningTime { get; private set; }   //Used for speed control
    public static float currentTime { get; private set; }   //Used by various Event Generators - measured in seconds

	void Start ()
    {
        reStart();
	}

    private void reStart(int speed = 1)
    {
        currentTime = 0;
        runningTime = 0;
        speedFactor = speed;
        events = new List<Event>();
        routeMan = routeManager.GetComponent<RouteManager>();
        studentMan = studentManager.GetComponent<StudentManager>();
        tableMan = tableManager.GetComponent<TableManager>();
        routeMan.initialize();
        studentMan.initialize(tableMan);
        tableMan.initialize(routeMan, this, studentMan);
        GlobalRegistry.initialize(routeMan.tables);
        addEvent(studentMan.getAnotherGroup());


        UIManager.updateImportantMessage("Started with config: [SEED] " + GlobalConstants.RANDOM_SEED +
                                                             " [TAKER] " + GlobalConstants.TABLE_TAKER_RATIO +
                                                             " [SHARER] " + GlobalConstants.TABLE_SHARER_RATIO);
    }

    public void ChangeSpeed(int newSpeed)
    {
        if (!isRunning)
        {
            Application.LoadLevel(Application.loadedLevel);
            isRunning = true;
            reStart(newSpeed);
        }
        speedFactor = newSpeed;
    }

    public void Stop()
    {
        if (!isRunning)
            return;
        isRunning = false;
        //Record result;
        GlobalRegistry.output();
        UIManager.updateImportantMessage("System Stopped, Data Recorded");
    }
	
	void Update () {
        if (!isRunning)
            return;
        
        if (runningTime > GlobalConstants.SIMULATION_TIME)
        {
            Stop();
        }

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
                UIManager.updateEventText("Event " + toExecute.ID + " " + toExecute.msg);
            this.addEvent(toExecute.execute());
            events.Remove(toExecute);
        }
        //Finish up by move the students that are en route?
        if(speedForThisFrame < 101)
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
