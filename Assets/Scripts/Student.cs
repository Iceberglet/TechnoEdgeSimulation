using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Student : MonoBehaviour {
    //******************* For Recording Purposes *********************
    public float searchStart = -1f;
    public float searchEnd = -1f;
    public float enterSystem = -1f;
    public float leaveSystem = -1f;



    //****************************************************************

    public StudentGroup group { get; private set; }
    public Stall stallOfChoice { get; private set; }
    public float eatingTime { get; private set; }
    //private List<Stage> todo = new List<Stage>();
    public long ID;
    private static long counter = 0;

    //*** Position Related ***
    public Coordinates currentPos { get; private set; }
    private Coordinates t;
    public Node prevNode { get; private set; }
    public Node nextNode { get; private set; }
    private List<Node> route;

    public bool isRoaming = false;  //Used by TableManager to decide whether to continue roaming or exit when roamingDoneEvent happens
    public bool hasFood = false;    //Used to decide whether student stays to eat
    public Table table = null;
    public bool finishedHisBusiness = false;

    public void advanceFor(float seconds)
    {
        if (route == null || route.Count < 1)
            return;
        float dist = seconds * GlobalConstants.WALK_SPEED;
        while(route.Count > 0 && dist > 0)
        {
            nextNode = route.First();
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
                prevNode = route.First();   //Remember where is the node you coming from
                route.RemoveAt(0);
                dist -= toNext;
            }
        }
        setPositionInUnity();
    }

    public void setPathTo(Node target, RouteManager man)
    {
        Node start1 = prevNode;
        Node start2 = nextNode;
        List<Node> path;
        if (Coordinates.distGrid(prevNode.coordinates, target.coordinates) < Coordinates.distGrid(nextNode.coordinates, target.coordinates))
            path = man.getPath(prevNode, target);
        else path = man.getPath(nextNode, target);
        setPositionAndRoute(currentPos, path);
    }

    public void setPositionAndRoute(Coordinates pos, List<Node> r)
    {
        currentPos = pos;
        route = r;
        t = (r != null && r.Count > 0) ? r.Last().coordinates : pos;
    }

    public float ETA(Coordinates target)
    {
        if (target == null)
            target = t;
        return Coordinates.distGrid(currentPos, target) / GlobalConstants.WALK_SPEED;
    }

    private void setPositionInUnity()
    {
        this.transform.position = new Vector3(currentPos.x, currentPos.y, GlobalConstants.Z_MOVING_OBJ);
    }

    public void initialize(Stall s, StudentGroup g, Node start, float eatingTime)
    {
        ID = counter++;
        this.stallOfChoice = s;
        group = g;
        prevNode = start;
        nextNode = start;
        currentPos = start.coordinates;
        this.eatingTime = eatingTime;
    }
}
