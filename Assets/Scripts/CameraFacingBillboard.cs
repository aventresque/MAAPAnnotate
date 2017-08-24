using UnityEngine;
using System.Collections;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 07/2017

//Attach this script to objects you want to have facing the camera = the user's head
public class CameraFacingBillboard : MonoBehaviour
{
    void Update()
    {
        Vector3 targetPosition = new Vector3(Camera.main.transform.position.x,
            this.transform.position.y, Camera.main.transform.position.z);
        transform.LookAt(targetPosition);
        transform.Rotate(0, 180, 0);
    }
}