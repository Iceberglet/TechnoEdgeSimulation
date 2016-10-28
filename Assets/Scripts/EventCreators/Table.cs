using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Table : MonoBehaviour {

    //********************* For Data Collection Purposes ***********
    private float recordingSince;
    private int latestSeated;
    private int latestReserved;

    public float[] utility; //utility[i] is the time spent with i people ACTUALLY SITTING THERE
    public float[] disutility;  //disutility[i] is the time spent with i people RESERVED A PLACE HERE

    private void update(float time)
    {
        //utility[latestSeated] += latestSeated * (time - recordingSince);
        //disutility[latestReserved] += latestReserved * (time - recordingSince);
        int newSeated = dummies.Count;
        int newReserved = Math.Max(students.Count - dummies.Count, 0);
        GlobalRegistry.updateTableData(newSeated - latestSeated, newReserved - latestReserved);
        latestReserved = newReserved;
        latestSeated = newSeated;
        recordingSince = time;
    }

    //2 or 4
    public Node node { get; private set; }  //The node at upper right
    public List<Node> corners;

    public enum Status { Empty, Half, Full};
    public Status status { get; private set; }
    public List<Student> students = new List<Student>(); //
    public List<GameObject> dummies = new List<GameObject>();
    public int size;

    public static float offset = 0.2f; //Used for graphics

    public void initialize(int s, List<Node> corners, List<Student> initialStudents = null)
    {
        this.size = s;
        this.corners = corners;
        this.node = corners.First();    //upper right
        recordingSince = 0;
        latestReserved = 0;
        latestSeated = 0;
        if (initialStudents != null)
        {
            latestSeated = initialStudents.Count;
            foreach (Student x in initialStudents)
                addStudent(x);
        }
        utility = new float[size + 1];
        disutility = new float[size + 1];
    }

    public Status addStudent(Student s)
    {
        if (!students.Contains(s))
        {
            if (this.students.Count < this.size)
            {
                this.students.Add(s);
            }
            else
            {
                string errorMsg = "Error: Trying to add student to a full table\n";
                errorMsg += "Students at table: ";
                foreach(Student ss in students)
                {
                    errorMsg += " " + ss.ID;
                }
                errorMsg += "\nAttempted to add Student: " + s.ID;
                throw new System.Exception(errorMsg);
            }
        }
        s.table = this;
        return updateStatus();
    }

    public Status removeStudent(Student s)
    {
        int OriginalStudentCount = students.Count;
        int OriginalDummiesCount = dummies.Count;
        students.Remove(s);
        graphicRemove(s);

        int removedStudent = students.Count - OriginalStudentCount;
        int removedDummiesCount = dummies.Count - OriginalDummiesCount;
        if(removedStudent != removedDummiesCount)
        {
            throw new Exception(removedStudent + " " + removedDummiesCount);
        }

        return updateStatus();
    }

    private Status updateStatus()
    {
        if (students.Count == size)
        {
            status = Status.Full;
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if (students.Count == 0)
        {
            status = Status.Empty;
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            status = Status.Half;
            this.gameObject.GetComponent<SpriteRenderer>().color = GlobalConstants.TABLE_SHARER_RATIO > 0 ? Color.blue : Color.red;
        }
        update(GlobalEventManager.currentTime);
        return status;
    }

    public void graphicAdd(Student s)
    {
        Coordinates topRight = this.node.coordinates;
        Coordinates pos = null;    //For this student

        if (this.size == 4)
        {
            switch (this.dummies.Count + 1)
            {
                case 1: pos = new Coordinates(topRight.x - offset, topRight.y - offset); break;
                case 2: pos = new Coordinates(topRight.x - offset, topRight.y - 1 + offset); break;
                case 3: pos = new Coordinates(topRight.x - 1 + offset, topRight.y - 1 + offset); break;
                case 4: pos = new Coordinates(topRight.x - 1 + offset, topRight.y - offset); break;
                default:  return; // throw new System.Exception("Invalid operation. Table is full!");
            }
        }
        if (this.size == 2)
        {
            switch (this.dummies.Count + 1)
            {
                case 1: pos = new Coordinates(topRight.x - 0.5f, topRight.y - offset); break;
                case 2: pos = new Coordinates(topRight.x - 0.5f, topRight.y - 1 + offset); break;
                default: return; // throw new System.Exception("Invalid operation. Table is full!");
            }
        }
        if(s == null)
        {
            throw new System.Exception("Shit why u gimme a null?");
        }
        s.gameObject.layer = 8;
        GameObject dummy = Instantiate(StudentManager.accessibleStudentTemplate);
        dummy.transform.position = new Vector3(pos.x, pos.y, GlobalConstants.Z_TABLE_STATIC);
        dummies.Add(dummy);
        dummy.transform.parent = this.transform;
        update(GlobalEventManager.currentTime);
    }

    public void graphicRemove(Student s)
    {
        //s.setPositionAndRoute(this.node.coordinates, null);
        s.gameObject.layer = 0;
        if(dummies.Count > 0)
        {
            GameObject todestroy = dummies.First();
            dummies.Remove(todestroy);
            Destroy(todestroy);
        }
        update(GlobalEventManager.currentTime);
    }

    public int availability()
    {
        return size - students.Count();
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
