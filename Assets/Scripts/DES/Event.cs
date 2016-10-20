using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Event
{
    public enum EventType {
        CanteenArrival,
        StallEnqueue,       //Joins the queue
        StallDequeue,
        TableArrival,       //Both for TableSeeking AND Before Dining
        TableDeparture,
        CanteenDeparture,
    }

    public readonly float timeStamp;
    public readonly EventType type;
    public readonly string msg;
    public readonly Func<Event> execute;

    public Event(float stamp, EventType type, Func<Event> f, string msg)
    {
        this.timeStamp = stamp;
        this.type = type;
        this.execute = f;
        this.msg = msg;
    }
    /*
    //Returns an event to be generated
    public Event execute()
    {
        switch (this.type)
        {
            case EventType.CanteenArrival:
                //Next Event: CanteenArrival - By: StudentGenerator
                //Processing: Create a new Student
                break;

            case EventType.StallEnqueue:
                //Does not generate another event
                //Stall takes into queue
                break;

            case EventType.StallDequeue:
                break;

            case EventType.TableArrival:
                break;

            case EventType.TableDeparture:
                break;

            case EventType.CanteenDeparture:
                break;
        }

        return null;
    }*/
}
