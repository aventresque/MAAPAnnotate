using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 08/2017

//Write from InputField to Text
public class TextFromInputField : MonoBehaviour
{
    [Tooltip("Text where you write what's in the input field")]
    public Text mainText;

    private InputField mainInputField;

    void Awake()
    {
        mainInputField = this.GetComponent<InputField>();
    }

    //write inputField in the text to which this script is attached
    public void WriteInputField()
    {
        mainText.text = mainInputField.text;
    }   
}
