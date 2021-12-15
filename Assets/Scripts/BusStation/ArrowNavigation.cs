using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Globalization;

public class ArrowNavigation : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private GPSLocation GPSInstance;
    private GoogleMapAPIQuery GoogleAPIScript;
    private Utils utils;
    private Action afterDestinationCallback = null;
    [SerializeField] private GameObject CompassPerfab;
    [SerializeField] private UnityARCompass.ARCompassIOS ARCompassIOS;
    [SerializeField] private TextMeshProUGUI Instruction;
    
    float lat;
    float lng;
	float destLat;
	float destLng;
	int count;
	List<step> steps;
    float brng;
    float compassBrng;
    TextMeshProUGUI textMeshProUGUI;
    float tabacchiLat;
    float tabacchiLng;
    
    //compass related 
    GameObject compass;
    public GameObject ARCamera;
    private int forwardOffset = 1;
    private int upOffset = 1;
    //public string testString = "11111";
    void Awake(){
    }
    // Start is called before the first frame update
    void Start()
    {
        GoogleAPIScript = GetComponent<GoogleMapAPIQuery>();
        ConversationController.istance.RegisterTextOutputField(Instruction);
        utils = Utils.Instance;
        GPSInstance = GPSLocation.Instance;
    }
    public void ShowNavigationInformation(Phases phase, Action callback){
        StartCoroutine(StepsInformation(phase));
        afterDestinationCallback = callback;
    }
    IEnumerator StepsInformation(Phases phase)
    {
        if(phase == Phases.BUY_TICKET)
        {
            destLat = float.Parse(InputFieldSubmit.tabacchiCoordinates[0], CultureInfo.InvariantCulture);
            destLng = float.Parse(InputFieldSubmit.tabacchiCoordinates[1], CultureInfo.InvariantCulture);
            yield return new WaitForSecondsRealtime(1);
            yield return StartCoroutine(ClickToGetStepsInformation());
        }
        else if(phase == Phases.FIND_BUS_STOP) 
        {
            destLat = float.Parse(InputFieldSubmit.destinationCoordinates[0], CultureInfo.InvariantCulture);
            destLng = float.Parse(InputFieldSubmit.destinationCoordinates[1], CultureInfo.InvariantCulture);
            yield return new WaitForSecondsRealtime(1);
            yield return StartCoroutine(ClickToGetStepsInformation());
        }
        

    }
    IEnumerator ClickToGetStepsInformation()
    {
        int maxWait = 3;
        while(GoogleAPIScript.walkingSteps.Count == 0 && maxWait > 0){
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if(maxWait <= 0) yield return 0;
        steps = GoogleAPIScript.walkingSteps;
        //instantiate prefab
        compass = Instantiate(CompassPerfab) as GameObject;
        //panel = Instantiate(PanelPrefab) as GameObject;
        panel.SetActive(true);
        textMeshProUGUI = panel.GetComponent<TextMeshProUGUI>();
        Debug.Log(textMeshProUGUI.text);
        //Debug.Log(JsonUtility.ToJson(steps[0], true));
        Instruction.text = "follow the arrow"; //utils.RemoveSpecialCharacters(steps[count].html_instructions);
        textMeshProUGUI.text = "Distance here";
        //texts[0].text = "Distance here";
        //texts[1].text = "Default"; // description
        //texts[2].text = "Step " + (count+1) + " / " + steps.Count;
        //texts[3].text = steps[count].end_location.lat + ", " + steps[count].end_location.lng;
        
        //StepLoop starts in 0.1s and repeating running every 0.5s
        InvokeRepeating("StepsLoop", 0.1f, 0.5f);
    }

    public void StepsLoop()
    {
        lat = GPSInstance.lat;
        lng = GPSInstance.lng;
        //destLat = steps[count].end_location.lat;
        //destLng = steps[count].end_location.lng;
        //update compass direction
        ARCompassIOS.startLat = lat;
        ARCompassIOS.startLng = lng;
        ARCompassIOS.endLat = destLat;
        ARCompassIOS.endLng = destLng;
        int distance = Mathf.RoundToInt(utils.CalculateDistanceMeters(lat, lng, destLat, destLng));
        // constantly update distance shown
        //Debug.Log("distance:"+distance.ToString());
        textMeshProUGUI.text = distance.ToString() + "m";
        //texts[0].text = distance.ToString() + "m";

        if (isCollide())
        {
            Destroy(compass);
            panel.SetActive(false);
            //Destroy(CompassObject);
            if(afterDestinationCallback != null) {afterDestinationCallback.Invoke();};
            CancelInvoke();
        }
				
	}

    bool isCollide() {
		lat = Input.location.lastData.latitude;
		lng = Input.location.lastData.longitude;
        //collide within 10m
		if (lat - destLat <= 0.0001f && lat - destLat >= -0.0001f) {
			if (lng - destLng <= 0.0001f && lng - destLng >= -0.0001f) {
				return true;
			}
		}

		return false;
	}
    
	// Update is called once per frame
	void Update () {      
        if(compass != null) {
            compass.transform.rotation = ARCompassIOS.TrueHeadingRotation;
            //the position is always on the front of camera with a offset
            compass.transform.position = ARCamera.transform.position + ARCamera.transform.forward * forwardOffset;
            //Debug.Log("compass rotation"+compass.transform.rotation);
            
        }
    }
}