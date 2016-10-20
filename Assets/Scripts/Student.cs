using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Student : MonoBehaviour {
    public enum Stage { GetTable, GetFood, GoToTable, EatFood, Leave };
    //****************************************************************

    public StudentGroup group { get; private set; }
    private Stall stallOfChoice;
    private List<Stage> todo = new List<Stage>();
    public long ID;
    private static long counter = 0;

    //*** Position Related ***
    public Coordinates currentPos { get; private set; }
    public Node prevNode { get; private set; }
    private List<Node> route;

    public void advanceFor(float seconds)
    {
        if (route == null || route.Count < 1)
            return;
        float dist = seconds * GlobalConstants.WALK_SPEED;
        while(route.Count > 0 && dist > 0)
        {
            Coordinates next = route.First().coordinates;
            float toNext = Coordinates.distGrid(next, currentPos);
            if (dist < toNext){
                currentPos = new Coordinates(
                    currentPos.x + (next.x - currentPos.x) * dist / toNext,
                    currentPos.y + (next.y - currentPos.y) * dist / toNext
                );
                dist = 0;
            } else
            {
                currentPos = next;
                route.RemoveAt(0);
                dist -= toNext;
            }
        }
    }

    public void setPositionAndRoute(Coordinates pos, List<Node> r)
    {
        currentPos = pos;
        route = r;
    }

    public void setPositionInUnity()
    {
        this.transform.position = new Vector3(currentPos.x, currentPos.y, GlobalConstants.Z_MOVING_OBJ);
    }

    public void initialize(Stall s, StudentGroup g, Node start)
    {
        ID = counter++;
        stallOfChoice = s;
        group = g;
        prevNode = start;
        currentPos = start.coordinates;
    }
}
