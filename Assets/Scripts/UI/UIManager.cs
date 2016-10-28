using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour {
    public GlobalEventManager globalEventManager;

    //**** SPEED ****
    public GameObject[] speedButtons = new GameObject[6];   //stop, pause, *1, *10, *100, *1000
    private Button[] buttons = new Button[6];       
    //private int currentSpeed;   //0, 1, 2, 3, 4, 5
    private int[] speedValues = new int[] { -1, 0, 1, 10, 80, 200 };

    //**** INPUTS ****
    public InputField seedInput;
    public InputField tableTaker;
    public InputField tableSharer;
    private int seed;
    private float takerRatio;
    private float sharerRatio;
    public Button updateConfig;

    //**** IMPORTANT MESSAGING ****
    private static Text importantText;
    public Text bottomText;
    private static Text eventText;
    private static List<string> eventTexts;
    public Text leftText;


	// Use this for initialization
	void Start () {
        //Initialize Speed Buttons
	    for(int i = 0; i < buttons.Length; i++)
        {
            int idx = i;
            buttons[idx] = speedButtons[idx].GetComponent<Button>();
            buttons[idx].interactable = false;
            buttons[idx].onClick.AddListener(()=> { this.onClickButton(idx); } );

            if(i >= 2)
                buttons[i].interactable = true; //can hit play buttons
        }

        //Initializa configs
        seed = GlobalConstants.RANDOM_SEED;
        seedInput.text = seed.ToString();
        seedInput.onEndEdit.AddListener((s) => { this.onUpdateSeed(s); });

        takerRatio = GlobalConstants.TABLE_TAKER_RATIO;
        tableTaker.text = takerRatio.ToString();
        tableTaker.onEndEdit.AddListener((v) => { this.onUpdateTableTaker(v);});

        sharerRatio = GlobalConstants.TABLE_SHARER_RATIO;
        tableSharer.text = sharerRatio.ToString();
        tableSharer.onEndEdit.AddListener((v) => { this.onUpdateTableSharer(v); });

        updateConfig.onClick.AddListener(() => {
        //Update Static Configs
        GlobalConstants.updateConfig(this.seed, this.takerRatio, this.sharerRatio);
            updateImportantMessage("Updated Initial Config");
        });

        eventText = leftText;
        eventTexts = new List<string>();

        importantText = bottomText;
    }

    private void onUpdateTableTaker(string v)
    {
        float temp = float.Parse(v);
        if(temp >= 0 && temp <= 1)
        {
            this.takerRatio = temp;
        } else
        {
            updateImportantMessage("Invalid Table Taker Ratio: " + temp);
        }
    }

    private void onUpdateTableSharer(string v)
    {
        float temp = float.Parse(v);
        if (temp >= 0 && temp <= 1)
        {
            this.sharerRatio = temp;
        }
        else
        {
            updateImportantMessage("Invalid Table Sharer Ratio: " + temp);
        }
    }

    private void onUpdateSeed(string s)
    {
        this.seed = int.Parse(s);
        updateImportantMessage("Seed changed to: " + seed);
    }

    private void onClickButton(int i)
    {
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

    public static void updateEventText(string s)
    {
        eventTexts.Insert(0, s);
        if (eventTexts.Count > 10)
            eventTexts.Remove(eventTexts.Last());
        eventText.text = string.Join("\n", eventTexts.ToArray());
    }

    public static void updateImportantMessage(string s)
    {
        importantText.text = s;
    }
}
