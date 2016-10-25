using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {
    public GlobalEventManager globalEventManager;
    public GameObject[] speedButtons = new GameObject[6];   //stop, pause, *1, *10, *100, *1000
    private Button[] buttons = new Button[6];               
    //private int currentSpeed;   //0, 1, 2, 3, 4, 5
    private int[] speedValues = new int[] { -1, 0, 1, 10, 80, 200 };

	// Use this for initialization
	void Start () {
	    for(int i = 0; i < buttons.Length; i++)
        {
            int idx = i;
            buttons[idx] = speedButtons[idx].GetComponent<Button>();
            buttons[idx].interactable = false;
            buttons[idx].onClick.AddListener(()=> { this.onClickButton(idx); } );

            if(i >= 2)
                buttons[i].interactable = true; //can hit play buttons
        }
	}
    
    private void onClickButton(int i)
    {
        Debug.Log("Clicked Button: " + i);
        //************ Now action according to new speed **************
        switch (i)
        {
            //Stopping
            case 0: globalEventManager.Stop();
                updateAllButtons(false);
                buttons[2].interactable = true;
                break;
            //Pause or any speed
            default:
                updateAllButtons(true);
                buttons[i].interactable = false;
                globalEventManager.ChangeSpeed(speedValues[i]);
                break;
        }
        return;
    }

    private void updateAllButtons(bool enable)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = enable;
        }
    }


	
	// Update is called once per frame
	void Update () {
	
	}
}
