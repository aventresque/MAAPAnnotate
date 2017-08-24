using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Examples.InteractiveElements;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Scale the target GameObject depending on the slider value
//Also change the scale button display and value
public class SliderScale : MonoBehaviour
{
    [Tooltip("Label of the slider")]
    public TextMesh label;
    [Tooltip("Object to scale")]
    public GameObject targetObject;

    //to have both scale option together
    [Tooltip("Label of the scale button")]
    public TextMesh buttonLabel;

    private Vector3 originalScale;
    private SliderGestureControl sliderGestureControl;
    private float prevSliderValue;

    private void Awake()
    {
        if (label == null)
        {
            label = this.gameObject.GetComponentInChildren<TextMesh>();
        }
        sliderGestureControl = GetComponent<SliderGestureControl>();
        originalScale = targetObject.transform.localScale;
        prevSliderValue = sliderGestureControl.SliderValue;
    }
	
	// Update is called once per frame
	void Update ()
    {
        float sliderValue = sliderGestureControl.SliderValue + sliderGestureControl.MinSliderValue;
        //if slider value change
        if (sliderValue != prevSliderValue)
        {
            label.text = "Scale: " + sliderValue.ToString(sliderGestureControl.LabelFormat) + "%";

            //change other scale option's value
            buttonLabel.text = "Scale: " + sliderValue.ToString(sliderGestureControl.LabelFormat) + "%";

            //scale the targetObject
            targetObject.transform.localScale = originalScale * (sliderValue / 100.0f);

            prevSliderValue = sliderValue;
        }
	}
}
