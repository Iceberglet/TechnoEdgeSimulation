using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class StudentGroup : MonoBehaviour
{
    //******************* System Wide Treaks *************************
    public static readonly double tableTakerRatio = GlobalConstants.TABLE_TAKER_RATIO;

    public List<Student> students = new List<Student>();
    public enum Type { TableFirst, FoodFirst };  //take a table first or go to stall first
    public Type type;
}
