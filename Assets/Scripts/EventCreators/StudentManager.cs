﻿using System;
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
    private TableManager tableManager;

    public GameObject studentTemplate;
    public GameObject studentGroupTemplate;
    private List<Student> students = new List<Student>();   //Needed for collective movement
    private IntervalGenerator arrivalIntervalGenerator;
    private IntervalGenerator eatingTimeGenerator;

    //Add in terms of group, Delete in terms of individual
    public StudentGroup addStudentGroup()
    {
        GameObject groupObj = Instantiate(studentGroupTemplate);
        StudentGroup groupScript = groupObj.GetComponent<StudentGroup>();
        Node entry = routeManagerScript.map_entries[GlobalConstants.getEntry()];
        int number = 4; // GlobalConstants.getStudentNumber();
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
            s.initialize(routeManagerScript.map_stalls[stall].GetComponent<Stall>(), groupScript, entry, eatingTimeGenerator.next());
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
        if (s && s.group && s.group.students.Count <= 1)
            Destroy(s.group.gameObject);
        else if(s != null)
        {
            //Remove from group
            s.group.students.Remove(s);
            //Remove from list
            students.Remove(s);
            //Remove from map
            Destroy(s.gameObject);
        }
        return null;
    }

    //Create a new StudentGroup, register its next event
    //Register a next StudentGroup creationEvent
    public Event getAnotherGroup()
    {
        //add entry event
        float time = GlobalEventManager.currentTime + (float)(arrivalIntervalGenerator.next());
        //Debug.Log("Time: " + GlobalEventManager.currentTime + " Next Group Scheduled at: " + time);
        Event e = new Event(time, Event.EventType.CanteenArrival, this.getAnotherGroup,
             "Time: " + GlobalEventManager.currentTime + " Student Group Arrival");

        //Currently, for testing purpose only. This group has ENTERED NOW
        //TODO: Make student go loop
        StudentGroup group = addStudentGroup();
        if (group.type == StudentGroup.Type.FoodFirst)
        {
            foreach (Student s in group.students)
            {
                Student another = s;
                another.setPathTo(s.stallOfChoice.node, routeManagerScript);
                float arrivalTime = GlobalEventManager.currentTime + another.ETA(null);
                globalEventManager.addEvent(new Event(arrivalTime, Event.EventType.StallEnqueue, ()=> another.stallOfChoice.addStudent(another),
                    "Time: " + arrivalTime + " Student ID: " + another.ID + " has arrived at stall"));
            }
        } else
        {
            foreach (Student s in group.students)
            {
                Student another = s;
                globalEventManager.addEvent(tableManager.addTableSearchingStudent(another));
            }
        }
        /*
        Node destination = routeManagerScript.map_entries[1];
        foreach (Student s in group.students)
        {
            Student another = s;
            List<Node> path = routeManagerScript.getPath(another.prevNode, destination);
            another.setPositionAndRoute(another.prevNode.coordinates, path);
            Func<Event> delete = () => {
                return deleteStudent(another);
            };

            //Triggered when student finishes the walk
            float expectedExitTime = GlobalEventManager.currentTime + another.ETA(path.Last().coordinates);

            //Debug.Log("Time: " + GlobalEventManager.currentTime + " No. of Students: " + group.students.Count + " exiting at: " + expectedExitTime);
            routeManagerScript.Highlight(path);

            Event walk = new Event(expectedExitTime, Event.EventType.CanteenDeparture, delete,
                 "Time: " + GlobalEventManager.currentTime + " Student Reached a point to leave ");
            globalEventManager.addEvent(walk);
        }*/
        return null;
    }

    // Use this for initialization
    public void initialize(TableManager tableMan)
    {
        tableManager = tableMan;
        routeManagerScript = routeManager.GetComponent<RouteManager>();
        globalEventManager = eventManager.GetComponent<GlobalEventManager>();
        arrivalIntervalGenerator = new StudentEntry();
        eatingTimeGenerator = new EatingTime();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
