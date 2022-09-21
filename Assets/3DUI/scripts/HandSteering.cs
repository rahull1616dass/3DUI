using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandSteering: MonoBehaviour
{
    
    public string RayCollisionLayer = "Default";
    
    public float speedInMeterPerSecond = 1;
    public float CurrentSpeedInMeterPerSecond = 0;
    public float angleInDegreePerSecond = 25;
    public float anglePerClick = 45;
    public float CurveValue = 0;
    private int RayDistance;

    private InputDevice handDevice;
    private GameObject handController;
    private GameObject trackingSpaceRoot;
    private bool bModeSnapRotation;
    private bool bModeSpeed;
    private bool isStickWasPressed;
    private bool isTriggerWasPressed;
    private bool isTriggerPressedNow;
    private RaycastHit lastRayCastHit;

    public AnimationCurve highSpeedModeAccelerationCurve;
    public float MinSpeed = 5;
    public float MaxSpeed= 50;


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
        getPointCollidingWithRayCasting();
    }

    //--------------------------------------------------------

    private void GetHandDevice()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

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

    private void getPointCollidingWithRayCasting()
    {
        // see raycast example from https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(transform.position,
            transform.TransformDirection(Vector3.forward),
            out RaycastHit hit,
            Mathf.Infinity,
            1 << LayerMask.NameToLayer(RayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // Debug.Log("Ray collided with:  " + hit.collider.gameObject + " collision point: " + hit.point);
            Debug.DrawLine(hit.point, (hit.point + hit.normal * 2));
            lastRayCastHit = hit;
        }
    }

    private void MoveTrackingSpaceRootWithHandSteering()  // simple - with no strafing 
    {
        if (handDevice.isValid) // still connected?
        {
            // check if smooth or snap rotation mode
            // see https://docs.unity3d.com/Manual/xr_input.html
            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool isStickPressedNow))
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

            // check if slow or speed mode
            if (handDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressedNow))
            {
                if (isTriggerPressedNow)
                {
                   // speedInMeterPerSecond = 5;
                    isTriggerWasPressed = true;

                }
                else if (isTriggerWasPressed) // release
                {
            
                 bModeSpeed = !bModeSpeed;
                 isTriggerWasPressed = false;
                    speedInMeterPerSecond = 1;
                 if (bModeSpeed) Debug.Log("SpeedMode Is ON");
                 else Debug.Log("SpeedMode Is OFF ");
                }
            }


                // see https://docs.unity3d.com/Manual/xr_input.html
                Vector2 thumbstickAxisValue; //  where left (-1.0,0.0), right (1.0,0.0), up (0.0,1.0), down (0.0,-1.0)

            if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
            {
                // Translate front/back Moving

                if (bModeSpeed)
                {

                    //Debug.Log(highSpeedModeAccelerationCurve.Evaluate(Time.time));

                   if (thumbstickAxisValue.y < 0.3 & thumbstickAxisValue.y > -0.3)
                        {
                            highSpeedModeAccelerationCurve = AnimationCurve.Linear(Time.time, MinSpeed, Time.time + 5.0f, MaxSpeed);
                        }
                        else
                        {
                            trackingSpaceRoot.transform.position +=
                            handController.transform.forward * (highSpeedModeAccelerationCurve.Evaluate(Time.time) * Time.deltaTime * thumbstickAxisValue.y);  
                        } 
                }

                else
                {

                   trackingSpaceRoot.transform.position +=
                    handController.transform.forward * (speedInMeterPerSecond  * lastRayCastHit.distance * Time.deltaTime * thumbstickAxisValue.y);

                }
                //// Translate Left/right Moving

                



                if (bModeSnapRotation)
                {
                     //do something here (Exercise tasks

                    if (thumbstickAxisValue.x < -0.9f) 
                    {
                       trackingSpaceRoot.transform.Rotate(Vector3.up, -anglePerClick);
                    }
                  

                    if (thumbstickAxisValue.x > 0.9f) 
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
