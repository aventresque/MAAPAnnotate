using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Not working now, it should handle the activation of a virtual keyboard in the application
//when there is a need to correct the dictation (speech to text).
public class KeyboardHoloLens : MonoBehaviour {

    UnityEngine.TouchScreenKeyboard keyboard;
    public static string keyboardText = "";

    public void OpenKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("hello", TouchScreenKeyboardType.Default);
        Debug.Log("Is keyboard active:");
        //Instantiate(TouchScreenKeyboard.area);
    }
    
	// Update is called once per frame
	void Update () {
        if (TouchScreenKeyboard.visible == false && keyboard != null)
        {
            if (keyboard.done == true)
            {
                keyboardText = keyboard.text;
                keyboard = null;
            }
        }
    }
}
