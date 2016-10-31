using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Stall : MonoBehaviour {
    private TableManager tableManager;
    private GlobalEventManager globalEventManager;
    private List<Student> queue = new List<Student>();
    private IntervalGenerator g;    //Service rate interval generator
    private int servers = 1;
    public int ID;
    public new string name { get; private set; }
    public Node node { get; private set; }
    public List<Node> pathToMainLoop { get; private set; }
    public int queueLength
    {
        get { return queue.Count; }
    }

    //Add Student to Queue
    //Start Serving student and Remove student

    public Event addStudent(Student s)
    {
        queue.Add(s);
        //We start serving if this is the only student!
        if (queue.Count <= servers)
        {
            Event e = new Event(GlobalEventManager.currentTime + g.next(), Event.EventType.StallDequeue, this.process,
                "Time: " + GlobalEventManager.currentTime + " Stall " + this.ID + " Finished Serving Student " + s.ID);
            globalEventManager.addEvent(e);
        }
        return null;
    }

    //Dequeues
    //If has more students in queue
    //  Add Event: StallDequeue (in future)
    public Event process()
    {
        //TODO: Notify Registry that service has ended for this student
        string msg = "Time: " + GlobalEventManager.currentTime + " Stall " + this.ID + " Finished Serving Student " + this.queue.First().ID;

        //Let this student go look for his table
        this.queue.ElementAt(0).hasFood = true;
        globalEventManager.addEvent(tableManager.addTableSearchingStudent(this.queue.First()));

        this.queue.RemoveAt(0);
        //If queue is not empty, something is wrong!
        if (queue.Count > 0)
        {
            //TODO: Notify Registry that service has started for this student
            return new Event(GlobalEventManager.currentTime + g.next(), Event.EventType.StallDequeue, process, msg);
        }
        else return null;
    }

    public void initializeTableManager(TableManager t)
    {
        this.tableManager = t;
    }

    public void initialize(int ID, Node n, StudentManager st, GlobalEventManager g)
    {
        this.g = new Exp(20);
        this.ID = ID;
        this.name = GlobalConstants.STALL_IDS[ID];
        this.servers = GlobalConstants.STALL_SERVER[ID];
        this.g = GlobalConstants.STALL_SERVICE_INTERVALS[ID];
        this.node = n;
        this.globalEventManager = g;

        int numberOfInitialStudent = GlobalConstants.initialStallQueues[ID];
        
        for (int i = 0; i < numberOfInitialStudent; i++)
        {
            Student s = st.getDummyStudent(ID, node);
            Event e = addStudent(s);
            //if(e != null)
            //    Debug.Log("Next Dequeue: " + e.timeStamp + " " + e.type);
            globalEventManager.addEvent(e);
        }
    }


    void Update()
    {
        float degreeOfDanger = Mathf.Min(queue.Count / 60.0f, 1);
        queueText.text = queue.Count.ToString();
        queueText.color = Utility.HSVToRGB((1 - degreeOfDanger) * 0.4f, 0.9f, 0.9f);
        //queueText.color = Color.Lerp(Color.green, Color.red, degreeOfDanger);
    }

    private TextMesh queueText
    {
        get { return this.gameObject.GetComponentInChildren<TextMesh>(); }
    }

    void OnMouseEnter()
    {
        //UIManager.updateImportantMessage("Selected Stall: " + name + " with queue length: " + queue.Count);
        queueText.fontSize = 250;
    }
    void OnMouseExit()
    {
        //UIManager.updateImportantMessage("Selected Stall: " + name + " with queue length: " + queue.Count);
        queueText.fontSize = 100;
    }
}
