using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 08/2017

//Implement the two ways of selecting areas with a draggable rectangle
//Option1 = Rectangle Selection projects the rectangle as a quad on the stone
//          by casting a ray through each corner of the rectangle in the user's forward direction
//Option2 = Rectangle Projection projects the surface of the rectangle on the stone
//          and create a corresponding mesh on the stone
public class SelectingArea : MonoBehaviour,
                             IInputHandler, //for source Up and Down
                             ISourceStateHandler // for source Detected and Lost
{
    [Tooltip("Object we want to select an area on")]
    public GameObject targetObject;
    [Tooltip("Material to assign to the selection quad")]
    public Material mat_Quad;
    [Tooltip("Main cursor of the scene")]
    public InteractiveMeshCursor mainCursor;
    [Tooltip("Cursor Dot for rectangle start point")]
    public GameObject dotRect;

    private IInputSource currInputSource = null;
    private uint currInputSourceId;

    private GameObject quad; //a primitive quad is two triangles
    private MeshFilter m_Quad;
    private bool isHolding = false;
    //is currHandPosition top right || bottom left compared to firstHandPosition, for the normals of the quad
    private bool isTRBL = true;

    private Vector3 firstHitPoint = Vector3.zero;
    private Vector3 forwardDir = Vector3.zero;
    private Vector3 upDir = Vector3.zero;
    private Vector3 rightDir = Vector3.zero;

    private Vector3 firstHandPosition = Vector3.zero;
    private Vector3 currHandPosition = Vector3.zero;

    private Vector3 originalScale = Vector3.zero;
    private Vector3 originalDotScale = Vector3.zero;

    private Vector3 diffCamX = Vector3.zero;
    private Vector3 diffCamY = Vector3.zero;

    private int option = 1; //two options with rectangle
    //list with all quad
    private List<GameObject> quadList = new List<GameObject>();
    //list with all meshObj
    private List<GameObject> meshObjList = new List<GameObject>();
    private int quadCpt1 = 0; //to name the quad for "previous annotation"
    private int quadCpt2 = 0;
    private int meshCpt1 = 0; //to name the meshObj for "previous annotation"
    private int meshCpt2 = 0;

    // Awake is called once at the very beginning of the play
    void Awake()
    {
        //save the original scale in order to change the interval between raycasts when projecting
        originalScale = this.transform.localScale;
        originalDotScale = dotRect.transform.localScale;
    }

    void Start ()
    {
        //create the quad
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        m_Quad = quad.GetComponent<MeshFilter>();
        quad.GetComponent<MeshRenderer>().material = mat_Quad;
        quad.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        quad.SetActive(false);
    }

	// Update is called once per frame
	void Update () {
		if(isHolding)
        {
            currInputSource.TryGetPosition(currInputSourceId, out currHandPosition);

            UpdateQuadVertices();
        }
	}

    public void SetOption(int opt)
    {
        option = opt;
    }

    //project quad on targetObject
    //option 1 create a quad on the four corners hit
    private void RectangleSelection()
    {
        quadCpt2++; //quadCpt2 counting the number of quads created

        // bit shift the index of the layer to get a bit mask
        int layerMask = 1 << 8; // the layer 8 is the kern layer

        GameObject currentQuad = Instantiate(quad, this.transform, true); //duplicate the quad
        currentQuad.name = "quad" + quadCpt1.ToString() + "_" + quadCpt2.ToString();
        //add the newly created quad to the list
        quadList.Add(currentQuad);
        Destroy(currentQuad.GetComponent<Collider>());//remove collider
        quad.SetActive(false);

        Vector3[] projectPts = new Vector3[4];
        RaycastHit hit;

        //raycast only in one direction, to be sure to be in front of the rock ratio 0.5f
        if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f, forwardDir, out hit, Mathf.Infinity, layerMask))
        {
            currentQuad.transform.position = hit.point + hit.normal * 0.01f; //and we keep the forward of the first quad
            projectPts[0] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f + diffCamX + diffCamY, forwardDir, out hit, Mathf.Infinity, layerMask))
        {
            projectPts[1] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        else //if we don't hit the stone
        {
            bool foundHit = false;
            //find a hit closer to the firstHitPoint
            for(float flagX = 0.0f; flagX <= 1.0f; flagX += 0.025f)
            {
                for(float flagY = 0.0f; flagY <= 1.0f; flagY += 0.025f)
                {
                    if(Physics.Raycast(firstHitPoint - forwardDir * 0.5f + (1 - flagX) * diffCamX + (1 - flagY) * diffCamY, forwardDir, out hit, Mathf.Infinity, layerMask))
                    {
                        //if we hit we break
                        foundHit = true;
                        break;
                    }
                }
                if (foundHit)
                {
                    break;
                }
            }
            projectPts[1] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f + diffCamY, forwardDir, out hit, Mathf.Infinity, layerMask))
        {
            projectPts[3] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        else //if we don't hit the stone
        {
            //find a hit closer to the firstHitPoint
            for (float flagY = 0.0f; flagY <= 1.0f; flagY += 0.025f)
            {
                if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f + (1 - flagY) * diffCamY, forwardDir, out hit, Mathf.Infinity, layerMask))
                {
                    //if we hit we break
                    break;
                }
            }
            projectPts[3] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f + diffCamX, forwardDir, out hit, Mathf.Infinity, layerMask))
        {
            projectPts[2] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        else //if we don't hit the stone
        {
            //find a hit closer to the firstHitPoint
            for (float flagX = 0.0f; flagX <= 1.0f; flagX += 0.025f)
            {
                if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f + (1 - flagX) * diffCamX, forwardDir, out hit, Mathf.Infinity, layerMask))
                {
                    //if we hit we break
                    break;
                }
            }
            projectPts[2] = currentQuad.transform.InverseTransformPoint(hit.point + hit.normal * 0.01f);
        }
        // remove the dot cursor showing the rectangle start
        dotRect.SetActive(false);
        //update the mesh vertices
        currentQuad.GetComponent<MeshFilter>().mesh.vertices = projectPts;
    }

    //project quad on targetObject
    //option 2 raycast uniformaly in the rectangle to get 3D model mesh vertices and triangles to create own mesh
    private void RectangleProjection()
    {
        meshCpt2++; //meshCpt2 counting the number of meshes created
        quad.SetActive(false);

        // bit shift the index of the layer to get a bit mask
        int layerMask = 1 << 8; // the layer 8 is the kern layer
        
        //create the new object that will receive the build mesh
        GameObject currentObj = new GameObject("meshObj" + meshCpt1.ToString() + "_" + meshCpt2.ToString()); //directly name it
        currentObj.transform.position = firstHitPoint;
        currentObj.transform.forward = forwardDir;
        //add the newly created mesh to the list
        meshObjList.Add(currentObj);

        //new mesh vertices and tris
        List<Vector3> projectedVerts = new List<Vector3>();
        List<int> projectedTris = new List<int>();
        RaycastHit hit;

        //set the change of scale to set the intervals accordingly
        float scaleChange = this.transform.localScale.x / originalScale.x;

        //interval between 2 raycast in X and Y direction depending on the current scale
        float interX = 0.005f * scaleChange * Mathf.Pow(Vector3.Magnitude(diffCamX), -0.811f);
        float interY = 0.005f * scaleChange * Mathf.Pow(Vector3.Magnitude(diffCamY), -0.811f);

        //Debug.Log("interX and Y " +interX + " " + interY);
        Debug.Log("magX and Y " + Vector3.Magnitude(diffCamX) + " " + Vector3.Magnitude(diffCamY));
        //raycast on the horizontal line
        for (float Nx = 0.0f; Nx <= 1.0f; Nx += interX)
        {   //raycast on the vertical line
            for (float Ny = 0.0f; Ny <= 1.0f; Ny += interY)
            {
                //raycast only in one direction, ratio 0.5f to be sure to be in front of the rock
                if (Physics.Raycast(firstHitPoint - forwardDir * 0.5f + Nx * diffCamX + Ny * diffCamY, forwardDir, out hit, Mathf.Infinity, layerMask))
                {
                    MeshCollider meshCollider = hit.collider as MeshCollider;
                    if (meshCollider == null || meshCollider.sharedMesh == null)
                        return;

                    Mesh mesh = meshCollider.sharedMesh;
                    Vector3[] vertices = mesh.vertices;
                    int[] triangles = mesh.triangles;
                                        
                    if (projectedTris.Count == 0) //first raycast
                    {
                        //add the triangle hit to the vertices
                        projectedVerts.Add(currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 0]] + hit.normal * 0.005f)));
                        projectedVerts.Add(currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 1]] + hit.normal * 0.005f)));
                        projectedVerts.Add(currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 2]] + hit.normal * 0.005f)));
                        //give the new triangle vertices position
                        projectedTris.Add(projectedVerts.Count - 3);
                        projectedTris.Add(projectedVerts.Count - 2);
                        projectedTris.Add(projectedVerts.Count - 1);
                    }
                    else
                    {   //avoid repetitions
                        bool addPoint = true;
                        for(int ind=0; ind < projectedVerts.Count / 3.0f; ind++)
                        {
                            //if this triangle is already in our list
                            if (currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 0]] 
                                + hit.normal * 0.005f)) == projectedVerts[ind * 3 + 0]
                            && currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 1]]
                            + hit.normal * 0.005f)) == projectedVerts[ind * 3 + 1]
                            && currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 2]]
                            + hit.normal * 0.005f)) == projectedVerts[ind * 3 + 2])
                            {
                                addPoint = false;
                                break; //break out of the loop "for"
                            }
                        }
                        if (addPoint)
                        {
                            //add the triangle hit to the vertices
                            projectedVerts.Add(currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 0]] + hit.normal * 0.005f)));
                            projectedVerts.Add(currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 1]] + hit.normal * 0.005f)));
                            projectedVerts.Add(currentObj.transform.InverseTransformPoint(meshCollider.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 2]] + hit.normal * 0.005f)));
                            //give the new triangle vertices position
                            projectedTris.Add(projectedVerts.Count - 3);
                            projectedTris.Add(projectedVerts.Count - 2);
                            projectedTris.Add(projectedVerts.Count - 1);
                        }
                    }
                }
            }
        }
        MeshFilter currMeshFilter = currentObj.AddComponent<MeshFilter>();
        currentObj.AddComponent<MeshRenderer>().material = mat_Quad;

        // remove the dot cursor showing the rectangle start
        dotRect.SetActive(false);

        //set the new mesh vertices and triangles
        currMeshFilter.mesh.SetVertices(projectedVerts);
        currMeshFilter.mesh.SetTriangles(projectedTris, 0); //we only have one material so one submesh
    }

    //update the vertices of the quad depending on the new hand position
    private void UpdateQuadVertices()
    {
        //Debug.Log("firshandposition " + firstHandPosition + "currhandposition " + currHandPosition);

        Vector3 diff = currHandPosition - firstHandPosition;
        diffCamX = Vector3.Project(diff, rightDir);
        diffCamY = Vector3.Project(diff, upDir);

        quad.transform.position = firstHitPoint - forwardDir * 0.05f;
        quad.transform.forward = forwardDir;

        Vector3[] verts = m_Quad.mesh.vertices;
        verts.SetValue(quad.transform.InverseTransformPoint(firstHitPoint - forwardDir * 0.05f), 0);
        verts.SetValue(quad.transform.InverseTransformPoint(firstHitPoint - forwardDir * 0.05f + diffCamX + diffCamY), 1);
        verts.SetValue(quad.transform.InverseTransformPoint(firstHitPoint - forwardDir * 0.05f + diffCamY), 3);
        verts.SetValue(quad.transform.InverseTransformPoint(firstHitPoint - forwardDir * 0.05f + diffCamX), 2);

        float dirX = Vector3.Dot(Vector3.Normalize(diffCamX), rightDir);
        float dirY = Vector3.Dot(Vector3.Normalize(diffCamY), upDir);

        //Debug.Log("dirX and dirY " + dirX + " " + dirY);
        if(!isTRBL //if we weren't in TRBL before
            && ((Mathf.Abs(dirX + 1.0f) < 0.001f && Mathf.Abs(dirY + 1.0f) < 0.001f) //bottom left dirX == -1.0f && dirY == -1.0f
            || (Mathf.Abs(dirX - 1.0f) < 0.001f && Mathf.Abs(dirY - 1.0f) < 0.001f))) //top right dirX == 1.0f && dirY == 1.0f
        {
            FlipQuadNormals();
            isTRBL = true;
        }
        else if (isTRBL // if we were in TRBL before
            && ((Mathf.Abs(dirX + 1.0f) < 0.001f && Mathf.Abs(dirY - 1.0f) < 0.001f) //top left dirX == -1.0f && dirY == 1.0f
            || (Mathf.Abs(dirX - 1.0f) < 0.001f && Mathf.Abs(dirY + 1.0f) < 0.001f))) //bottom right dirX == 1.0f && dirY == -1.0f
        {
            FlipQuadNormals();
            isTRBL = false;
        }
        if (dirX == 0.0f && dirY == 0.0f) //first time currHandPosition = firstHandPosition
        {
            //doing nothing
        }

        //apply changes
        m_Quad.mesh.vertices = verts;
    }

    private void FlipQuadNormals()
    {
        int[] tris = m_Quad.mesh.triangles;
        //reverse triangle winding order
        for (int ind = 0; ind < tris.Length / 3.0f; ind++)
        {
            int tmp = tris[ind * 3 + 0];
            tris[ind * 3 + 0] = tris[ind * 3 + 1];
            tris[ind * 3 + 1] = tmp;
        }
        m_Quad.mesh.triangles = tris;
    }

    //hide all the projectmesh created
    public void HideMeshObjList()
    {
        for (int i = 0; i < meshObjList.Count; i++)
        {
            meshObjList[i].gameObject.SetActive(false);
        }
    }

    public void DestroyMeshObjList()
    {
        for (int i = 0; i < meshObjList.Count; i++)
        {
            Destroy(meshObjList[i]);
        }
    }

    ////hide all the quads created
    public void HideQuadList()
    {
        for (int i = 0; i<quadList.Count; i++)
        {
            quadList[i].gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        //hide all the quads created
        HideQuadList();
        //hide all the projectmesh created
        DestroyMeshObjList();       
        
        if(option == 1)
        {
            if (quadCpt2 == 0)
            {
                //no line added
                if (quadCpt1 != 0)
                {
                    quadCpt1--;
                }
            }
            quadCpt2 = 0; //reinitialize quadCpt2
        }
        else if(option == 2)
        {
            if (meshCpt2 == 0)
            {
                //no line added
                if (meshCpt1 != 0)
                {
                    meshCpt1--;
                }
            }
            meshCpt2 = 0; //reinitialize quadCpt2
        }
    }

    void OnEnable()
    {
        if (option == 1) quadCpt1++;
        else if (option == 2) meshCpt1++;
    }

    public void OnInputUp(InputEventData eventData)
    {
        if(currInputSource != null && eventData.SourceId == currInputSourceId)
        {
            //Debug.Log("InputUp");
            isHolding = false;
            currInputSource = null;
            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
            //set the maincursor back
            mainCursor.SetVisiblity(true);

            //Project object
            if(option == 1)
            {
                RectangleSelection();
            }
            else if (option == 2)
            {
                RectangleProjection();
            }
            else
            {
                Debug.Log("Error option number should be 1 or 2");
            }
        }                
    }
    public void OnInputDown(InputEventData eventData)
    {
        //Debug.Log("InputDown");
        //hide the main cursor while selecting
        mainCursor.SetVisiblity(false);

        isHolding = true;
        currInputSource = eventData.InputSource;
        currInputSourceId = eventData.SourceId;

        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);

        //save the first hit point
        firstHitPoint = GazeManager.Instance.HitPosition;
        forwardDir = Camera.main.transform.forward;
        upDir = Camera.main.transform.up;
        rightDir = Vector3.Cross(upDir, forwardDir);

        currInputSource.TryGetPosition(currInputSourceId, out firstHandPosition);
        //initialise the currHandPosition
        currHandPosition = firstHandPosition;

        //put a dot cursor where we first hit the stone
        dotRect.SetActive(true);
        dotRect.transform.position = mainCursor.Dot.transform.position;
        dotRect.transform.forward = mainCursor.Dot.transform.forward; // look at user
        dotRect.transform.localScale = (originalDotScale * this.transform.localScale.x) / originalScale.x;

        UpdateQuadVertices();
        quad.SetActive(true);
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        //nothing to do
    }
    public void OnSourceLost(SourceStateEventData eventData)
    {
        if(currInputSource != null && currInputSourceId == eventData.SourceId)
        {
            isHolding = false;
            currInputSource = null;
            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
            //set the main cursor back
            mainCursor.SetVisiblity(true);

            //Project object
            if (option == 1)
            {
                RectangleSelection();
            }
            else if (option == 2)
            {
                RectangleProjection();
            }
            else
            {
                Debug.Log("Error option number should be 1 or 2");
            }
        }
    }
}
