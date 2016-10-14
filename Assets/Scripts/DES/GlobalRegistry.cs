using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// In charge of 
/// 1) keeping track of time
/// 2) keeping track of future events
/// 3) kickstart events
/// 4) receiving logs about everything 
/// 
/// </summary>
public class GlobalRegistry
{
    public static double TIME { private set; get; }
    public static List<Event> events;


}
