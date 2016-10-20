using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class StudentGroup : MonoBehaviour
{
    //******************* System Wide Treaks *************************
    public static readonly double tableTakerRatio = GlobalConstants.TABLE_TAKER_RATIO;

    public readonly List<Student> students;
    public enum Type { TableFirst, FoodFirst };  //take a table first or go to stall first
    public Type type;

    //Initialize a new group of student
    public StudentGroup()
    {
        students = new List<Student>();
        //1. Determine Group Type.

        //2. Determine Student Choice of Stall

        //3. Generate Number of Student desired
    }

    //Called when anyone finished eating
    //If all done eating
    //Triggers leaving event
    public void notifyDoneEating(Student s)
    {

    }

    //Called when the group uses table first strategy
    //All students in group will proceed to next stage (Ordering)
    public void notifyFoundTable(Student s)
    {

    }
}
