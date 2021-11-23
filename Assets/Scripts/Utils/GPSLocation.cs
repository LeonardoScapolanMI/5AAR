using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPSLocation : MonoBehaviour
{
    [HideInInspector]
    public float lat;
    [HideInInspector]
    public float lon;
    [HideInInspector]
    public string status;
    private static GPSLocation _instance;

    public static GPSLocation Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GPSLoc());

    }
    // reference : https://www.youtube.com/watch?v=JWccDbm69Cg
    IEnumerator GPSLoc(){
        if(!Input.location.isEnabledByUser) yield break;

        Input.location.Start(5.0f);

        int maxWait = 20;

        while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0){
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        //service didn't init in 20 sec
        if(maxWait < 1){
            status = "Time out";
            yield break;
        }
        //connection failed
        if(Input.location.status == LocationServiceStatus.Failed){
            status = "Unable to determin device location";
            yield break;
        }else{
            status = "Running";
            Debug.Log("GPS Location is "+ status);
            //print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            InvokeRepeating("UpdateGPSData", 0f, 1f);
        }
    }
    private void UpdateGPSData(){
        if(Input.location.status == LocationServiceStatus.Running){
            //Access granted to gps values and it has been init
            status = "Running";
            // latitudeValue.text = Input.location.lastData.latitude.ToString();
            // longitudeValue.text = Input.location.lastData.longitude.ToString();
            // altitudeValue.text = Input.location.lastData.altitude.ToString();
            // horizontalAccuracyValue.text = Input.location.lastData.horizontalAccuracy.ToString();
            // timestampValue.text = Input.location.lastData.timestamp.ToString();
            lat = Input.location.lastData.latitude;
            lon = Input.location.lastData.longitude;
            Debug.Log(lat+"  "+lon);
        }else{
            // service is stopped
            status = "Stop";
        }
    }
}
