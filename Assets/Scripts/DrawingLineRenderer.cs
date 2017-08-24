using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Implement a way of selecting areas of interests by drawing on the 3D model
public class DrawingLineRenderer : MonoBehaviour,
                                   IInputHandler, //for source Up and Down
                                   ISourceStateHandler // for source Detected and Lost
{
    //points of the rendered line
    private List<Vector3> currentPoints = new List<Vector3>();
    private RaycastHit hit;
    private Vector3 handPosition = Vector3.zero;
    //vector to add to handPosition to get replacedHandPosition
    private Vector3 replacingHand = Vector3.zero;
    //the back of the drawing ray, drawOriginPosition -> replacedHandPosition -> 3DModel
    private Vector3 drawOriginPosition = Vector3.zero;
    private Vector3 replacedHandPosition = Vector3.zero;
    //list with all lines
    private List<LineRenderer> lineList = new List<LineRenderer>();

    //Cursor appearing and following the hand when drawing
    public GameObject drawingCursor;
    //Main cursor of the scene
    public InteractiveMeshCursor mainCursor;

    private IInputSource currentInputSource = null;
    private InputEventData currentInputEventData = null;
    private uint currentInputSourceId;

    private bool isInputUp = true;

    public LineRenderer lineStyle;
    private LineRenderer currentLine;

    private Vector3 originalScale = Vector3.zero;
    private Vector3 originalDrawCursorScale = Vector3.zero;

    private int cpt1 = 0; //to name the line for "previous annotation"
    private int cpt2 = 0;

    // Awake() is called once at the very start of the app even if the script is inactive
    void Awake()
    {
        //save the original scale in order to change the line renderer width depending on it
        originalScale = this.transform.localScale;
        originalDrawCursorScale = drawingCursor.transform.localScale;
    }
	
	// Update is called once per frame
    // FixedUpdate is called in a regular timeline, same time between each call, to use for physics
	void FixedUpdate () {

        // bit shift the index of the layer to get a bit mask
        int layerMask = 1 << 8; // the layer 8 is the kern layer

        RaycastHit gazeHitResult = GazeManager.Instance.HitInfo;

        if (!isInputUp)
        {
            Debug.DrawLine(drawOriginPosition, drawOriginPosition + (replacedHandPosition - drawOriginPosition) * 2);

            //get current inputsource Position information if we can
            if (currentInputSource.SupportsInputInfo(currentInputSourceId, SupportedInputInfo.Position))
            {
                currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);
                //Debug.Log("Hand Position " + handPosition);
                //replacing hand close to the user gaze direction to avoid occlusion issues
                replacedHandPosition = handPosition + replacingHand;
                
                //Raycast(originPoint, DIRECTION, etc.)
                if (Physics.Raycast(drawOriginPosition, replacedHandPosition - drawOriginPosition, out hit, Mathf.Infinity, layerMask))
                {
                    //drawingCursor show current drawing position and orientation
                    Vector3 lookForward = -GazeManager.Instance.GazeNormal; //Get the forward vector looking back at camera
                    drawingCursor.transform.position = hit.point + (lookForward * 0.02f);
                    drawingCursor.transform.rotation = Quaternion.LookRotation(Vector3.Lerp(gazeHitResult.normal, lookForward, 0.5f), Vector3.up);
                    drawingCursor.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));

                    if(currentPoints.Count == 1)//first time in the loop
                    {
                        drawingCursor.SetActive(true);
                        mainCursor.SetVisiblity(false);
                    }
                    //to avoid a point repetitions
                    if (currentPoints.Count != 0 && currentPoints[currentPoints.Count - 1] != hit.point)
                    {
                        currentPoints.Add(hit.point + hit.normal * 0.0015f);
                    }
                    //drawing the line
                    if (currentPoints.Count != 0)
                    {
                        //setting the points of the line one by one
                        //using this instead of setPositions because it's more logical to use list here
                        currentLine.positionCount = currentPoints.Count;
                        for (int i = 0; i < currentPoints.Count; i++)
                        {
                            currentLine.SetPosition(i, currentPoints[i]);
                        }
                    }
                }
                else //no collision found between drawOriginPosition, handPosition and 3Dmodel
                {
                    drawingCursor.transform.position = replacedHandPosition + Vector3.Normalize(replacedHandPosition - drawOriginPosition) *1.5f;
                }
            }
        }
    }

    void OnDisable()
    {
        //hide all the line renderer created
        for(int i=0; i< lineList.Count; i++)
        {
            lineList[i].gameObject.SetActive(false);
        }
        if(cpt2 == 0)
        {
            //no line added
            if(cpt1 != 0)
            {
                cpt1--;
            }
        }
        cpt2 = 0; //reinitialize cpt2
    }

    void OnEnable()
    {
        cpt1++;
    }

    //IMPLEMENTING INTERFACES
    //Implementing IInputHandler
    public void OnInputUp(InputEventData eventData)
    {
        //Debug.Log("Input Up");
        isInputUp = true;
        drawingCursor.SetActive(false);
        mainCursor.SetVisiblity(true);
        currentInputSource = null;

        // Remove self as a modal input handler
        InputManager.Instance.PopModalInputHandler();

    }
    public void OnInputDown(InputEventData eventData)
    {
        //check if the source down detected is the hand
        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            return;
        }

        //Debug.Log("Input Down");

        //start a new line
        cpt2++;
        currentLine = Instantiate(lineStyle, this.transform, true);
        currentLine.name = "line" + cpt1.ToString() + "_" + cpt2.ToString();
        //add the newly created line ID to the list
        lineList.Add(currentLine);
        currentLine.positionCount = 0;
        //set the width of the line depending on the current scale
        currentLine.widthMultiplier = (lineStyle.widthMultiplier * this.transform.localScale.x) / originalScale.x;
        currentPoints.Clear();

        //save this eventData (the user hand) as currentInputSource
        currentInputSource = eventData.InputSource;
        currentInputEventData = eventData;
        currentInputSourceId = eventData.SourceId;

        isInputUp = false;

        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);

        //point hit by gaze ray
        RaycastHit gazeHitResult = GazeManager.Instance.HitInfo;

        currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);
        //calculate drawing ray origin at each new down input behind the user gaze on gaze direction
        drawOriginPosition = Camera.main.transform.position - Vector3.Normalize(gazeHitResult.point
            - Camera.main.transform.position) * 4; //4 or other ratio depending on what's the steadiest
        //set the drawing cursor depending on the scale
        drawingCursor.transform.localScale = (originalDrawCursorScale * this.transform.localScale.x) / originalScale.x;

        //calculate vector needed to go from handPosition to replacedHandPosition in the direction of the gaze
        replacingHand = Vector3.Project(handPosition - drawOriginPosition, Camera.main.transform.position - drawOriginPosition)
                        - (handPosition - drawOriginPosition);
        replacedHandPosition = handPosition + replacingHand;
            
        int layerMask = 1 << 8; // the layer 8 is the kern layer
        Physics.Raycast(drawOriginPosition, replacedHandPosition - drawOriginPosition, out hit, Mathf.Infinity, layerMask);
        //add the first point of the current line
        currentPoints.Add(hit.point + hit.normal * 0.0015f);
    }

    //Implementing ISourceStateHandler
    public void OnSourceDetected(SourceStateEventData eventData)
    {
        //Debug.Log("Source Detected");
    }
    public void OnSourceLost(SourceStateEventData eventData)
    {
        //Debug.Log("Source Lost");
        //if the source lost is the current hand source
        if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
        {
            //get the main cursor back if not the case
            mainCursor.SetVisiblity(true);
            mainCursor.OnInputUp(currentInputEventData);
            drawingCursor.SetActive(false);

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
        }
    }
}
