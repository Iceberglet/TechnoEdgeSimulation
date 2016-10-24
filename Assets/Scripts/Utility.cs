using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Utility
{
    public static string Path
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return Application.dataPath + "/../";
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                return Application.dataPath + "/../../";
            }
            else return Application.dataPath;
        }
    }

    public static bool checkFloatEqual(float a, float b, float epsilon = 0.0001f)
    {
        float absA = Math.Abs(a);
        float absB = Math.Abs(b);
        float diff = Math.Abs(a - b);

        if (a == b)
        { // shortcut, handles infinities
            return true;
        }
        /*
        else if (a == 0 || b == 0 || diff < Double.Epsilon)
        {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < (epsilon * Double.Epsilon);
        }*/
        else
        { // use relative error
            return diff / (absA + absB) < epsilon;
        }
    }
}
