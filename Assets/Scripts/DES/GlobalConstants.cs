using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GlobalConstants
{
    public static readonly float TABLE_TAKER_RATIO = 0.99f;
    public static readonly float TABLE_SHARER_RATIO = 0.5f;  //Ratio of people willing to share a table

    public static readonly float Z_BOTTOM_NETWORK_1 = 10f;
    public static readonly float Z_BOTTOM_NETWORK_2 = 9.9f;
    public static readonly float Z_BOTTOM_NETWORK_3 = 9.8f;
    public static readonly float Z_BOTTOM_STATIC = 9f;
    public static readonly float Z_TABLE_STATIC = 6f;
    public static readonly float Z_MOVING_OBJ = 5f;

    public static readonly float WALK_SPEED = 0.7f;
    //Unit per second

    public static readonly bool allowTableSharing = true;



    private static readonly float[] STALL_PROBA = new float[10]
    {
        0.1f, 0.2f, 0.3f, 0.4f, 0.5f,
        0.6f, 0.7f, 0.8f, 0.9f, 1f
    };
    public static readonly int[] STALL_SERVER = new int[10]
    {
        1, 1, 1, 1, 1,
        1, 1, 1, 1, 1
    };
    private static readonly float[] ENTRY_PROBA = new float[4]
    {
        0.25f, 0.36f, 0.96f, 1
    };
    private static readonly float[] GROUPSIZE_PROBA = new float[4]
    {
        0.6f, 0.88f, 0.95f, 1f
    };
    public static readonly string[] STALL_IDS = new string[10]
    {
        "INDO", "INDI", "MINC", "VEGE", "TOFU",
        "WOK_", "JAP_", "CHIC", "WEST", "CHIN"
    };

    public static Random rand = new Random();

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

