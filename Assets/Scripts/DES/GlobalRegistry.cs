using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// In charge of 
/// 1) keeping track of time
/// 2) keeping track of future events
/// 3) kickstart events
/// 4) receiving logs about everything 
/// 
/// </summary>
public static class GlobalRegistry
{
    static Dictionary<long, float> studentSearchTime;//time that a particular student starts to search for a table
    static Dictionary<Table, TableData> tableData;

    public static void initialize(List<Table> ts)
    {
        studentSearchTime = new Dictionary<long, float>();
        tableData = new Dictionary<Table, TableData>();
    }

    public static void updateTableData(Table t)
    {
    }
    
    public static void signalStudent(Student s)
    {
        float runningTime = (s.searchEnd - s.searchStart);
        if (runningTime > 0)
        {
            Debug.Log("Student: " + s.ID + " has searched for " + runningTime);
        }
        studentSearchTime.Add(s.ID, runningTime);
    }
}

class TableData
{

}

