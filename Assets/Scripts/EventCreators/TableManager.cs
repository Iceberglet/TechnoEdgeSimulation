using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    private GlobalEventManager eventManager;
    private RouteManager routeManager;
    private StudentManager studentManager;
    private List<Table> tables = new List<Table>();
    private List<Table> availableTables = new List<Table>();

    private List<Node> mainLoopNodes;
    private List<Student> roamingStudents = new List<Student>();

    public void initialize(RouteManager r, GlobalEventManager eventManager, StudentManager s)
    {
        routeManager = r;
        mainLoopNodes = r.mainLoopNodes;
        this.eventManager = eventManager;
        studentManager = s;
        foreach(GameObject ss in r.map_stalls)
        {
            Stall sscript = ss.GetComponent<Stall>();
            sscript.initializeTableManager(eventManager, this);
        }

        //Add all tables
        Debug.Log("Tables: " + r.tables.Count);
        foreach (Table t in r.tables)
        {
            this.tables.Add(t);
            this.availableTables.Add(t);
        }
    }

    public Event notifyTableEmptied(Table t)
    {
        if (roamingStudents.Count > 0)
        {
            //Find the closest student and his group
            StudentGroup s = roamingStudents.Where(x => (t.availability() >= x.group.students.Count))
                .OrderBy(stu => Coordinates.distGrid(stu.currentPos, t.node.coordinates)).First().group;
            foreach(Student x in s.students)
            {
                Student x1 = x;
                x1.isRoaming = false;
                eventManager.addEvent(boundStudentToTable(x1, t));
            }
            return null;
        }
        else
        {
            availableTables.Add(t);
            return null;
        }
    }

    //When a student is GOING TO a table
    public Event boundStudentToTable(Student s, Table t)
    {
        //s.table = t;
        t.addStudent(s);
        s.setPathTo(t.node, routeManager);
        float time = s.ETA(null) + GlobalEventManager.currentTime;
        return new Event(time, Event.EventType.TableArrival, () => studentArrive(s, t),
            "Time: " + time + " Student ID: " + s.ID + " has arrived at his/her table");
    }

    //Event at the MOMENT student arrives
    public Event studentArrive(Student s, Table t)
    {
        if (s.hasFood)
        {
            //Sit and eat!
            float time = GlobalEventManager.currentTime + s.eatingTime;
            return new Event(time, Event.EventType.TableDeparture, ()=>releaseStudent(s, t),
                "Time: " + time + " Student ID: " + s.ID + " has done eating.");
        } else
        {
            //Send to stall
            s.table = t;
            s.setPathTo(s.stallOfChoice.node, routeManager);
            float time = s.ETA(null) + GlobalEventManager.currentTime;
            return new Event(time, Event.EventType.StallEnqueue, () => s.stallOfChoice.addStudent(s),
                "Time: " + time + " Student ID: " + s.ID + " has arrived at stall: " + s.stallOfChoice.name);
        }
    }

    //Student READY to its entry point
    public Event releaseStudent(Student s, Table t)
    {
        //TODO
        s.finishedHisBusiness = true;
        if(s.group.students.All(student=>student.finishedHisBusiness))
            foreach(Student x in s.group.students)
            {
                Student ars = x;
                ars.setPathTo(findClosestExit(ars.currentPos), routeManager);    //Get the closest exit
                t.removeStudent(ars);
                float time = ars.ETA(null) + GlobalEventManager.currentTime;
                eventManager.addEvent(new Event(time, Event.EventType.CanteenDeparture, () => studentManager.deleteStudent(ars),
                    "Time: " + time + " Student ID: " + ars.ID + " has left"));
            }
        return null;
    }

    //Add in a student who is BEGINNING to search for a Table
    public Event addTableSearchingStudent(Student s)
    {
        //This is a student whose friends have already got him a table
        if (s.table != null)
        {
            return boundStudentToTable(s, s.table);
        }

        //Check if any of his friends have a table already
        List<Student> friendsWithTable = s.group.students.Where(xx => xx.table != null).ToList();
        if (friendsWithTable.Count > 0)
        {
            return boundStudentToTable(s, friendsWithTable.First().table);
        }

        //TODO: check if there are available tables FIRST!
        if (availableTables.Count > 0)
        {
            //Select one that has enough seats and 
            Table target = availableTables.Where(t => (t.availability() >= s.group.students.Count))
                .OrderBy(t => Coordinates.distGrid(s.currentPos, t.node.coordinates)).First();
            foreach (Student studentInGroup in s.group.students)
            {
                studentInGroup.table = target;
                //Student s2 = studentInGroup;
                //eventManager.addEvent(boundStudentToTable(s2, target));
            }
            return boundStudentToTable(s, target);
        }
        Debug.Log("No available table, start roaming.");
        s.isRoaming = true;
        roamingStudents.Add(s);
        return startRoaming(s);
    }

    public Event startRoaming(Student s)
    {
        //If we need to continue roaming
        if (s.isRoaming)
        {
            //Get to the next roamingNode
            Node target = mainLoopNodes.OrderBy(n => Coordinates.distGrid(s.currentPos, n.coordinates)).ElementAt(GlobalConstants.rand.Next(1,6));
            s.setPathTo(target, routeManager);
            float time = GlobalEventManager.currentTime + s.ETA(target.coordinates);

            //Return a new RoamingEvent
            return new Event(time, Event.EventType.RoamToPoint, ()=>startRoaming(s), "Student: " + s.ID + " Finished another segment of roaming");
        }
        else return null;
    }
    
    public Node findClosestMainloop(Coordinates from)
    {
        Node best = mainLoopNodes.OrderBy(n => Coordinates.distGrid(n.coordinates, from)).First();
        return best;
    }

    public Node findClosestExit(Coordinates from)
    {
        Node best = routeManager.map_entries.OrderBy(n => Coordinates.distGrid(n.coordinates, from)).First();
        return best;
    }
}
