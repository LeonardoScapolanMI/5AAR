using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class InputFieldSubmit : MonoBehaviour
{
    public static string[] destinationCoordinates = new string[2] {"45.516826", "9.216668"};
    public InputField inputField;
    public Text settingsState;

    public void Awake()
    {
        inputField.placeholder.GetComponent<Text>().text = destinationCoordinates[0]+","+destinationCoordinates[1];
    }
    public void LockInput(InputField inputField)
    {
        settingsState.text = "Finished";
        destinationCoordinates = inputField.text.Split(',');
    }
    public void Start()
	{
		inputField.onEndEdit.AddListener(delegate{LockInput(inputField);});
	}

}