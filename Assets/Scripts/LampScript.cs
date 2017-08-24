using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

//author: Johanna Barbier, Johanna.Barbier@eleves.ec-nantes.fr, 08/2017

//Handle the lamp activation
public class LampScript : MonoBehaviour,
                          IInputClickHandler
{
    [Tooltip("Object the lamp will look at (needs a MeshRenderer)")]
    public GameObject targetObject;
    [Tooltip("Light connected to lamp")]
    public Light lampLight;

    private MeshRenderer m_renderer = null;
    private Material material = null;
    private bool lampOn = false;

	// Use this for initialization
	void Awake ()
    {
        if (targetObject != null)
        {
            m_renderer = targetObject.GetComponentInChildren<MeshRenderer>();
        }
        material = this.GetComponent<Renderer>().material;
        material.color = Color.white;
    }
	
	// Update is called once per frame
	void Update ()
    { 		
        if (lampOn)
        {
            this.transform.LookAt(m_renderer.bounds.center);
        }
	}

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("inputClicked");
        if (lampOn == false)
        {
            lampOn = true;
            material.color = new Color(1.000f, 0.969f, 0.559f, 1.000f); //pale yellow
            lampLight.enabled = true;
        }
        else //false
        {
            lampOn = false;
            material.color = Color.white;
            lampLight.enabled = false;
        }
    }
}
