using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandSteering: MonoBehaviour
{
    [SerializeField] private float speedInMeterPerSecond = 1;
    [SerializeField] private float angleInDegreePerSecond = 25;
    [SerializeField] private float anglePerClick = 45;
    [SerializeField] private float currentSpeedInMperS = 0;
    [SerializeField] private float curveVal = 0;


    private InputDevice handDevice;
    private GameObject handController;
    private GameObject trackingSpaceRoot;
    private bool bModeSnapRotation;
    private bool bModeSpeed;
    private bool isStickWasPressed;
    private bool isStickPressedNow;
    private bool isTriggerPressedPrevFrame;
    private bool isTriggerPressedCurrFrame;

    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private float MinSpeed = 5;
    [SerializeField] private float MaxSpeed = 50;




    // aka tracking space's position in virtual environment 
    // i.e a game object positon and orientation is added to the tracking data

    //--------------------------------------------------------

    void Start()
    {
        GetHandDevice();
        GetHandControllerGameObject();
        GetTrackingSpaceRoot();
    }

    void Update()
    {
        MoveTrackingSpaceRootWithHandSteering();
    }

    //--------------------------------------------------------

    private void GetHandDevice()
    {
      
       var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
            | InputDeviceCharacteristics.Left
            | InputDeviceCharacteristics.Controller;

        var controller = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controller);

        foreach (var device in controller)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            handDevice = device;
        }
    }


    private void GetTrackingSpaceRoot()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        trackingSpaceRoot = XRRig.rig; // Gameobject representing the center of tracking space in virtual enviroment
    }


    private void GetHandControllerGameObject()
    {
        handController = this.gameObject; // i.e. with this script component and an XR controller component
    }


    private void MoveTrackingSpaceRootWithHandSteering()  // simple - with no strafing 
    {
        if (handDevice.isValid) // still connected?
        {
            // check if smooth or snap rotation mode
            // see https://docs.unity3d.com/Manual/xr_input.html
            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out isStickPressedNow))
            {
                if (isStickPressedNow)
                {
                    isStickWasPressed = true;
                }
                else if(isStickWasPressed) // release
                {
                    bModeSnapRotation = !bModeSnapRotation;
                    isStickWasPressed = false;
                    if(bModeSnapRotation) Debug.Log("Snap Turning Is ON");
                    else Debug.Log("Snap Turning Is OFF (Smooth Rotation");
                }

            }

            if(handDevice.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressedCurrFrame))
            {
                if (isTriggerPressedCurrFrame) { 
                    isTriggerPressedPrevFrame = true;
                }
                else if(isTriggerPressedPrevFrame)
                {
                    bModeSpeed = !bModeSpeed;
                    isTriggerPressedPrevFrame = false;
                    speedInMeterPerSecond = 1;
                    if (bModeSpeed)
                        Debug.Log("SpeedModeIsOn");
                    else
                        Debug.Log("SpeedModeIsOff");
                }
            }

            Vector2 thumbstickAxisValue;


            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
            {
                // Translate front/back Moving
                if (bModeSpeed)
                {
                    Debug.Log(accelerationCurve.Evaluate(Time.time));
                    if (thumbstickAxisValue.y < 0.3 & thumbstickAxisValue.y > -0.3)
                    {
                        accelerationCurve = AnimationCurve.Linear(Time.time, MinSpeed, Time.time + 5.0f, MaxSpeed);
                    }
                    else
                    {
                        trackingSpaceRoot.transform.position +=
                        handController.transform.forward * (accelerationCurve.Evaluate(Time.time) * Time.deltaTime * thumbstickAxisValue.y);
                    }
                }
                else
                {
                    trackingSpaceRoot.transform.position +=
                   handController.transform.forward * (speedInMeterPerSecond * Time.deltaTime * thumbstickAxisValue.y);
                }


                //// Translate Left/right Moving

                if (bModeSnapRotation)
                {

                    if(thumbstickAxisValue.x < 0.9f)
                    {
                        trackingSpaceRoot.transform.Rotate(Vector3.up, -anglePerClick);
                    }

                    else if(thumbstickAxisValue.x > 0.9f)
                    {
                        trackingSpaceRoot.transform.Rotate(Vector3.down, anglePerClick);
                    }
                }
                else
                {
                    //// Smooth Rotate Left/right Moving
                    ///
                    trackingSpaceRoot.transform.Rotate(Vector3.up, angleInDegreePerSecond * Time.deltaTime * thumbstickAxisValue.x);
                }

            }

        }
    }
}
