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
    public int size;

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
                this.students.Add(s);
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
        if (students.Count == size)
            status = Status.Full;
        else status = Status.Half;
        return status;
    }

    public Status removeStudent(Student s)
    {
        students.Remove(s);
        if (students.Count == 0)
            status = Status.Empty;
        else status = Status.Half;
        return status;
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
