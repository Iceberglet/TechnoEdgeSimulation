using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class GlobalConstants
{
    public static int RANDOM_SEED = 9;
    public static float TABLE_TAKER_RATIO = 0f;
    public static float TABLE_SHARER_RATIO = 0.5f;

    public static void updateConfig(int seed, float taker, float sharer)
    {
        RANDOM_SEED = seed;
        TABLE_SHARER_RATIO = sharer;
        TABLE_TAKER_RATIO = taker;
    }

    public static readonly float SIMULATION_TIME = 2 * 60 * 60;
  //Ratio of people willing to share a table

    public static readonly float Z_BOTTOM_NETWORK_1 = 10f;
    public static readonly float Z_BOTTOM_NETWORK_2 = 9.9f;
    public static readonly float Z_BOTTOM_NETWORK_3 = 9.8f;
    public static readonly float Z_BOTTOM_STATIC = 9f;
    public static readonly float Z_TABLE_STATIC = 6f;
    public static readonly float Z_MOVING_OBJ = 5f;

    public static readonly float WALK_SPEED = 0.7f;
    //Unit per second

    //public static readonly bool allowTableSharing = true;

    public static readonly IntervalGenerator[] STALL_SERVICE_INTERVALS = new IntervalGenerator[10]
    {
        GenericDistribution.createInstanceFromFile("Indonesia.csv"),
        GenericDistribution.createInstanceFromFile("India.csv"),
        GenericDistribution.createInstanceFromFile("Minced meat noodle.csv"),
        GenericDistribution.createInstanceFromFile("Vegetarian.csv"),
        GenericDistribution.createInstanceFromFile("Yong Tau Foo.csv"),
        GenericDistribution.createInstanceFromFile("Wok Soup.csv"),
        GenericDistribution.createInstanceFromFile("Japanese.csv"),
        GenericDistribution.createInstanceFromFile("Chicken Rice.csv"),
        GenericDistribution.createInstanceFromFile("Western.csv"),
        GenericDistribution.createInstanceFromFile("Mixed rice.csv")
    };

    public static float[] STALL_PROBA = new float[10]
    {
        0.144208038f,
        0.236406619f,
        0.319148936f,
        0.38534279f,
        0.460992908f,
        0.498817967f,
        0.572104019f,
        0.718676123f,
        0.841607565f,
        1,
    };
    public static readonly int[] STALL_SERVER = new int[10]
    {
        1, 1, 1, 1, 1,
        1, 1, 1, 1, 1
    };
    private static readonly float[] ENTRY_PROBA = new float[3]
    {
        0.18f, 0.27f, 1
    };
    private static readonly float[] GROUPSIZE_PROBA = new float[4]
    {
        0.6793f,
        0.8586f,
        0.9564f,
        1
    };
    public static readonly string[] STALL_IDS = new string[10]
    {
        "INDO", "INDI", "MINC", "VEGE", "TOFU",
        "WOK_", "JAP_", "CHIC", "WEST", "CHIN"
    };

    public static System.Random rand = new System.Random(GlobalConstants.RANDOM_SEED);


    public static float initialTablesTaken;
    public static int[] initialStallQueues = new int[10];
    public static IntervalGenerator initialEatingTimeGenerator;
    public static void loadInitialCondition()
    {
        string path = Utility.Path + "/Input/initialConditionTablesAndStalls.txt";

        String[] rows = File.ReadAllText(path).Split('\n');

        for (int i = 0; i < rows.Length; i++)
        {
            String[] r = rows[i].Trim().Split(',');
            if (i > 0)
            {
                initialStallQueues[i - 1] = int.Parse(r[0]);
            } else
            {
                initialTablesTaken = float.Parse(r[0]);
            }
        }
        initialEatingTimeGenerator = GenericDistribution.createInstanceFromFile("initialConditionEatingTime.csv");



        path = Utility.Path + "/Input/StallPreferences.csv";
        rows = File.ReadAllText(path).Split('\n');
        float[] temp = new float[10];
        float[] cumu = new float[10];
        float sum = 0;

        for (int i = 0; i < 10; i++)
        {
            String[] r = rows[i].Trim().Split(',');
            temp[i] = float.Parse(r[0]);
            sum += temp[i];
            cumu[i] = sum;
        }
        STALL_PROBA = cumu.Select(number => number / sum).ToArray();
    }
    
    public static string[] entryDistributionFileNames = new string[3]
    {
        "entrance3 ",
        "entrance1 ",
        "entrance2 "
    };


    public static int getStudentGroupSize()
    {
        return getIdx(GROUPSIZE_PROBA) + 1;
    }
    public static int getStallChoice()
    {
        return getIdx(STALL_PROBA);
    }
    public static int getEntry()
    {
        return getIdx(ENTRY_PROBA);
    }

    private static int getIdx(float[] cumu)
    {
        float p = (float)rand.NextDouble();
        for(int i = 0; i < cumu.Length; ++i)
        {
            if (p <= cumu[i])
                return i;
        }
        return cumu.Length - 1;
    }
}

