using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;

public class StudentManager : MonoBehaviour {
    //Dependency
    public GameObject routeManager;
    public GameObject eventManager;
    private RouteManager routeManagerScript;
    private GlobalEventManager globalEventManager;

    public GameObject studentTemplate;
    public GameObject studentGroupTemplate;
    private List<Student> students = new List<Student>();
    private IntervalGenerator g;

    //Add in terms of group, Delete in terms of individual
    public StudentGroup addStudentGroup()
    {
        GameObject groupObj = Instantiate(studentGroupTemplate);
        StudentGroup groupScript = groupObj.GetComponent<StudentGroup>();
        Node entry = routeManagerScript.map_entries[GlobalConstants.getEntry()];
        int number = 3; // GlobalConstants.getStudentNumber();
        //1. Determine Group Type.
        StudentGroup.Type type = GlobalConstants.rand.NextDouble() < GlobalConstants.TABLE_TAKER_RATIO ? StudentGroup.Type.TableFirst : StudentGroup.Type.FoodFirst;
        groupScript.type = type;
        for (int i = 0; i < number; i++)
        {
            //2. Determine Student Choice of Stall
            int stall = GlobalConstants.getStallChoice();
            GameObject newStudent = Instantiate(studentTemplate);
            Student s = newStudent.GetComponent<Student>();
            students.Add(s);
            s.initialize(routeManagerScript.map_stalls[stall].GetComponent<Stall>(), groupScript, entry);
            //3. Add to group
            groupScript.students.Add(s);
            s.transform.parent = groupObj.transform;
        }
        return groupScript;
    }

    public void advanceAllStudents(float seconds)
    {
        foreach (Student s in students)
            s.advanceFor(seconds);
    }

    public Event deleteStudent(Student s)
    {
        if (s.group.students.Count <= 1)
            Destroy(s.group.gameObject);
        //Remove from group
        s.group.students.Remove(s);
        //Remove from list
        students.Remove(s);
        //Remove from map
        Destroy(s.gameObject);
        return null;
    }

    //Return the very first event with us!
    public Event getAnotherGroup()
    {
        //add entry event
        float time = GlobalEventManager.currentTime + (float)(g.next());
        //Debug.Log("Time: " + GlobalEventManager.currentTime + " Next Group Scheduled at: " + time);
        Event e = new Event(time, Event.EventType.CanteenArrival, this.getAnotherGroup,
             "Time: " + GlobalEventManager.currentTime + " Student Group Arrival");

        //Currently, for testing purpose only. This group has ENTERED NOW
        //TODO: Make student go loop
        StudentGroup group = addStudentGroup();
        Node destination = routeManagerScript.map_entries[1];
        foreach (Student s in group.students)
        {
            List<Node> path = routeManagerScript.getPath(s.prevNode, destination);
            s.setPositionAndRoute(s.prevNode.coordinates, path);
            Debug.Log("Student ID: " + s.ID);
            Func<Event> delete = () => {
                Debug.Log("Delete Student ID: " + s.ID);
                return deleteStudent(s);
            };    //This event does not prompt another event
            //Triggered when student finishes the walk
            //TODO: Add bool isRandomWalking to student
            float expectedExitTime = GlobalEventManager.currentTime + Math.Max(path.Count - 1, 0) / GlobalConstants.WALK_SPEED;

            //Debug.Log("Time: " + GlobalEventManager.currentTime + " No. of Students: " + group.students.Count + " exiting at: " + expectedExitTime);
            routeManagerScript.Highlight(path);

            Event walk = new Event(expectedExitTime, Event.EventType.CanteenDeparture, delete,
                 "Time: " + GlobalEventManager.currentTime + " Student Reached a point to leave ");
            globalEventManager.addEvent(walk);
        }
        return null;
    }

    // Use this for initialization
    public void initialize()
    {
        routeManagerScript = routeManager.GetComponent<RouteManager>();
        globalEventManager = eventManager.GetComponent<GlobalEventManager>();
        g = new StudentEntry();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
