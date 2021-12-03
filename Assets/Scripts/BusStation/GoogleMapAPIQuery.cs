using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using UnityEngine.UI;
using System.Collections.Specialized;
using UnityEngine.Networking;

public class GoogleMapAPIQuery : MonoBehaviour
{
    GPSLocation GPSInstance;
    StepConverter StepConverterInstance;
    //import api key from .config file
    private string APIKey;
    private string keyword = "tabacchi";
    [HideInInspector]
    public Text resultValue;
    private string radius = "150";
    [HideInInspector]
    public List<step> walkingSteps;
    [HideInInspector]
    public List<step> walkingToTabacchiSteps;
    [HideInInspector]
    public transitDetails busInformation;
    [HideInInspector]
    public loc tabacchiLoc = new loc();
    void Awake(){

    }
    void  Start(){
        GPSInstance = GPSLocation.Instance;
        StepConverterInstance = StepConverter.Instance;
        APIKey = GlobalConfig.GoogleMapAPIKey;
    }
    public void RouteToTabacchiShopQuery()
    {
        StartCoroutine(TabacchiInOrder());
        // if(tabacchiLoc.lat == 0.0 || tabacchiLoc.lng == 0.0) {
        // Debug.Log("tabacchiLoc is null");
        // return;
        // }
        // Debug.Log("tabacchi lat in query"+tabacchiLoc.lat);
        // StartCoroutine (GetWalkRouteJSON (tabacchiLoc.lat, tabacchiLoc.lng));
    }
    IEnumerator TabacchiInOrder() {
        yield return StartCoroutine(GetTabacchiJSON());
        yield return StartCoroutine(GetWalkRouteJSON (tabacchiLoc.lat, tabacchiLoc.lng));
    }
    public void RouteToBusStationQuery()
    {
        //get value from settings scene
        float destLat = float.Parse(InputFieldSubmit.destinationCoordinates[0]);
        float destLng = float.Parse(InputFieldSubmit.destinationCoordinates[1]);
        StartCoroutine (GetBusRouteJSON (destLat, destLng));//45.5168268f, 9.2166683f
    }

    IEnumerator GetTabacchiJSON() {
		string baseURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
		string keyword = "keyword="+this.keyword;
        string location = "location="+GPSInstance.lat.ToString()+"%2C"+GPSInstance.lng.ToString();
        string radius = "radius="+this.radius;
		string apiKey = "key="+APIKey;
		string api = baseURL + keyword + "&" + location + "&" + radius + "&" + apiKey;
		//Debug.Log(api);
        UnityWebRequest www = UnityWebRequest.Get(api);
		yield return www.Send();
		if(www.isNetworkError) {
			Debug.Log(www.error);
		}
		else {
			// Show results as text
			string result = www.downloadHandler.text;
			//Debug.Log (result);
            //parse the result
            attribution attr = JsonUtility.FromJson<attribution>(result);
            //Debug.Log(JsonUtility.ToJson(attr, true));
            int maxWait = 3;
            while(attr.results.Count == 0 && maxWait > 0){
                yield return new WaitForSeconds(1);
                maxWait--;
            }
            if(maxWait <= 0) yield return 0;
            tabacchiLoc.lat = attr.results[0].geometry.location.lat;
            tabacchiLoc.lng = attr.results[0].geometry.location.lng;
		}

	}

    IEnumerator GetBusRouteJSON(float destLat, float destLng) {
        string baseURL = "https://maps.googleapis.com/maps/api/directions/json?";
        //string origin = "origin=" + "45.5219%2C9.2216939";
		string origin = "origin="+GPSInstance.lat.ToString()+"%2C"+GPSInstance.lng.ToString();
        //TODO: set the dest
        string dest = "destination=" + destLat +"%2C" + destLng;
		string mode = "mode=transit";
        string transit_mode = "transit_mode=bus";
        string transit_routing_preference="transit_routing_preference=fewer_transfers";
		string apiKey = "key="+APIKey;
		string api = baseURL + origin + "&" + dest + "&" + mode + "&" + apiKey + "&" + transit_mode + "&" + transit_routing_preference;
        //Debug.Log(api);
		UnityWebRequest www = UnityWebRequest.Get(api);
		yield return www.Send();
		if(www.isNetworkError) {
			Debug.Log(www.error);
		}
		else {
			string result = www.downloadHandler.text;
			geoCoded g = JsonUtility.FromJson<geoCoded>(result);
			leg l = g.routes [0].legs [0];
            //steps include walking part and bus part
            foreach (step step in l.steps)
            {
                if(step.travel_mode == "WALKING"){
                    //convert innnerstep type to step
                    walkingSteps = StepConverterInstance.Convert(step.steps);
                }
                else if(step.travel_mode == "TRANSIT"){
                    busInformation = step.transit_details;
                    break;
                }
            }
        }
    }
    IEnumerator GetWalkRouteJSON(float destLat, float destLng) {
        string baseURL = "https://maps.googleapis.com/maps/api/directions/json?";
        //string origin = "origin=" + "45.5219%2C9.2216939";
		string origin = "origin="+GPSInstance.lat.ToString()+"%2C"+GPSInstance.lng.ToString();
        //TODO: set the dest
        string dest = "destination=" + destLat +"%2C" + destLng;
		string mode = "mode=walking";
		string apiKey = "key="+APIKey;
		string api = baseURL + origin + "&" + dest + "&" + mode + "&" + apiKey;
        //Debug.Log(api);
		UnityWebRequest www = UnityWebRequest.Get(api);
		yield return www.Send();
		if(www.isNetworkError) {
			Debug.Log(www.error);
		}
		else {
			string result = www.downloadHandler.text;
			geoCoded g = JsonUtility.FromJson<geoCoded>(result);
			leg l = g.routes [0].legs [0];
            //steps include walking part and bus part
			walkingSteps = l.steps;
        }
    }
}