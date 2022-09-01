using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RayPickerMod : MonoBehaviour
{
    public float translationIncrement = 0.1f;
    public float rotationIncrement = 1.0f;
    public float thumbstickDeadZone = 0.5f;  // a bit of a dead zone (make it less sensitive to axis movement)
    public string RayCollisionLayer = "Default";
    public bool PickedUpObjectPositionNotControlledByPhysics = true; //otherwise object position will be still computed by physics engine, even when attached to ray

    private InputDevice righHandDevice;
    private GameObject rightHandController;
    private GameObject trackingSpaceRoot;
    public GameObject SelectorVariable;
    public GameObject CollidedObjectMain;
    public GameObject CollidedObjectTemp;
    public Color ColorBeforeChanges;

    public int actualsize;
    public int transformz;

    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;
    private GameObject objectPickedUP = null;
    private GameObject previousObjectCollidingWithRay = null;
    private GameObject lastObjectCollidingWithRay = null;
    private bool IsThereAnewObjectCollidingWithRay = false;
    private TriggerHandler ColissionScript;
    public GameObject selectorObject;
    public bool gribButtonNotPressed = true;
    public bool ObjectNotCollided = true;
    public bool ObjectGreyed = false;
    public bool objectActualPicked = false; 
    public bool colliderLocked = false;

    /// 
    ///  Events
    /// 

    void Start()
    {
        GetRightHandDevice();
        GetRighHandController();
        GetTrackingSpaceRoot();
        ColissionScript = selectorObject.GetComponentInChildren<TriggerHandler>();
        SelectorVariable.SetActive(true);
        GetComponent<XRRayInteractor>().enabled = false;
        GetComponent<XRInteractorLineVisual>().enabled = false;
        GetComponent<RayPicking>().enabled = false;
    }

    void Update()
    {
        if (objectPickedUP == null)
        {

            //   UpdateObjectCollidingWithRay();
            //  UpdateFlagNewObjectCollidingWithRay();
            // OutlineObjectCollidingWithRay();
        }
        // AttachOrDetachTargetedObject();
        // MoveTargetedObjectAlongRay();
        // RotateTargetedObjectOnLocalUpAxis();
        ResizeSelector();
        CollidedObjectMain = ColissionScript.collidedObject;
        IsObjectCollided();
        MoveTargetedObject();

    }


    /// 
    ///  Start Functions (to get VR Devices)
    /// 

    private void GetRightHandDevice()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Right
            | InputDeviceCharacteristics.Controller;

        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);

        foreach (var device in rightHandedControllers)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            righHandDevice = device;
        }
    }
    private void GetTrackingSpaceRoot()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        trackingSpaceRoot = XRRig.rig; // Gameobject representing the center of tracking space in virtual enviroment
    }

    private void GetRighHandController()
    {
        rightHandController = gameObject; // i.e. with this script component and an XR controller component
    }



    private void MakeSelectorLonger()
    {

        SelectorVariable.transform.localScale += new Vector3(0.0f, 0.05f, 0.0f);

    }

    private void MakeSelectorShorter()
    {
        SelectorVariable.transform.localScale -= new Vector3(0.0f, 0.05f, 0.0f);
    }

    //Jetzt wird gefragt, ob ein Object collidet

    private void IsObjectCollided()
    {
        if (CollidedObjectMain != null)
        {
            TryToGrabObject();
            //Debug.Log("Es collidet im hauptscript");
        }



    }



    private void TryToGrabObject()
    {

        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonPressedNow))
            {
                if (!bButtonWasPressed && gripButtonPressedNow)
                {
                    bButtonWasPressed = true;
                    //GenerateSound();
                    GetComponents<AudioSource>()[1].Play();
                    GenerateVibrations();
                    MakeRed();
                    objectActualPicked = true;
                }
                if (bButtonWasPressed && gripButtonPressedNow)
                {
                    objectPickedUP = ColissionScript.collidedObject;
                    objectPickedUP.transform.parent = gameObject.transform; // see Transform.parent https://docs.unity3d.com/ScriptReference/Transform-parent.html?_ga=2.21222203.1039085328.1595859162-225834982.1593000816
                    if (PickedUpObjectPositionNotControlledByPhysics)
                    {
                        Rigidbody rb = objectPickedUP.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                        }
                        objectActualPicked = true;
                    }
                    //Debug.Log("Object Picked up:" + objectPickedUP);
                }
                if (!gripButtonPressedNow && bButtonWasPressed) // Button was released?
                {

                    if (objectPickedUP != null) // already pick up an object?
                    {
                        gameObject.GetComponents<AudioSource>()[2].Play();
                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = objectPickedUP.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                                rb.useGravity = false;
                            }
                        }
                        objectPickedUP.transform.parent = null;
                        objectPickedUP = null;
                        objectActualPicked = false;
                        //Debug.Log("Object released: " + objectPickedUP);
                        CollidedObjectMain = null;
                        ObjectNotCollided = true;
                        MakeOriginalColor();
                        ColissionScript.collidedObject = null;
                    }



                    bButtonWasPressed = false;
                }
            }
        }
    }

    public void MakeRed()
    {
        colliderLocked = false;
        CollidedObjectMain = ColissionScript.collidedObject;
        var objectRenderer = CollidedObjectMain.GetComponent<Renderer>();
        var controllerRenderer = rightHandController.gameObject.GetComponent<Renderer>();
        objectRenderer.material.SetColor("_Color", Color.red);
        controllerRenderer.material.SetColor("_Color", Color.red);

    }

    public void MakeGrey()
    {
        CollidedObjectMain = ColissionScript.collidedObject;
        var objectRenderer = CollidedObjectMain.GetComponent<Renderer>();
        ColorBeforeChanges = objectRenderer.material.GetColor("_Color");
        objectRenderer.material.SetColor("_Color", Color.grey);


    }

    public void MakeOriginalColor()
    {
        var objectRenderer = CollidedObjectMain.GetComponent<Renderer>();
        objectRenderer.material.SetColor("_Color", ColorBeforeChanges);
    }

    /*
    private void GetTargetedObjectCollidingWithRayCasting()
    {
        // see raycast example from https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(transformz.position,
            transformz.TransformDirection(Vector3.forward),
            out RaycastHit hit,
            Mathf.Infinity,
            1 << LayerMask.NameToLayer(RayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            Debug.DrawRay(transformz.position, transformz.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
           // Debug.Log("Ray collided with:  " + hit.collider.gameObject + " collision point: " + hit.point);
            Debug.DrawLine(hit.point, (hit.point + hit.normal * 2));
            lastRayCastHit = hit;
        }
    }


    private void GetTargetedObjectGrabbingByHand()
    {
        // see raycast example from https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(transformz.position,
            transformz.TransformDirection(Vector3.forward),
            out RaycastHit hit,
            Mathf.Infinity,
            1 << LayerMask.NameToLayer(RayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            Debug.DrawRay(transformz.position, transformz.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // Debug.Log("Ray collided with:  " + hit.collider.gameObject + " collision point: " + hit.point);
            Debug.DrawLine(hit.point, (hit.point + hit.normal * 2));
            lastRayCastHit = hit;
        }
    }
    
    private void UpdateObjectCollidingWithRay()
    {
        if (lastRayCastHit.collider != null)
        {
            GameObject currentObjectCollidingWithRay = lastRayCastHit.collider.gameObject;
            if (lastObjectCollidingWithRay != currentObjectCollidingWithRay)
            {
                previousObjectCollidingWithRay = lastObjectCollidingWithRay;
                lastObjectCollidingWithRay = currentObjectCollidingWithRay;
            }
        }
    }
    private void UpdateFlagNewObjectCollidingWithRay()
    {
        if (lastObjectCollidingWithRay != previousObjectCollidingWithRay)
        {
            IsThereAnewObjectCollidingWithRay = true;
        }
        else
        {
            IsThereAnewObjectCollidingWithRay = false;
        }
    }
    private void OutlineObjectCollidingWithRay()
    {
        if (IsThereAnewObjectCollidingWithRay)
        {
            //add outline to new one
            if (lastObjectCollidingWithRay != null)
            {
                var outliner = lastObjectCollidingWithRay.GetComponent<OutlineModified>();
                if (outliner == null) // if not, we will add a component to be able to outline it
                {
                    //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
                    outliner = lastObjectCollidingWithRay.AddComponent<OutlineModified>();
                }

                if (outliner != null)
                {
                    outliner.enabled = true;
                    //Debug.Log("outline new object color"+ lastObjectCollidingWithRay);
                }
                // remove outline from previous one
                //add outline new one
                if (previousObjectCollidingWithRay != null)
                {
                    outliner = previousObjectCollidingWithRay.GetComponent<OutlineModified>();
                    if (outliner != null)
                    {
                        outliner.enabled = false;
                        //Debug.Log("outline new object color"+ previousObjectCollidingWithRay);
                    }
                }
            }

        }
    }

   

    private void AttachOrDetachTargetedObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool bButtonAPressedNow))
            {
                if (!bButtonWasPressed && bButtonAPressedNow && lastRayCastHit.collider != null)
                {
                    bButtonWasPressed = true;
                }
                if (!bButtonAPressedNow && bButtonWasPressed) // Button was released?
                {
                    if (objectPickedUP != null) // already pick up an object?
                    { 
                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = objectPickedUP.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                            }
                        }
                        objectPickedUP.transform.parent = null;
                        objectPickedUP = null;
                        Debug.Log("Object released: " + objectPickedUP);
                    }
                    else
                    {
                        GenerateSound(); 
                        GenerateVibrations();
                        
                        objectPickedUP = lastRayCastHit.collider.gameObject;
                        objectPickedUP.transform.parent = gameObject.transform; // see Transform.parent https://docs.unity3d.com/ScriptReference/Transform-parent.html?_ga=2.21222203.1039085328.1595859162-225834982.1593000816
                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = objectPickedUP.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = true;
                            }
                        }
                        Debug.Log("Object Picked up:" + objectPickedUP);
                    }
                    bButtonWasPressed = false;
                }
            }
        }
    }
*/
    private void MoveTargetedObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP != null) // already picked up an object?
                {
                    if (thumbstickAxis.y > thumbstickDeadZone || thumbstickAxis.y < -thumbstickDeadZone)
                    {
                        objectPickedUP.transform.position += selectorObject.transform.TransformDirection(Vector3.up) * translationIncrement * thumbstickAxis.y;
                        //Debug.Log("Move object along ray: " + objectPickedUP + " axis: " + thumbstickAxis);
                    }
                }
            }
        }
    }

    private void ResizeSelector()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP == null) // already picked up an object?
                {

                    if (thumbstickAxis.y < 0.3)
                    {
                        MakeSelectorShorter();

                    }
                    if (thumbstickAxis.y > -0.3)
                    {
                        MakeSelectorLonger(); 

                    }
                }
            }
        }
    }
    /*
    private void RotateTargetedObjectOnLocalUpAxis()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (objectPickedUP != null) // already pick up an object?
                {
                    if (thumbstickAxis.x > thumbstickDeadZone || thumbstickAxis.x < -thumbstickDeadZone)
                    {
                        objectPickedUP.transform.Rotate(Vector3.up, rotationIncrement * thumbstickAxis.x, Space.Self);
                    }
                    //Debug.Log("Rotate Object: " + objectPickedUP + "axis " + thumbstickAxis);
                }
            }
        }
    }

 
    private void ResizeSelector()
    {
        if (righHandDevice.isValid)
        {
                if (righHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton) && gribButtonNotPressed)
                {
                    if (secondaryButton)
                    {
                        MakeSelectorLonger();
                    }
            }

                if (righHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton))
                {

                    // Debug.Log("Gerät erkannt");

                    if (primaryButton)
                    {
                    MakeSelectorShorter();
                    }


                }
            
        }
    }
 */




    private void GenerateVibrations()
    {
        HapticCapabilities capabilities;
        if (righHandDevice.TryGetHapticCapabilities(out capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                uint channel = 0;
                float amplitude = 0.5f;
                float duration = 1.0f;
                righHandDevice.SendHapticImpulse(channel, amplitude, duration);
            }
        }
    }

    private void GenerateSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            // Debug.LogError("No Audio Source Found!");
        }
    }
}