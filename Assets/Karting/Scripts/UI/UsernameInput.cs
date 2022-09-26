/*
 * UsernameInput.cs: Used in the intro menu, to capture a user's name input.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsernameInput : MonoBehaviour
{
    private TMPro.TMP_InputField usernameInput;
    private Button startButton;
    // Start is called before the first frame update
    void Start()
    {
        usernameInput = GameObject.Find("UsernameInputField").GetComponent<TMPro.TMP_InputField>();
        startButton = GameObject.Find("StartButton").GetComponent<Button>();

        usernameInput.onValueChanged.AddListener(value => ToggleStartButton());
    }
 
    //Enables/Disables the Play (start) button when there is a username entered/not-entered
    public void ToggleStartButton()
    {
        //Cap username at Length of 6.
        if(string.IsNullOrWhiteSpace(usernameInput.text) || usernameInput.text.Length > 6)
        {
            startButton.interactable = false;
        }

        else
        {
            startButton.interactable = true;
            Player.Username = usernameInput.text;
        }
    }
}
