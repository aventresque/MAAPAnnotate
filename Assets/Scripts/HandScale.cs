using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using HoloToolkit.Examples.InteractiveElements;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Scale the target GameObject depending on the hand
//forward and backward movement in the user's space
public class HandScale : MonoBehaviour,
                         IInputHandler,
                         ISourceStateHandler

{
    [Tooltip("Main cursor of the scene")]
    public InteractiveMeshCursor mainCursor;
    [Tooltip("Object to scale")]
    public GameObject targetObject;
    [Tooltip("Label of the scale button")]
    public TextMesh buttonLabel;
    [Tooltip("Controls the speed at which the object will interpolate toward the desired scale")]
    [Range(0.01f, 1.0f)]
    public float ScaleLerpSpeed = 0.2f;

    //to have both scale option together
    [Tooltip("Label of the scale slider")]
    public TextMesh label;
    [Tooltip("Slider gesture of the scale slider")]
    public SliderGestureControl sliderGestureControl;

    private bool isScaling = false;
    private Vector3 originalScale = Vector3.zero;
    private Vector3 currentScale = Vector3.zero;
    private Vector3 targetScale = Vector3.zero;
    private Vector3 firstHandPosition = Vector3.zero;
    private Vector3 currHandPosition = Vector3.zero;
    private Vector3 firstGazeDirection = Vector3.zero;

    private InputEventData currentInputEventData = null;

    // Use this for initialization
    void Awake()
    {
        if (targetObject == null)
        {
            targetObject = this.gameObject;
        }
        originalScale = targetObject.transform.localScale;
        targetScale = originalScale;
    }

    // Update is called in a regular timeline
    void FixedUpdate()
    {
        //write scale percentage on the button
        float scalePercentage = (targetObject.transform.localScale.x * 100.0f) / originalScale.x;
        buttonLabel.text = "Scale: " + Math.Round(scalePercentage).ToString() + "%";

        if (isScaling)
        {
            //change other scale option's value
            sliderGestureControl.SetSliderValue(scalePercentage - sliderGestureControl.MinSliderValue);
            UpdateHandScale();
        }
    }

    public void UpdateHandScale()
    {
        currentInputEventData.InputSource.TryGetPosition(currentInputEventData.SourceId, out currHandPosition);
        //project the translation vector (firstHandPosition, currHandPosition) onto the gaze direction
        Vector3 translationVector = Vector3.Project(currHandPosition - firstHandPosition,
            Vector3.Normalize(firstGazeDirection));

        float direction = Vector3.Dot(Vector3.Normalize(firstGazeDirection), Vector3.Normalize(translationVector));
        //rescaling targetObject depending on direction of hand movement
        if (Math.Abs(direction - 1.0f) < 0.001f) //direction = 1 => zoom out
        {
            targetScale = currentScale - Vector3.one * Vector3.Magnitude(translationVector) * 1.5f;
        }
        else if (Math.Abs(direction + 1.0f) < 0.001f) //direction = -1 => zoom in
        {
            targetScale = currentScale + Vector3.one * Vector3.Magnitude(translationVector) * 1.5f;
        }
        else if(Math.Abs(direction) < 0.001f) //direction = 0 -> first iteration translation vector = 0.0f
        {
            targetScale = targetObject.transform.localScale;
        }
        //limit the zoom in and zoom out at twice or half the original scale
        if (targetScale.x > originalScale.x * 2) //x = y = z for our scaling
        {
            targetScale = originalScale * 2;
        }
        else if (targetScale.x < originalScale.x / 2)
        {
            targetScale = originalScale / 2;
        }
        //apply scale smoothly with lerp
        targetObject.transform.localScale = Vector3.Lerp(targetObject.transform.localScale, targetScale, ScaleLerpSpeed);
    }

    //IInputHandler implementation
    public void OnInputUp(InputEventData eventData)
    {
        if (currentInputEventData != null && eventData.SourceId == currentInputEventData.SourceId)
        {
            isScaling = false;
            currentInputEventData = null;

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
        }
    }
    public void OnInputDown(InputEventData eventData)
    {
        if (isScaling)
        {
            // We're already handling drag input, so we can't start a new drag operation.
            return;
        }

        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            // The input source must provide positional data for this script to be usable
            return;
        }
        isScaling = true;
        currentInputEventData = eventData;
        currentInputEventData.InputSource.TryGetPosition(currentInputEventData.SourceId, out firstHandPosition);
        //prevHandPosition = firstHandPosition; // initialize prevHandPosition
        firstGazeDirection = GazeManager.Instance.HitPosition - Camera.main.transform.position;
        currentScale = targetObject.transform.localScale;

        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);
    }

    //Implementing ISourceStateHandler
    public void OnSourceDetected(SourceStateEventData eventData)
    {
        //doing nothing
    }
    public void OnSourceLost(SourceStateEventData eventData)
    {
        //if the source lost is the current hand source
        if (currentInputEventData != null && eventData.SourceId == currentInputEventData.SourceId)
        {
            isScaling = false;
            mainCursor.OnInputUp(currentInputEventData);
            currentInputEventData = null;

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
        }
    }
}
