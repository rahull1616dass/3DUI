using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GrabThroughAir : MonoBehaviour
{

    private InputDevice handDeviceLeft;
    private GameObject trackingSpaceRoot;
    private GameObject handControllerGameObject;


    private bool bButtonWasPressed = false;
    private Vector3 OldPosition;
    public int ScaleValue;
    public int Scale = 1;
    public int velocitas = 7;
    private bool isTriggerPressedNow;



    // Start is called before the first frame update
    void Start()
    {
        getLeftHandDevice();
        getLeftHandController();
        getTrackingSpaceRoot();
    }

    // Update is called once per frame
    void Update()
    {
        MoveTrackingSpaceRootWithGrabbing();
         ScaleUp();
    }

    
    private void getLeftHandDevice()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
                                     | InputDeviceCharacteristics.Left
                                     | InputDeviceCharacteristics.Controller;

        var leftHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);

        foreach (var device in leftHandedControllers)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            handDeviceLeft = device;
        }
    }

    private void getTrackingSpaceRoot()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        trackingSpaceRoot = XRRig.rig; // Gameobject representing the center of tracking space in virtual enviroment
    }

    private void getLeftHandController()
    {
        handControllerGameObject = this.gameObject; // i.e. with this script component and an XR controller component
    }


    private void MoveTrackingSpaceRootWithGrabbing()
    {
        if (handDeviceLeft.isValid)
        {
            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressedNow))
            {
                if (handDeviceLeft.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton))
                {

                    // Debug.Log("Gerät erkannt");

                    if (!bButtonWasPressed && gripButton && isTriggerPressedNow)
                    {
                        OldPosition = handControllerGameObject.transform.position;
                        bButtonWasPressed = true;
                        //  Debug.Log("Grabbing! ");

                    }

                    if (!gripButton && bButtonWasPressed && isTriggerPressedNow)
                    {
                        bButtonWasPressed = false;

                        // Debug.Log("Not Grabbing! ");

                    }

                    if (bButtonWasPressed && gripButton && isTriggerPressedNow)
                    {
                        GenerateVibrations();
                        trackingSpaceRoot.transform.position -=
                            (handControllerGameObject.transform.position - OldPosition) * velocitas;

                        OldPosition = handControllerGameObject.transform.position;
                        //  Debug.Log("Schleife 3! ");
                    }

                }
            }
        }
    }


    private void ScaleUp()
    {
        if (handDeviceLeft.isValid)
        {
            Vector2 thumbstickAxisValue;

            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressedNow))
            { 
                if (handDeviceLeft.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
                {
                    //Debug.Log("passt alles ");
                    if (thumbstickAxisValue.y > 0.9 && isTriggerPressedNow)
                    {

                        Debug.Log("größer ");
                        trackingSpaceRoot.transform.localScale += trackingSpaceRoot.transform.localScale * Time.deltaTime * Scale;
                    }

                    if (thumbstickAxisValue.y < -0.9 && isTriggerPressedNow)
                    {
                        Debug.Log("kleiner ");
                        trackingSpaceRoot.transform.localScale -= trackingSpaceRoot.transform.localScale * Time.deltaTime * Scale;
                    }


                }
            }

        }
    }
    private void GenerateVibrations()
    {
        HapticCapabilities capabilities;
        if (handDeviceLeft.TryGetHapticCapabilities(out capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                uint channel = 0;
                float amplitude = 0.5f;
                float duration = 0.2f;
                handDeviceLeft.SendHapticImpulse(channel, amplitude, duration);
            }
        }
    }


}