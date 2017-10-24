using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class TrackedObjectsManager : MonoBehaviour {

    public Button mainButton;
    public Text scalpelString;
    public Text spongeString;
    public Text scissorsString;

    public Text activeObjectString;

    private StateManager trackerManager;

    private bool ProcessingBool = false;
    private int counter = 0;
    private string attentionObject;

    private MyTrackableObject mTrackedObject;
    private Dictionary<string, MyTrackableObject> myTrackedObjectsDict;

    private List<string> activeObjectsList = new List<string>(new string[] { "2ofclubs", "3ofclubs" });
    private enum objectStates : int { visible, attention, active, unsure, error };

	// Use this for initialization
	void Start () {

        //Credentials credentials = new Credentials(< username >, < password >, < url >);
        //SpeechToText _speechToText = new SpeechToText(credentials);

        trackerManager = TrackerManager.Instance.GetStateManager();

        mainButton.onClick.AddListener(StartProcess);

        myTrackedObjectsDict = new Dictionary<string, MyTrackableObject>();
    }
	
	// Update is called once per frame
	void Update () {

        if (ProcessingBool)
        {
            counter++;

            if (counter >= 1000)
            {
                counter = 0;

                System.Random r = new System.Random();

                if (r.Next(0, 2) == 0)
                { attentionObject = (string)activeObjectsList[0]; }
                else
                { attentionObject = (string)activeObjectsList[1]; }

                activeObjectString.text = attentionObject;
            }

            IEnumerable<TrackableBehaviour> activeTrackables = trackerManager.GetActiveTrackableBehaviours();

            foreach (var obj in myTrackedObjectsDict)
            {
                if (obj.Value.State == (int)objectStates.attention)
                {
                    // REQUESTED - YELLOW

                    obj.Value.State = (int)objectStates.active;

                    if (obj.Key == "2ofclubs")
                    {
                        scalpelString.color = Color.yellow;
                    }
                    else if (obj.Key == "3ofclubs")
                    {
                        spongeString.color = Color.yellow;
                    }
                }
                else
                {
                    //UNSURE - ORNAGE

                    obj.Value.State = (int)objectStates.unsure;

                    if (obj.Key == "2ofclubs")
                    {
                        scalpelString.color = new Color(255, 165, 0);
                    }
                    else if (obj.Key == "3ofclubs")
                    {
                        spongeString.color = new Color(255, 165, 0);
                    }
                }
            }
            
            // Iterate through the list of active trackables
            foreach (TrackableBehaviour tracked in activeTrackables)
            {
                //TRACKED - RESET TO GRAY

                if (tracked.TrackableName == "2ofclubs")
                {
                    mTrackedObject = ContainedInDict("2ofclubs");
                    mTrackedObject.State = (int)objectStates.visible;
                    scalpelString.color = Color.gray;
                }
                else if (tracked.TrackableName == "3ofclubs")
                {
                    mTrackedObject = ContainedInDict("3ofclubs");
                    mTrackedObject.State = (int)objectStates.visible;
                    spongeString.color = Color.gray;
                }

                //ATTENTION - GREEN

                if (tracked.TrackableName == "2ofclubs" && attentionObject == "2ofclubs")
                {
                    mTrackedObject.State = (int)objectStates.attention;
                    scalpelString.color = Color.green;
                }
                else if (tracked.TrackableName == "3ofclubs" && attentionObject == "3ofclubs")
                {
                    mTrackedObject.State = (int)objectStates.attention;
                    spongeString.color = Color.green;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!ProcessingBool)
        {
            foreach (var obj in myTrackedObjectsDict)
            {
                if (obj.Value.State != (int)objectStates.visible && obj.Value.State != (int)objectStates.attention)
                {
                    obj.Value.State = (int)objectStates.error;
                    if (obj.Key == "2ofclubs")
                    {
                        scalpelString.color = Color.red;
                    }
                    else if (obj.Key == "3ofclubs")
                    {
                        spongeString.color = Color.red;
                    }
                }
            }
        }
    }

    void StartProcess ()
    {
        if (mainButton.GetComponentInChildren<Text>().text == "START")
        {
            print("START PROCESS");
            ProcessingBool = true;
            mainButton.GetComponentInChildren<Text>().text = "STOP";

            scalpelString.color = Color.gray;
            spongeString.color = Color.gray;
            scissorsString.color = Color.gray;
        }
        else
        {
            ProcessingBool = false;
            mainButton.GetComponentInChildren<Text>().text = "START";
        }
    }

    private MyTrackableObject ContainedInDict(string objName)
    {
        MyTrackableObject obj;

        if (!myTrackedObjectsDict.ContainsKey(objName))
        {
            obj = new MyTrackableObject(objName, (int) objectStates.visible);
            myTrackedObjectsDict.Add( objName, obj );
        }
        else
            obj = myTrackedObjectsDict[ objName ];

        return obj;
    }
}