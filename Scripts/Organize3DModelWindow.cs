using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Organize the position of all the panels, menus and lamps when scaling the stone
public class Organize3DModelWindow : MonoBehaviour
{
    [Tooltip("Will organize window depending on this object scale")]
    public GameObject sizableObject;
    public List<GameObject> windowObjects;

    private Vector3 prevScale = Vector3.zero;
    private Vector3 currScale = Vector3.zero;
    //height, width and depth of 3D model for a scale of (1,1,1)
    private Vector3 originalBoxSize = Vector3.zero;

    // Use this for initialization
    void Awake()
    {
        prevScale = sizableObject.transform.localScale;
        //height, width and depth of 3D model
        originalBoxSize = sizableObject.GetComponentInChildren<MeshCollider>().bounds.size / prevScale.x;
        //Debug.Log("originalBoxSize " + originalBoxSize);
    }

    // Update is called once per frame
    void Update()
    {
        currScale = sizableObject.transform.localScale;
        //when scale change reorganise window
        if (currScale != prevScale)
        {
            ReorganizeWindow();
            prevScale = currScale;
        }
    }

    //function called when scale of sizableObject change
    public void ReorganizeWindow()
    {
        float changeX = originalBoxSize.x * (currScale.x - prevScale.x) / 2.0f;
        float changeY = originalBoxSize.y * (currScale.y - prevScale.y) / 2.0f;
        float changeZ = originalBoxSize.z * (currScale.z - prevScale.z) / 2.0f;

        foreach(GameObject myObject in windowObjects)
        {
            if(myObject.name == "Buttons" || myObject.name == "MainMenu") //to the left of 3D model
            {
                myObject.transform.localPosition = new Vector3(myObject.transform.localPosition.x - changeX,
                    myObject.transform.localPosition.y + changeY, myObject.transform.localPosition.z);
            }
            else if(myObject.name == "ButtonOptions") //to the right of the 3D model
            {
                myObject.transform.localPosition = new Vector3(myObject.transform.localPosition.x + changeX,
                    myObject.transform.localPosition.y + changeY, myObject.transform.localPosition.z);
            }
            else if(myObject.name == "AnnotationFrame" || myObject.name == "Lamp")
            {
                if(myObject.transform.localPosition.x > 0.0f) // to the right
                {
                    myObject.transform.localPosition = new Vector3(myObject.transform.localPosition.x + changeX,
                        myObject.transform.localPosition.y + changeY, myObject.transform.localPosition.z);
                }
                else //to the left
                {
                    myObject.transform.localPosition = new Vector3(myObject.transform.localPosition.x - changeX,
                        myObject.transform.localPosition.y + changeY, myObject.transform.localPosition.z);
                }
            }
        }
    }
}
