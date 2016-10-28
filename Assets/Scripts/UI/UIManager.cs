using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {
    public GlobalEventManager globalEventManager;

    //**** SPEED ****
    public GameObject[] speedButtons = new GameObject[6];   //stop, pause, *1, *10, *100, *1000
    private Button[] buttons = new Button[6];       
    //private int currentSpeed;   //0, 1, 2, 3, 4, 5
    private int[] speedValues = new int[] { -1, 0, 1, 10, 80, 200 };

    //**** INPUTS ****
    public InputField seedInput;
    public Slider tableTaker;
    public Slider tableSharer;
    private int seed;
    private float takerRatio;
    private float sharerRatio;
    public Button updateConfig;


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
        seedInput.text = GlobalConstants.RANDOM_SEED.ToString();
        seedInput.onEndEdit.AddListener((s) => { this.onUpdateSeed(s); });
        tableTaker.value = GlobalConstants.TABLE_TAKER_RATIO;
        tableTaker.onValueChanged.AddListener((v) => { this.onUpdateTableTaker(v);});
        tableSharer.value = GlobalConstants.TABLE_SHARER_RATIO;
        tableSharer.onValueChanged.AddListener((v) => { this.onUpdateTableSharer(v); });
        updateConfig.onClick.AddListener(() => {
            //Update Static Configs
            GlobalConstants.updateConfig(seed, takerRatio, sharerRatio);
        });
    }

    private void onUpdateTableTaker(float v)
    {
        this.takerRatio = v;
        Debug.Log("Table Taker Ratio New: " + takerRatio);
    }

    private void onUpdateTableSharer(float v)
    {
        this.sharerRatio = v;
        Debug.Log("Table Sharer Ratio New: " + sharerRatio);
    }

    private void onUpdateSeed(string s)
    {
        this.seed = int.Parse(s);
        Debug.Log("Seed changed to: " + seed);
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
}
