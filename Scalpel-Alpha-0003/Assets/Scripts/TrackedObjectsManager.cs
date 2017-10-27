using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Vuforia;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;

public class TrackedObjectsManager : MonoBehaviour {

    public Button mainButton;
    public Text scalpelString;
    public Text pincerString;
    public Text scissorsString;
    public Text activeObjectString;

    private StateManager trackerManager;
    private MyTrackableObject mTrackedObject;
    private Dictionary<string, MyTrackableObject> myTrackedObjectsDict;

    private List<string> activeObjectsList = new List<string>(new string[] { "2ofclubs", "3ofclubs" });
    private enum objectStates : int { visible, attention, active, unsure, error };

    private bool ProcessingBool = false;
    private int counter = 0;
    private string attentionObject;

    public AudioRecorder recorder;
    public AudioPlayer player;

    private string 
        username = "25cd6fcb-aa09-4ed7-a97b-a7e991fe8564", 
        password = "Hf4XEkydMnNJ", 
        url = "https://stream.watsonplatform.net/speech-to-text/api";

    private SpeechToText _speechToText;

    //INIT

    void Start () {

        Credentials credentials = new Credentials( username, password, url);
        _speechToText = new SpeechToText(credentials);
        GetWatsonModels();

        trackerManager = TrackerManager.Instance.GetStateManager();

        mainButton.onClick.AddListener(StartProcess);

        myTrackedObjectsDict = new Dictionary<string, MyTrackableObject>();
    }

    // UPDATE - CALLED ONCE PER FRAME

    void Update () {

        if (ProcessingBool)
        {
            // RANDOM "ATTENTION" - TO BE REPLACED WITH WATSON

            //counter++;

            //if (counter >= 1000)
            //{
            //    counter = 0;

            //    System.Random r = new System.Random();

            //    if (r.Next(0, 2) == 0)
            //    { attentionObject = (string)activeObjectsList[0]; }
            //    else
            //    { attentionObject = (string)activeObjectsList[1]; }
            //}

            activeObjectString.text = attentionObject;

            // RANDOM END 

            IEnumerable<TrackableBehaviour> activeTrackables = trackerManager.GetActiveTrackableBehaviours();

            foreach (var obj in myTrackedObjectsDict)
            {
                if (obj.Value.State == (int)objectStates.attention)
                {
                    // REQUESTED - YELLOW

                    obj.Value.State = (int)objectStates.active;

                    if (obj.Key == "Scalpel")
                    {
                        scalpelString.color = Color.yellow;
                    }
                    else if (obj.Key == "Pincers")
                    {
                        pincerString.color = Color.yellow;
                    }
                    else if (obj.Key == "Scissors")
                    {
                        scissorsString.color = Color.yellow;
                    }
                }
                else
                {
                    //UNSURE - ORNAGE

                    obj.Value.State = (int)objectStates.unsure;

                    if (obj.Key == "Scalpel")
                    {
                        scalpelString.color = new Color(255, 165, 0);
                    }
                    else if (obj.Key == "Pincers")
                    {
                        pincerString.color = new Color(255, 165, 0);
                    }
                    else if (obj.Key == "Scissors")
                    {
                        scissorsString.color = new Color(255, 165, 0);
                    }
                }
            }
            
            // Iterate through the list of active trackables
            foreach (TrackableBehaviour tracked in activeTrackables)
            {
                //TRACKED - RESET TO GRAY

                if (tracked.TrackableName == "Scalpel")
                {
                    mTrackedObject = ContainedInDict("Scalpel");
                    mTrackedObject.State = (int)objectStates.visible;
                    scalpelString.color = Color.gray;
                }
                else if (tracked.TrackableName == "Pincers")
                {
                    mTrackedObject = ContainedInDict("Pincers");
                    mTrackedObject.State = (int)objectStates.visible;
                    pincerString.color = Color.gray;
                }
                else if (tracked.TrackableName == "Scissors")
                {
                    mTrackedObject = ContainedInDict("Scissors");
                    mTrackedObject.State = (int)objectStates.visible;
                    scissorsString.color = Color.gray;
                }

                //ATTENTION - GREEN

                if (tracked.TrackableName == "Scalpel" && ( attentionObject == "scalpel" || attentionObject == "Scalpel" ) )
                {
                    mTrackedObject.State = (int)objectStates.attention;
                    scalpelString.color = Color.green;
                }
                else if (tracked.TrackableName == "Pincers" && (attentionObject == "pincers" || attentionObject == "Pincers"))
                {
                    mTrackedObject.State = (int)objectStates.attention;
                    pincerString.color = Color.green;
                }
                else if (tracked.TrackableName == "Scissors" && (attentionObject == "scissor" || attentionObject == "Scissor" || attentionObject == "scissors" || attentionObject == "Scissors"))
                {
                    mTrackedObject.State = (int)objectStates.attention;
                    scissorsString.color = Color.green;
                }
            }
        }
    }

    //HELPER METHOD

    private MyTrackableObject ContainedInDict(string objName)
    {
        MyTrackableObject obj;

        if (!myTrackedObjectsDict.ContainsKey(objName))
        {
            obj = new MyTrackableObject(objName, (int)objectStates.visible);
            myTrackedObjectsDict.Add(objName, obj);
        }
        else
            obj = myTrackedObjectsDict[objName];

        return obj;
    }

