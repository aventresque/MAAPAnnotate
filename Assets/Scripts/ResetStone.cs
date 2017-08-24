using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Reset the GameObject (3D model) orientation back towards the current user's head position
public class ResetStone : MonoBehaviour {

    public GameObject targetObject;
    public GameObject targetObject2;
    
    private Vector3 posStartValue;

    private void Awake()
    {
        if (targetObject == null)
        {
            targetObject = this.gameObject;
        }
        if(targetObject2 != null)
        {
            posStartValue = targetObject2.transform.localPosition;
        }
    }

    // Reset GameObject rotation and scale
    public void Reset()
    {
        //billboard towards camera
        targetObject.transform.LookAt(new Vector3(Camera.main.transform.position.x,
            targetObject.transform.position.y, Camera.main.transform.position.z));
        targetObject.transform.Rotate(0, 180, 0);
        if (targetObject2 != null)
        {
            targetObject2.transform.localPosition = posStartValue;
            targetObject2.transform.LookAt(Camera.main.transform.position);
        }
    }
}
