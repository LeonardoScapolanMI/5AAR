using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepConverter : MonoBehaviour
{
    private static StepConverter _instance;

    public static StepConverter Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    public List<step> Convert(List<innerStep> steps){
        Debug.Log(JsonUtility.ToJson(steps[0], true));
        List<step> newSteps = new List<step>();
        foreach(innerStep step in steps){
            step newStep = new step();
            newStep.end_location = step.end_location;
            newStep.start_location = step.start_location;
            newStep.travel_mode = step.travel_mode;
            newStep.dis = step.dis;
            newStep.dur = step.dur;
            newSteps.Add(newStep);
        }
            Debug.Log(JsonUtility.ToJson(newSteps, true));
        return newSteps;
    }
}
