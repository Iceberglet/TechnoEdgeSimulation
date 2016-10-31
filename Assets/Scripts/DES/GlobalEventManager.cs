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
    private static int speedFactor = 0;   //0, 1, 10, 100, 1000
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
        StudentManager.NumberOfPeopleInSystem = 0;
        currentTime = 0;
        runningTime = 0;
        speedFactor = speed;
        events = new List<Event>();
        GlobalConstants.loadInitialCondition();
        GlobalRegistry.initialize();
        routeMan = routeManager.GetComponent<RouteManager>();
        studentMan = studentManager.GetComponent<StudentManager>();
        tableMan = tableManager.GetComponent<TableManager>();
        routeMan.initialize();
        studentMan.initialize();
        tableMan.initialize(routeMan, this, studentMan);
        //Add students to stall queues as initialization

        addEvent(studentMan.getAnotherGroup(0));
        addEvent(studentMan.getAnotherGroup(1));
        addEvent(studentMan.getAnotherGroup(2));
        UIManager.updateImportantMessage("Started with config: [SEED] " + GlobalConstants.RANDOM_SEED +
                                                             " [TAKER] " + GlobalConstants.TABLE_TAKER_RATIO +
                                                             " [SHARER] " + GlobalConstants.TABLE_SHARER_RATIO);
        GlobalRegistry.initializeData();
        addEvent(adjustStallProba());
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
            if(toExecute.type == Event.EventType.StallDequeue)
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


    int favourableFactor = 25;  //The higher the value, the less students are driven off by long queues
    int updateInterval = 20;

    public Event adjustStallProba()
    {
        int[] queueLengths = routeMan.map_stalls.Select(g => g.GetComponent<Stall>().queueLength).ToArray();
        float[] currentCumuProba = GlobalConstants.STALL_PROBA_ORIGINAL;
        float[] newProba = new float[currentCumuProba.Length];
        float totalProba = 0;
        for (int i = 0; i < currentCumuProba.Length; ++i)
        {
            float newMarginalProba = (currentCumuProba[i] - (i > 0 ? currentCumuProba[i - 1] : 0)) * Mathf.Exp(-queueLengths[i] / favourableFactor);
            totalProba += newMarginalProba;
            newProba[i] = totalProba;
        }
        newProba = newProba.ToList().Select(p=>p/totalProba).ToArray();
        //Debug.Log("A: " + string.Join(" ", newProba.ToList().Select(p => p.ToString("F2")).ToArray()));

        GlobalConstants.STALL_PROBA = newProba;
        return new Event(GlobalEventManager.currentTime + updateInterval, Event.EventType.UpdateStudentGenerator, adjustStallProba, "Adjusting Probabilities");
    }
}
