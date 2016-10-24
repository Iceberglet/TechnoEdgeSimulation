using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Table : MonoBehaviour {
  //2 or 4
    public Node node { get; private set; }  //The node at upper right
    public List<Node> corners;

    public enum Status { Empty, Half, Full};
    public Status status { get; private set; }
    public List<Student> students = new List<Student>(); //
    private List<GameObject> dummies = new List<GameObject>();
    public int size;

    public static float offset = 0.2f; //Used for graphics

    public void initialize(int s, List<Node> corners, List<Student> initialStudents = null)
    {
        this.size = s;
        this.corners = corners;
        this.node = corners.First();    //upper right
        if(initialStudents != null)
        {
            foreach (Student x in initialStudents)
                addStudent(x);
        }
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
        students.Remove(s);
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
            this.gameObject.GetComponent<SpriteRenderer>().color = GlobalConstants.allowTableSharing ? Color.blue : Color.red;
        }

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
                default: throw new System.Exception("Invalid operation. Table is full!");
            }
        }
        if (this.size == 2)
        {
            switch (this.dummies.Count + 1)
            {
                case 1: pos = new Coordinates(topRight.x - 0.5f, topRight.y - offset); break;
                case 2: pos = new Coordinates(topRight.x - 0.5f, topRight.y - 1 + offset); break;
                default: throw new System.Exception("Invalid operation. Table is full!");
            }
        }
        s.gameObject.layer = 8;
        GameObject dummy = Instantiate(StudentManager.accessibleStudentTemplate);
        dummy.transform.position = new Vector3(pos.x, pos.y, GlobalConstants.Z_TABLE_STATIC);
        dummies.Add(dummy);
    }

    public void graphicRemove(Student s)
    {
        //s.setPositionAndRoute(this.node.coordinates, null);
        s.gameObject.layer = 0;
        GameObject todestroy = dummies.First();
        dummies.Remove(todestroy);
        Destroy(todestroy);
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
