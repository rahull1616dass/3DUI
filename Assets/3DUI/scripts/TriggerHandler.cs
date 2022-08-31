using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHandler : MonoBehaviour
{

    public GameObject collidedObject;
    public GameObject controllerObject;
    public bool collided = false;
    private Color ColorBeforeChanges;

    private RayPickerMod MainPickingScript;


    // Start is called before the first frame update
    void Start()
    {
        MainPickingScript = controllerObject.GetComponentInChildren<RayPickerMod>();
    }

    // Update is called once per frame
    void Update()
    {

    }



    private void OnTriggerEnter(Collider other)
    {
        if (!MainPickingScript.colliderLocked)
        {
            collidedObject = other.gameObject;
        }

        if (MainPickingScript.objectActualPicked == false)
        {
            MainPickingScript.MakeGrey();
            // var objectRenderer = collidedObject.GetComponent<Renderer>();
            // ColorBeforeChanges = objectRenderer.material.GetColor("_Color");
            // Debug.Log(ColorBeforeChanges);
            // objectRenderer.material.SetColor("_Color", Color.grey); 
            //Debug.Log("Es collidet"); 
        }




        collided = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (MainPickingScript.objectActualPicked == false)
        {
            MainPickingScript.MakeOriginalColor();
            // var objectRenderer = collidedObject.GetComponent<Renderer>();
            //                objectRenderer.material.SetColor("_Color", ColorBeforeChanges);
            // Debug.Log("Es collidet");  

        }



        collided = false;
    }


}
