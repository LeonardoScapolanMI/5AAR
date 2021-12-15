using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class InputFieldSubmit : MonoBehaviour
{
    //dest start from home:"45.516826", "9.216668"
    //start from the south 45.48007783995513, 9.228568817237909
    public static string[] destinationCoordinates = new string[2] {"45.4802374", "9.2279100"}; // fake bus stop: north of building 13 45.480237431274034, 9.227910040365503
    public static string[] tabacchiCoordinates = new string[2] {"45.4797353", "9.2279137"}; //fake tabacchi: south of building 13 45.479735303548296, 9.227913743827143
    public InputField destinationCord;
    public InputField tabacchiCord;
    public Text settingsState;

    public void Awake()
    {
        destinationCord.placeholder.GetComponent<Text>().text = destinationCoordinates[0]+","+destinationCoordinates[1];
        tabacchiCord.placeholder.GetComponent<Text>().text = tabacchiCoordinates[0]+","+tabacchiCoordinates[1];
        DontDestroyOnLoad(transform.gameObject);
    }
    public void LockDestInput(InputField inputField)
    {
        destinationCoordinates = inputField.text.Split(',');
    }
    public void LockTabacchiInput(InputField inputField)
    {
        tabacchiCoordinates = inputField.text.Split(',');
    }
    public void Start()
	{
		destinationCord.onEndEdit.AddListener(delegate{LockDestInput(destinationCord);});
		tabacchiCord.onEndEdit.AddListener(delegate{LockTabacchiInput(tabacchiCord);});

	}

}