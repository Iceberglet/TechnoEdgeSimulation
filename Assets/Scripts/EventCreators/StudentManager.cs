using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;

public class StudentManager : MonoBehaviour {
    //Dependency
    public RouteManager routeManagerScript;
    public GlobalEventManager globalEventManager;
    public TableManager tableManager;

    public GameObject studentTemplate;
    public static GameObject accessibleStudentTemplate;
    public GameObject studentGroupTemplate;
    private List<Student> students = new List<Student>();   //Needed for collective movement
    private IntervalGenerator[] arrivalIntervalGenerators = new IntervalGenerator[3];
    private IntervalGenerator eatingTimeGenerator;
    private System.Random rand;

    public static int NumberOfPeopleInSystem = 0;

    //Add in terms of group, Delete in terms of individual
    public StudentGroup addStudentGroup(int entryIdx)
    {
        GameObject groupObj = Instantiate(studentGroupTemplate);
        StudentGroup groupScript = groupObj.GetComponent<StudentGroup>();
        Node entry = routeManagerScript.map_entries[entryIdx];
        int number = GlobalConstants.getStudentGroupSize();
        //1. Determine Group Type.
        StudentGroup.Type type = rand.NextDouble() < GlobalConstants.TABLE_TAKER_RATIO ? StudentGroup.Type.TableFirst : StudentGroup.Type.FoodFirst;
        groupScript.isSharer = rand.NextDouble() < GlobalConstants.TABLE_SHARER_RATIO ? true : false;
        groupScript.type = type;
        for (int i = 0; i < number; i++)
        {
            //2. Determine Student Choice of Stall
            int stall = GlobalConstants.getStallChoice();
            GameObject newStudent = Instantiate(studentTemplate);
            Student s = newStudent.GetComponent<Student>();
            students.Add(s);
            float eatingTime = eatingTimeGenerator.next();
            s.initialize(routeManagerScript.map_stalls[stall].GetComponent<Stall>(), groupScript, entry, eatingTime);
            s.enterSystem = GlobalEventManager.currentTime;
            //3. Add to group
            groupScript.students.Add(s);
            s.transform.parent = groupObj.transform;
            NumberOfPeopleInSystem++;
        }
        groupObj.transform.parent = this.transform;
        return groupScript;
    }

    public Student getDummyStudent(int stallIdx, Node start)
    {
        NumberOfPeopleInSystem++;
        if (eatingTimeGenerator == null)
        {
            eatingTimeGenerator = GenericDistribution.createInstanceFromFile("eating time.csv");
        }
        GameObject newStudent = Instantiate(studentTemplate);
        Student student = newStudent.GetComponent<Student>();
        StudentGroup group = Instantiate(studentGroupTemplate).GetComponent<StudentGroup>();
        group.students.Add(student);
        student.transform.parent = group.transform;

        if(stallIdx > -1)
        {
            Stall stall = routeManagerScript.map_stalls[stallIdx].GetComponent<Stall>();
            student.initialize(stall, group, stall.node, eatingTimeGenerator.next());
            //Get him a table if he is a taker
            if (GlobalConstants.rand.NextDouble() < GlobalConstants.TABLE_TAKER_RATIO)
            {
                var tables = tableManager.availableTables.Where(t => t.students.Count == 0).ToList();
                if(tables.Count > 0)
                {
                    Table table = tables.ElementAt(GlobalConstants.rand.Next(0, tables.Count));
                    table.addStudent(student);
                    GlobalRegistry.initialReserved++;
                }
            }
        } else
        {
            student.initialize(null, group, start, GlobalConstants.initialEatingTimeGenerator.next());
        }
        student.enterSystem = GlobalEventManager.currentTime;
        student.isDummy = true;
        student.setPositionAndRoute(start.coordinates, null);
        students.Add(student);
        return student;
    }

    public void advanceAllStudents(float seconds)
    {
        foreach (Student s in students)
            s.advanceFor(seconds);
    }

    public Event deleteStudent(Student s)
    {
        NumberOfPeopleInSystem--;
        if (s != null)
        {
            s.leaveSystem = GlobalEventManager.currentTime;
            GlobalRegistry.signalStudent(s);
            StudentGroup group = s.group;
            //Remove from group
            s.group.students.Remove(s);
            //Remove from list
            students.Remove(s);
            //Remove from map
            Destroy(s.gameObject);
            if (group.students.Count == 0)
                Destroy(group.gameObject);
        }
        return null;
    }

    //Create a new StudentGroup, register its next event
    //Register a next StudentGroup creationEvent
    public Event getAnotherGroup(int entryIdx)
    {
        //add entry event
        float time = GlobalEventManager.currentTime + (float)(arrivalIntervalGenerators[entryIdx].next());
        //Debug.Log("Time: " + GlobalEventManager.currentTime + " Next Group Scheduled at: " + time);
        Event e = new Event(time, Event.EventType.CanteenArrival, ()=>this.getAnotherGroup(entryIdx),
             "Time: " + GlobalEventManager.currentTime + " Student Group Arrival");
        
        StudentGroup group = addStudentGroup(entryIdx);
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
        return e;
    }

    // Use this for initialization
    public void initialize()
    {
        rand = new System.Random(GlobalConstants.RANDOM_SEED);
        accessibleStudentTemplate = Instantiate(studentTemplate);
        accessibleStudentTemplate.GetComponent<SpriteRenderer>().color = Color.white;
        accessibleStudentTemplate.transform.position = new Vector3(99, 99, -99);
        eatingTimeGenerator = GenericDistribution.createInstanceFromFile("eating time.csv");

        //Initialize Arrival Generator and add events to change them
        updateGenerators("1200-1230.csv");
        globalEventManager.addEvent(new Event(60 * 30, Event.EventType.UpdateStudentGenerator, () => updateGenerators("1230-1300.csv"), "Updated Entry Interval"));
        globalEventManager.addEvent(new Event(60 * 60, Event.EventType.UpdateStudentGenerator, () => updateGenerators("1330-1400.csv"), "Updated Entry Interval"));
    }

    private Event updateGenerators(string timeAppendon)
    {
        for(int i = 0; i < GlobalConstants.entryDistributionFileNames.Length; ++i)
        {
            //Must be in input
            arrivalIntervalGenerators[i] = GenericDistribution.createInstanceFromFile(GlobalConstants.entryDistributionFileNames[i] + timeAppendon);
        }
        return null;
    }
}
