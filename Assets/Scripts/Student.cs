using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Student : MonoBehaviour {


    public enum Stage { GetTable, GetFood, GoToTable, EatFood, Leave };
    //****************************************************************

    private StudentGroup group;
    private Stall stallOfChoice;
    private List<Stage> todo = new List<Stage>();
    public readonly string ID;

    public Student (string id)
    {
        ID = id;
    }

    /*
    public void proceedToNextStage()
    {
        Stage nextStage = todo[0];
        switch (nextStage)
        {
            case Stage.GetFood:    //Go To a stall and get food. When finished, go to next stage
                break;
            case Stage.GetTable:   //Random Walk in canteen. Does not finish until tableManager removes you and proceed to GoToTable mode
                break;
            case Stage.GoToTable:  //Slowly walks to the table being assigned
                break;
            case Stage.EatFood:    
                break;
            case Stage.Leave:
                break;
        }
        todo.RemoveAt(0);
    }*/


    
	// Use this for initialization
	void Start () {
        //TODO: the type should be initialized at the student group level.

	    //Assign a Stage
        if (this.group.type == StudentGroup.Type.TableFirst)
        {
            this.todo.Add(Stage.GetTable);
            this.todo.Add(Stage.GetFood);
        } else
        {
            this.todo.Add(Stage.GetFood);
            this.todo.Add(Stage.GetTable);
        }
        this.todo.Add(Stage.GoToTable);
        this.todo.Add(Stage.EatFood);
        this.todo.Add(Stage.Leave);

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
