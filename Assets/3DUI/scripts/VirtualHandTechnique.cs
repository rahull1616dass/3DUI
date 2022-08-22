using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualHandTechnique : MonoBehaviour
{
    public float translationIncrement = 0.1f;
    public float rotationIncrement = 1.0f;
    public float thumbstickDeadZone = 0.5f;  // a bit of a dead zone (make it less sensitive to axis movement)
    public string RayCollisionLayer = "Default";
    public bool PickedUpObjectPositionNotControlledByPhysics = true; //otherwise object position will be still computed by physics engine, even when attached to ray

    private InputDevice righHandDevice;
    private GameObject rightHandController;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;
    private GameObject objectPickedUP = null;
    private GameObject previousObjectCollidingWithRay = null;
    private GameObject lastObjectCollidingWithRay = null;
    private bool IsThereAnewObjectCollidingWithHand = false;



    private Color ColorBeforeChanges;
    public GameObject CollidedObject;

    /// 
    ///  Events
    /// 

    void Start()
    {
        GetRightHandDevice();
        GetRighHandController();
        GetTrackingSpaceRoot();

    }

    void Update()
    {

        if (CollidedObject != null)
        {
            TryToGrapObject();
        }


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

    /// 
    ///  Update Functions 
    /// 


    private void OnTriggerEnter(Collider other)
    {
        CollidedObject = other.gameObject;
        var objectRenderer = CollidedObject.GetComponent<Renderer>();
        ColorBeforeChanges = objectRenderer.material.GetColor("_Color");
        Debug.Log(ColorBeforeChanges);
        objectRenderer.material.SetColor("_Color", Color.grey);
        // Debug.Log("Es collidet");
    }
    void OnTriggerExit(Collider other)
    {
        var controllerRenderer = rightHandController.gameObject.GetComponent<Renderer>();
        controllerRenderer.material.SetColor("_Color", Color.grey);
        var objectRenderer = CollidedObject.GetComponent<Renderer>();
        objectRenderer.material.SetColor("_Color", ColorBeforeChanges);
        // Debug.Log("Es collidet");
    }
    public void MakeRed()
    {
        var objectRenderer = CollidedObject.GetComponent<Renderer>();
        var controllerRenderer = rightHandController.gameObject.GetComponent<Renderer>();
        objectRenderer.material.SetColor("_Color", Color.red);
        controllerRenderer.material.SetColor("_Color", Color.red);
    }


    private void TryToGrapObject()
    {


        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonPressedNow))
            {
                if (!bButtonWasPressed && gripButtonPressedNow)
                {
                    bButtonWasPressed = true;
                }
                if (bButtonWasPressed && gripButtonPressedNow)
                {
                    MakeRed();
                    GenerateSound();
                    GenerateVibrations();

                    objectPickedUP = CollidedObject;
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
                if (!gripButtonPressedNow && bButtonWasPressed) // Button was released?
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
                    bButtonWasPressed = false;
                }
            }
        }
    }



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
            Debug.LogError("No Audio Source Found!");
        }
    }

}
