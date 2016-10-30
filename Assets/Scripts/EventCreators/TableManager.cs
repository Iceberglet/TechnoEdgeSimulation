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
    public List<Table> availableTables = new List<Table>();

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
            sscript.initializeTableManager(this);
        }
        
        //Add all tables
        //Debug.Log("Tables: " + r.tables.Count);
        Dictionary<Student, Table> st = new Dictionary<Student, Table>();
        int seats = 0;
        foreach (Table t in r.tables)
        {
            
            this.tables.Add(t);
            //Fill this table with eating students
            if (GlobalConstants.rand.NextDouble() < GlobalConstants.initialTablesTaken)
            {
                int numberOfStudentsAtTable = Math.Min(GlobalConstants.getStudentGroupSize(), t.size);
                for (int x = 0; x < numberOfStudentsAtTable; x++)
                {
                    Student student = studentManager.getDummyStudent(-1, t.node);
                    t.addStudent(student);
                    t.graphicAdd(student);
                    student.setPositionAndRoute(t.node.coordinates, null);
                    student.gameObject.layer = 8;
                    student.hasFood = true;
                    st.Add(student, t);
                }
                seats += numberOfStudentsAtTable;
                //Do not add to available ones
                if (numberOfStudentsAtTable == t.size)
                    continue;
            }
            this.availableTables.Add(t);
        }

        GlobalRegistry.initialUsed = seats;

        foreach (var kv in st)
        {
            Student ss = kv.Key;
            Table tt = kv.Value;
            eventManager.addEvent(new Event(ss.eatingTime, Event.EventType.TableDeparture, () => { return releaseStudent(ss, tt); }, "Finished Eating"));
        }
    }

    public void notifyGroupLeaveTable(Table t)
    {
        if (roamingStudents.Count > 0)
        {
            //Find the closest student and his group
            List<Student> s = roamingStudents
                //None sharer will solely look for an empty table. Thanks for asking!
                .Where(x => (x.group.isSharer ? t.availability() >= x.group.students.Count : t.students.Count == 0 && t.size >= x.group.students.Count))
                .OrderBy(stu => Coordinates.distGrid(stu.currentPos, t.node.coordinates)).ToList();
            if(s.Count > 0)
            {
                s = s.First().group.students;
                foreach (Student x in s)
                {
                    Student x1 = x;
                    x1.isRoaming = false;
                    //Debug.Log("**** Table Emptied, stop roaming: " + x1.ID);
                    t.addStudent(x1);
                    eventManager.addEvent(boundStudentToTable(x1, t));
                    roamingStudents.Remove(x1);
                    x1.searchEnd = GlobalEventManager.currentTime;
                }
                return;
            } 
        }
        availableTables.Add(t);
        return;
    }

    //When a student is GOING TO a table
    public Event boundStudentToTable(Student s, Table t)
    {
        if (t.status == Table.Status.Full)
            availableTables.Remove(t);
        s.setPathTo(t.node, routeManager);
        float time = s.ETA(null) + GlobalEventManager.currentTime;
        return new Event(time, Event.EventType.TableArrival, () => studentArrive(s, t),
            "Time: " + time + " Student ID: " + s.ID + " has arrived at his/her table");
    }

    //Event at the MOMENT student arrives
    //NOTE: somehow this is called twice for a single student at a point. Might wanna look into it
    public Event studentArrive(Student s, Table t)
    {
        if (s.hasFood)
        {
            //Sit and eat!
            if (s != null)
            {
                t.graphicAdd(s);
                float time = GlobalEventManager.currentTime + s.eatingTime;
                return new Event(time, Event.EventType.TableDeparture, () => releaseStudent(s, t),
                    "Time: " + time + " Student ID: " + s.ID + " has done eating.");
            }
            else return null;
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
        //Debug.Log(t.students.Count + " " + t.dummies.Count);
        s.finishedHisBusiness = true;
        if (s.group.students.All(student => student.finishedHisBusiness))
        {
            foreach (Student x in s.group.students)
            {
                Student ars = x;
                ars.setPathTo(findClosestExit(ars.currentPos), routeManager);    //Get the closest exit
                                                                                 //Hard coded debug
                t.removeStudent(ars);

                float time = ars.ETA(null) + GlobalEventManager.currentTime;
                eventManager.addEvent(new Event(time, Event.EventType.CanteenDeparture, () => studentManager.deleteStudent(ars),
                    "Time: " + time + " Student ID: " + ars.ID + " has left"));
            }
            notifyGroupLeaveTable(s.table);
        }
        return null;
    }

    //Add in a student who is BEGINNING to search for a Table
    public Event addTableSearchingStudent(Student s)
    {
        s.searchStart = s.searchEnd = GlobalEventManager.currentTime;
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

        //Check if there are available tables FIRST!
        if (availableTables.Count > 0)
        {
            //Select one that has enough seats OR EMPTY! 
            IEnumerable<Table> eligible = availableTables
                .Where(t => (s.group.isSharer? t.availability() >= s.group.students.Count : t.students.Count == 0 && t.size >= s.group.students.Count));
            if (eligible.Count() > 0)
            {
                //Is Closest to the student
                Table target = eligible.OrderBy(t => Coordinates.distGrid(s.currentPos, t.node.coordinates)).First();
                //Debug.Log("Current: " + target.students.Count + " Available: " + target.availability() + " to add: " + s.group.students.Count + " isSharer? " + s.group.isSharer);

                foreach (Student studentInGroup in s.group.students)
                {
                    studentInGroup.table = target;
                    target.addStudent(studentInGroup);
                    //Student s2 = studentInGroup;
                    //eventManager.addEvent(boundStudentToTable(s2, target));
                }
                return boundStudentToTable(s, target);
            }
        }
        //Debug.Log("No available table, start roaming.");
        if (roamingStudents.Count > 200)
            UIManager.updateImportantMessage("***** WARNING: TOO MANY TABLELESS STUDENT: " + roamingStudents.Count + " Stopping at 300 ******");
        if (roamingStudents.Count > 300)
        {
            eventManager.Stop();
        }

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
