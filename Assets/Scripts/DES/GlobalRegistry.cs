using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    static Dictionary<long, StudentData> studentData;//time that a particular student starts to search for a table

    static List<float> time;
    static List<int> seatsUsed;
    static List<int> seatsReserved;

    public static int initialReserved = 0;
    public static int initialUsed = 0;
    public static int seated = 0;
    public static int reserved = 0;

    public static void initialize()
    {
        studentData = new Dictionary<long, StudentData>();
        time = new List<float>();
        seatsUsed = new List<int>();
        seatsReserved = new List<int>();
    }

    public static void initializeData()
    {
        time.Add(0);
        seatsUsed.Add(initialUsed);
        seatsReserved.Add(initialReserved);
        //Debug.Log("Taken: " + initialUsed + " Reserved: " + initialReserved);
    }

    public static void updateTableData(int deltaUsed, int deltaReserved)
    {
        if (GlobalEventManager.currentTime == 0f)
        {
            return;
        }
        time.Add(GlobalEventManager.currentTime);
        seated = seatsUsed.Last() + deltaUsed;
        reserved = seatsReserved.Last() + deltaReserved;
        seatsReserved.Add(reserved);
        seatsUsed.Add(seated);
    }

    //Output Data upon termination
    public static void output()
    {
        string config = "TAKER_" + GlobalConstants.TABLE_TAKER_RATIO.ToString("0.00") + "_SHARER_" + GlobalConstants.TABLE_SHARER_RATIO + "_";
        string path = Utility.Path + "/Output/";
        //Directory.CreateDirectory(Path.GetDirectoryName(path));
        System.Guid uniqueID = System.Guid.NewGuid();
        string pathStudent = path + "STUDE_DATA_" + config + uniqueID + ".csv";
        string pathTable = path + "TABLE_DATA_" + config + uniqueID + ".csv";

        //ID, System Time, Search Time
        var studentStringData = studentData.Select(s => s.Key + "," + s.Value.systemTime + "," + s.Value.searchTime).ToList();
        studentStringData.Insert(0, "ID,Time in System, Time in search");
        File.WriteAllLines(pathStudent, studentStringData.ToArray());

        List<string> tableStringData = Enumerable.Range(0, time.Count - 1)
            .Select(i =>
            {
                if (i > 0)
                {
                    float timeInterval = time.ElementAt(i) - time.ElementAt(i - 1);
                    int seatU = seatsUsed.ElementAt(i - 1);
                    int seatR = seatsReserved.ElementAt(i - 1);

                    float increU = seatU * timeInterval;
                    float increR = seatR * timeInterval;
                    return time.ElementAt(i) + "," + seatU + "," + seatR + "," + timeInterval + "," + increU + "," + increR;
                }
                else return "Time, Seats Used, Seats Taken, Delta_Time, Increment_Used, Increment_Reserved";
            }).ToList();
        File.WriteAllLines(pathTable, tableStringData.ToArray());
    }
    
    public static void signalStudent(Student s)
    {
        if (s.isDummy)
            return;
        float searchTime = (s.searchEnd - s.searchStart);
        float systemTime = s.leaveSystem - s.enterSystem;
        if (searchTime > 0)
        {
            Debug.Log("Student: " + s.ID + " has searched for " + searchTime);
        }
        studentData.Add(s.ID, new StudentData(searchTime, systemTime));
    }
}

//This is for all students
class StudentData
{
    public readonly float searchTime;
    public readonly float systemTime;

    public StudentData(float search, float system)
    {
        searchTime = search;
        systemTime = system;
    }
}