    //LATE UPDATE - CLEAN UP SCENE

    private void LateUpdate()
    {
        if (!ProcessingBool)
        {
            foreach (var obj in myTrackedObjectsDict)
            {
                print("END " + obj.Key);

                if (obj.Value.State != (int)objectStates.visible )
                    //&& obj.Value.State != (int)objectStates.attention)
                {
                    obj.Value.State = (int)objectStates.error;
                    if (obj.Key == "Scalpel")
                    {
                        scalpelString.color = Color.red;
                    }
                    else if (obj.Key == "Pincers")
                    {
                        pincerString.color = Color.red;
                    }
                    else if (obj.Key == "Scissors")
                    {
                        scissorsString.color = Color.red;
                    }
                }
            }
        }
    }

    // MAIN BUTON - START / STOP PROCESS

    void StartProcess ()
    {
        if (mainButton.GetComponentInChildren<Text>().text == "START")
        {
            print("START PROCESS");
            ProcessingBool = true;
            mainButton.GetComponentInChildren<Text>().text = "STOP";

            scalpelString.color = Color.gray;
            pincerString.color = Color.gray;
            scissorsString.color = Color.gray;

            StartRecording();
        }
        else
        {
            ProcessingBool = false;
            mainButton.GetComponentInChildren<Text>().text = "START";

            //_speechToText.StopListening();
        }
    }

    //WATSON - SpeechToText
    bool init = true;
    private void StartRecording()
    {
        if (recorder != null )
        {
            recorder.StartRecording();
            print("RECORDING ");

            if (init)
            {
                init = false;
                recorder.OnRecordingEnd.AddListener(AudioClipCallback);
            }

            //_speechToText.StartListening(OnRecognize);
        }
    }

    private void AudioClipCallback(AudioClip clip)
    {
        if (player != null)
        {
            //player.StartPlaying(clip);
        }
        else
            print("***PLAYER MISSING***");

        //AudioData record = new AudioData( clip, Mathf.Max(clip.samples / 2) );

        //int midPoint = clip.samples / 2;
        //float[] samples = new float[midPoint];
        //clip.GetData(samples, 0);

        //AudioData record = new AudioData();
        //record.MaxLevel = Mathf.Max(samples);
        //record.Clip = AudioClip.Create("Recording", midPoint, clip.channels, clip.frequency, false);
        //record.Clip.SetData(samples, 0);

        //_speechToText.OnListen(record);

        AudioClip _audioClip = WaveFile.ParseWAV("testClip", AudioClipToByteArray( clip ));

        _speechToText.Recognize(_audioClip, OnRecognize);


        print("AUDIO CLIP SENT");
    }

    private void GetWatsonModels()
    {
        if (!_speechToText.GetModels(HandleGetModels))
            Debug.Log("***FAILED TO GET MODELS***");
    }

    private void HandleGetModels(ModelSet result, string customData)
    {
        Debug.Log("ExampleSpeechToText" + " Speech to Text - Get models response: {0} " + customData);
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {        
        print("REGOGNIZED");

        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                //foreach (var alt in res.alternatives)
                //{
                //    string text = alt.transcript;

                //    print("RESULT: " + text);
                //}

                foreach (var alt in res.alternatives)
                {
                    string text = alt.transcript;
                    Debug.Log("ExampleStreaming " + string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));

                    if (res.final)
                        attentionObject = text;
                }
            }
        }

        StartRecording();
    }

    // Returns a wav file from any AudioClip (doesn't saves):
    private static byte[] AudioClipToByteArray(AudioClip clip)
    {
        // Clip content:
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);                                                       // The audio data in samples.

        // Write all data to byte array:
        List<byte> wavFile = new List<byte>();

        // RIFF header:
        wavFile.AddRange(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });    // "RIFF"
        wavFile.AddRange(System.BitConverter.GetBytes(samples.Length * 2 + 44 - 8));    // ChunkSize
        wavFile.AddRange(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });    // "WAVE"
        wavFile.AddRange(new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });    // "fmt"
        wavFile.AddRange(System.BitConverter.GetBytes(16));                             // Subchunk1Size (16bit for PCM)
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)1));                      // AudioFormat (1 for PCM)
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)clip.channels));          // NumChannels
        wavFile.AddRange(System.BitConverter.GetBytes(clip.frequency));                 // SampleRate
        wavFile.AddRange(System.BitConverter.GetBytes(clip.frequency * clip.channels * (16 / 8)));    // ByteRate
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)(clip.channels * (16 / 8))));         // BlockAlign
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)16));                                 // BitsPerSample
        wavFile.AddRange(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });    // "data"
        wavFile.AddRange(System.BitConverter.GetBytes(samples.Length * 2));             // Subchunk2Size
        // Add the audio data in bytes:
        for (int i = 0; i < samples.Length; i++)
        {
            short sample = (short)(samples[i] * 32768.0F);
            wavFile.AddRange(System.BitConverter.GetBytes(sample));                     // The audio data in bytes.
        }
        // Return the byte array to be saved:
        return wavFile.ToArray();
    }
}