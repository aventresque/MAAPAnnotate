using UnityEngine;
using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI; //Required when Using UI elements

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Rotate the stone around its middle X and Y axis depending on the
//hand movement right - left or up - down in the user's space
public class HandRotate : MonoBehaviour,
                          IInputHandler,
                          ISourceStateHandler
{
    [Tooltip("Main cursor of the scene")]
    public InteractiveMeshCursor mainCursor;
    [Tooltip("Object to rotate")]
    public GameObject targetObject;

    private IInputSource currentInputSource = null;
    private uint currentInputSourceId;

    private bool isRotating = false;
    InputEventData inputEventData = null;
    
    private Vector3 refCamUp = Vector3.zero;
    private Vector3 refCamRight = Vector3.zero;
    private Vector3 prevHandPosition = Vector3.zero;
    private Vector3 currHandPosition = Vector3.zero;

    // Use this for initialization
    void Start () {
        if (targetObject == null)
        {
            targetObject = this.gameObject;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (isRotating)
        {
            //get the current hand position
            inputEventData.InputSource.TryGetPosition(currentInputSourceId, out currHandPosition);
            //Debug.Log("prev and curr hand position " + prevHandPosition + " " + currHandPosition);

            float magnitude = Vector3.Magnitude(currHandPosition - prevHandPosition);
            Debug.Log("magnitude " + magnitude);

            //project the translation vector onto the camera forward and right direction
            Vector3 translationUp = Vector3.Project(currHandPosition - prevHandPosition, refCamUp);
            Vector3 translationRight = Vector3.Project(currHandPosition - prevHandPosition, refCamRight);

            float orientationUp = Vector3.Magnitude(translationUp);
            float orientationRight = Vector3.Magnitude(translationRight);

            float directionUp = Vector3.Dot(Vector3.Normalize(translationUp), refCamUp);
            float directionRight = Vector3.Dot(Vector3.Normalize(translationRight), refCamRight);

            //rescaling targetObject depending on direction of hand movement
            if (Math.Abs(directionUp + 1.0f) < 0.001f) //direction = 1 => rotate up
            {
                orientationUp = -orientationUp * 1.3f;
            }
            if (Math.Abs(directionRight + 1.0f) < 0.001f) //direction = -1 => rotate down
            {
                orientationRight = -orientationRight * 1.3f;
            }

            //Debug.Log("targetOrientationUp " + orientationUp + " targetOrientationRight " + orientationRight);
            
            //apply rotation around x axis and through targetObject center
            targetObject.transform.RotateAround(targetObject.GetComponentInChildren<MeshRenderer>().bounds.center,
                    refCamRight, orientationUp*150);
            targetObject.transform.RotateAround(targetObject.GetComponentInChildren<MeshRenderer>().bounds.center,
                    refCamUp, - orientationRight*150);

            prevHandPosition = currHandPosition;
        }
    }

    public void OnInputUp(InputEventData eventData)
    {
        if (currentInputSource != null &&
            eventData.SourceId == currentInputSourceId)
        {
            isRotating = false;
            Debug.Log("inputUp");
            currentInputSource = null;
            inputEventData = null;

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        if (isRotating)
        {
            // We're already handling drag input, so we can't start a new drag operation.
            return;
        }

        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            // The input source must provide positional data for this script to be usable
            return;
        }
        Debug.Log("inputDownAfter");
        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;
        inputEventData = eventData;
        isRotating = true;

        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);

        eventData.InputSource.TryGetPosition(currentInputSourceId, out prevHandPosition);
        Vector3 refCamForward = Camera.main.transform.forward; // = Vector3.Normalize(GazeManager.Instance.HitInfo.point - Camera.main.transform.position)
        refCamUp = Camera.main.transform.up;
        refCamRight = Vector3.Cross(refCamUp, refCamForward);
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        // Nothing to do
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
        {
            Debug.Log("sourceLost");
            isRotating = false;

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();

            mainCursor.OnInputUp(inputEventData);
            inputEventData = null;
            currentInputSource = null;
        }
    }
}
