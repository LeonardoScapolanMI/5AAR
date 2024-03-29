using Syrus.Plugins.DFV2Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ConversationController : MonoBehaviour
{
    public static ConversationController Instance { private set; get; } = null;

    private  DialogFlowV2Client client;

    //TODO implement a way to get session name (UUID)
    private string sessionName = Guid.NewGuid().ToString();

    private  List<Text> textOutputFields;
    private  List<TextMeshProUGUI> textPROOutputFields;
    private Action<string[]> optionsConsumer = null;

    private readonly object textFieldsLock = new object();
    private bool textLock = false;
    public bool textFieldsOverwritten { private set; get; }
    private Action afterWriteCallback = null;

    //private bool inactive = false;
    public List<Coroutine> inactivityCoroutines = new List<Coroutine>();
    private bool lastTextOverwritten = false;

    private string lastText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(transform.gameObject);

            client = GetComponent<DialogFlowV2Client>();
            client.ChatbotResponded += OnResponse;
            client.DetectIntentError += LogError;

            textOutputFields = new List<Text>();
            textPROOutputFields = new List<TextMeshProUGUI>();
            textFieldsOverwritten = true;

            InterfaceMethods.AddMethod("REPEAT", ()=>StartCoroutine(RepeatAfterSeconds(10)));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator RepeatAfterSeconds(int s)
    {
        string text = lastText;
        lastTextOverwritten = false;
        yield return new WaitForSecondsRealtime(s);
        if (!lastTextOverwritten)
            StartCoroutine(_ChangeTextFields(text, true));
    }

    // Start is called before the first frame update
    void Start()
    {
        //textFieldsOverwritten = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SendTextIntent(string text, Action callback = null)
    {
        //inactive = false;
        stopInactivityCoroutines();
        lastTextOverwritten = true;
        StartCoroutine(_SendTextIntent(text, callback));
    }

    private IEnumerator _SendTextIntent(string text, Action callback = null)
    {
        if (textFieldsLock != null)
        {
            bool canGo = false;
            new Thread(() =>
            {
                Monitor.Enter(textFieldsLock);
                try
                {
                    while (textLock)
                        Monitor.Wait(textFieldsLock);
                    canGo = true;
                    afterWriteCallback = callback;
                    textLock = true;
                    Monitor.PulseAll(textFieldsLock);
                }
                finally
                {
                    Monitor.Exit(textFieldsLock);
                }
            }).Start();
            CAAnimationsController.istance.SetLoading(true);
            yield return new WaitUntil(() => canGo);
            //lock (textFieldsLock)
            ///yield return new WaitUntil(() => mockLock);
            //{
            ///mockLock = false;
            textFieldsOverwritten = false;
            client.DetectIntentFromText(text, sessionName);
            yield return new WaitUntil(() => textFieldsOverwritten);
            //}
            ///mockLock = true;
        }
    }

    public void SendAudioIntent(AudioClip clip, Action callback = null)
    {
        //inactive = false;
        stopInactivityCoroutines();
        lastTextOverwritten = true;
        StartCoroutine(_SendAudioIntent(clip, callback));
    }

    private IEnumerator _SendAudioIntent(AudioClip clip, Action callback = null)
    {
        byte[] audioBytes = WavUtility.FromAudioClip(clip);
        string audioString = Convert.ToBase64String(audioBytes);
        if (textFieldsLock != null)
        {
            bool canGo = false;
            new Thread(() =>
            {
                Monitor.Enter(textFieldsLock);
                try
                {
                    while (textLock)
                        Monitor.Wait(textFieldsLock);
                    canGo = true;
                    afterWriteCallback = callback;
                    textLock = true;
                    Monitor.PulseAll(textFieldsLock);
                }
                finally
                {
                    Monitor.Exit(textFieldsLock);
                }
            }).Start();
            CAAnimationsController.istance.SetLoading(true);
            yield return new WaitUntil(() => canGo);
            //lock (textFieldsLock)
            ///yield return new WaitUntil(() => mockLock);
            //{
            //textFieldsOverwritten = false;
            client.DetectIntentFromAudio(audioString, sessionName);
            yield return new WaitUntil(() => textFieldsOverwritten);
            //}
            ///mockLock = true;
        }
    }

    public void SendEventIntent(string eventName, Dictionary<string, object> parameters, Action callback = null)
    {
        //inactive = false;
        stopInactivityCoroutines();
        lastTextOverwritten = true;
        StartCoroutine(_SendEventIntent(eventName, parameters, callback));
    }

    private IEnumerator _SendEventIntent(string eventName, Dictionary<string, object> parameters, Action callback = null)
    {
        if (textFieldsLock != null)
        {
            bool canGo = false;
            Debug.Log("pre-enter evnet intent");
            new Thread(() =>
            {
                Monitor.Enter(textFieldsLock);
                Debug.Log("Got lock");
                try
                {
                    Debug.Log(textLock);
                    while (textLock)
                        Monitor.Wait(textFieldsLock);
                    canGo = true;
                    afterWriteCallback = callback;
                    textLock = true;
                    Monitor.PulseAll(textFieldsLock);
                }
                finally
                {
                    Monitor.Exit(textFieldsLock);
                }
            }).Start();
            CAAnimationsController.istance.SetLoading(true);
            yield return new WaitUntil(() => canGo);
            //lock (textFieldsLock)
            ///yield return new WaitUntil(() => mockLock);
            //{
            ///mockLock = false;
            Debug.Log("enter evnet intent");
            Debug.Log(Thread.CurrentThread.ManagedThreadId.ToString());
            textFieldsOverwritten = false;
            client.DetectIntentFromEvent(eventName, parameters, sessionName);
            yield return new WaitUntil(() => textFieldsOverwritten);
            //Debug.Log("finish event intent");

            //}
            ///mockLock = true;
        }
    }

    public void SendEventIntent(string eventName, Action callback = null)
    {
        SendEventIntent(eventName, new Dictionary<string, object>(), callback);
    }

    public void ResetContext(DF2Context[] contexts = null)
    {
        client.ClearSession(sessionName);
        if (contexts != null)
        {
            foreach (DF2Context context in contexts)
                client.AddInputContext(context, sessionName);
        }
    }

    public void RegisterTextOutputField(Text field)
    {
        textOutputFields.Add(field);
    }

    public void UnregisterTextOutputField(Text field)
    {
        textOutputFields.Remove(field);
    }

    public void RegisterTextOutputField(TextMeshProUGUI field)
    {
        textPROOutputFields.Add(field);
    }

    public void UnregisterTextOutputField(TextMeshProUGUI field)
    {
        textPROOutputFields.Remove(field);
    }

    public void ChangeOptionsConsumer(Action<string[]> fun)
    {
        optionsConsumer = fun;
    }

    private void OnResponse(DF2Response response)
    {
        //Debug.Log(Thread.CurrentThread.ManagedThreadId.ToString());
        string responseText = GetResponseText(response);//response.queryResult.fulfillmentText;
        string method = response.queryResult.action;
        bool isRepeatResponse = method != null && method.Equals("REPEAT");
        StartCoroutine(_OverwriteTextFields(responseText,!isRepeatResponse));

        CustomPayload cp = GetCustomPayload(response);
        if (method!=null && InterfaceMethods.list.ContainsKey(method)) InterfaceMethods.list[method].Invoke();
        if (optionsConsumer != null && !isRepeatResponse) optionsConsumer.Invoke(cp?.options);
    }

    private string GetResponseText(DF2Response response)
    {
        string text = response.queryResult.fulfillmentText;
        Debug.Log(text);

        CustomPayload cp = GetCustomPayload(response);
        if (cp != null && cp.substitutions!=null)
        {
            Substitution[] sub = cp.substitutions;
            foreach(Substitution s in sub)
            {
                if (Parameters.list.ContainsKey(s.parameterName) && Parameters.list[s.parameterName]!=null) text = text.Replace(s.placeholder, Parameters.list[s.parameterName].Invoke());
            }
        }

        return text;
    }

    private CustomPayload GetCustomPayload(DF2Response response)
    {
        if (response.queryResult.fulfillmentMessages.Length <= 1) return null;
        string s = response.queryResult.fulfillmentMessages[1]["payload"].ToString();
        return JsonUtility.FromJson<CustomPayload>(s);
    }

    private IEnumerator _OverwriteTextFields(string text, bool rememberText)
    {
        //Debug.Log(name + " said: \"" + response.queryResult.fulfillmentText + "\"");
        if(rememberText) lastText = text;
        foreach (Text field in textOutputFields)
            field.text = text;

        foreach (TextMeshProUGUI field in textPROOutputFields)
            field.text = text;

        TTSController.Speak(text);

        if (afterWriteCallback != null) afterWriteCallback.Invoke();
        CAAnimationsController.istance.SetLoading(false);

        yield return new WaitForSecondsRealtime(10);
        new Thread(() =>
        {
            Monitor.Enter(textFieldsLock);
            try
            {
                textFieldsOverwritten = true;
                textLock = false;
                Monitor.PulseAll(textFieldsLock);
            }
            finally
            {
                Monitor.Exit(textFieldsLock);
            }
        }).Start();
        Debug.Log("finish overwrite");

    }

    public void ChangeTextFields(string text, Action callback = null)
    {
        lastTextOverwritten = true;
        StartCoroutine(_ChangeTextFields(text, true, callback));
    }

    private IEnumerator _ChangeTextFields(string text, bool rememberText, Action callback = null)
    {
        if (textFieldsLock != null)
        {
            //lock (textFieldsLock)
            bool canGo = false;
            new Thread(() =>
            {
                Monitor.Enter(textFieldsLock);
                try
                {
                    while (textLock)
                        Monitor.Wait(textFieldsLock);
                    canGo = true;
                    afterWriteCallback = callback;
                    textLock = true;
                    Monitor.PulseAll(textFieldsLock);
                }
                finally
                {
                    Monitor.Exit(textFieldsLock);
                }
            }).Start();
            CAAnimationsController.istance.SetLoading(true);
            yield return new WaitUntil(() => canGo);
            ///yield return new WaitForSecondsRealtime(1);
            ///yield return new WaitUntil(() => mockLock);
            //{
            ///mockLock = false;
            Debug.Log("enter _change text fields");
            Debug.Log(Thread.CurrentThread.ManagedThreadId.ToString());
            textFieldsOverwritten = false;
            StartCoroutine(_OverwriteTextFields(text, rememberText));
            Debug.Log("textFieldsOverwritten:" + textFieldsOverwritten);
            yield return new WaitUntil(() => textFieldsOverwritten);
            //Debug.Log("finish _change text fields");
            //}
            ///mockLock = true;
        }
    }
    /*public void ChangeTextFieldsPriority(string text)
    {
        StartCoroutine(_ChangeTextFieldsPriority(text));

    }

    private IEnumerator _ChangeTextFieldsPriority(string text)
    {
        if (textFieldsLock != null)
        {
            //lock (textFieldsLock);
            yield return new WaitUntil(() => mockLock);
            //{
            mockLock = false;
            Debug.Log("enter _change text fields");
            Debug.Log(Thread.CurrentThread.ManagedThreadId.ToString());
            textFieldsOverwritten = false;
            StartCoroutine(_OverwriteTextFields(text));
            yield return new WaitUntil(() => textFieldsOverwritten);
            Debug.Log("finish _change text fields");
            //}
            mockLock = true;
        }
    }*/

    public void DoSomethingOnInactivity(float time, Action action)
    {
        inactivityCoroutines.Add(StartCoroutine(_DoSomethingOnInactivity(time, action)));
    }

    private IEnumerator _DoSomethingOnInactivity(float time, Action action)
    {
        //inactive = true;
        yield return new WaitForSecondsRealtime(time);
        //if (inactive)
        action.Invoke();
    }

    private void stopInactivityCoroutines()
    {
        if (inactivityCoroutines.Count > 0)
        {
            foreach (Coroutine c in inactivityCoroutines) StopCoroutine(c);
            inactivityCoroutines = new List<Coroutine>();
        }
    }

    private void LogError(DF2ErrorResponse errorResponse)
    {
        Debug.LogError(string.Format("Error {0}: {1}", errorResponse.error.code.ToString(), errorResponse.error.message));
        ChangeTextFields("ERROR");
    }
}

[Serializable]
public class CustomPayload
{
    public string[] options;
    public Substitution[] substitutions;
}

[Serializable]
public class Substitution
{
    public string placeholder;
    public string parameterName;
}

public class InterfaceMethods
{
    public static readonly Dictionary<string, Action> list = new Dictionary<string, Action>
    {
        { "HELP_SUBSCRIPTION", () =>{ } },
        { "HELP_STOP_BUTTON", () =>{ } },
        { "HELP_TICKET_MACHINE", () =>{ } },
        { "FIND_TABACCHI_SHOP", () =>{ } },  //start the jurney towords the nearest tabacchi shop
        { "FIND_BUS_STOP", () =>{ } }, //start the jurney towords the bus stop
        { "FIND_ANOTHER_TABACCHI_SHOP", () =>{ } }, //start the jurney towords another tabacchi shop (because the first was closed)
        { "CHECK_TICKET", () =>{ } }, //the ticket got recognized
        { "INSIDE_THE_BUS", () =>{ } },
        { "GOT_OFF_THE_BUS", () =>{ } },
        { "FINAL_REWARD", () =>{ } },
        { "REPEAT", () =>{ } },
        { "GET_OFF_INACTIVITY", () =>{ } },
        { "TICKET_YES", () =>{ } },
        { "TICKET_NO", () =>{ } },
        { "TICKET_HELP", () =>{ } },
        { "ARROW_FACILITATOR", () =>{ } }
    };

    public static bool AddMethod(string interfaceName, Action method)
    {
        if (list.ContainsKey(interfaceName))
        {
            list[interfaceName] = method;
            return true;
        }

        return false;
    }

    public static bool RemoveMethod(string interfaceName)
    {
        if (list.ContainsKey(interfaceName))
        {
            list[interfaceName] = () => { };
            return true;
        }

        return false;
    }
}

public class Parameters
{
    public static readonly Dictionary<string, Func<string>> list = new Dictionary<string, Func<string>>
    {
        { "timeToBus", null },
        { "busNumber", null },
        { "busArrivalTime", null },
        { "destination", null },
        { "distance", null }
    };

    public static bool AddParameter(string parameterName, Func<string> parameterPrinter)
    {
        if (list.ContainsKey(parameterName))
        {
            list[parameterName] = parameterPrinter;
            return true;
        }

        return false;
    }

    public static bool RemoveParameter(string parameterName)
    {
        if (list.ContainsKey(parameterName))
        {
            list[parameterName] = null;
            return true;
        }

        return false;
    }
}
