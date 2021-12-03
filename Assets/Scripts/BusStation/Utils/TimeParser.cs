using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeParser : MonoBehaviour
{
    private static TimeParser _instance;

    public static TimeParser Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    public DateTime TimeParse(string timeStr){
        DateTime parsedTime;

        string hhmmss = timeStr +":00";
        Debug.Log("time:" + hhmmss);
        string[] formats = {"HH:mm:ss"};
        if (DateTime.TryParseExact(hhmmss, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out parsedTime))
            return parsedTime;
        else{
            Debug.Log("time parse error");
            return parsedTime;
        }
    }
}
