﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Jumping : MonoBehaviour
{
    public string RayCollisionLayer = "Default";

    private InputDevice handDevice;
    private GameObject handControllerGameObject;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;


    /// 
    ///  Events
    ///  

    void Start()
    {
        getRightHandDevice();
        getRighHandController();
        getTrackingSpaceRoot();
    }

    void Update()
    {
        getPointCollidingWithRayCasting();
        MoveTrackingSpaceRootWithJumping();
    }


    /// 
    ///  Start Functions (to get VR Devices)
    /// 


    private void getRightHandDevice()
    {
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
             | InputDeviceCharacteristics.Left
             | InputDeviceCharacteristics.Controller;

        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);

        foreach (var device in rightHandedControllers)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            handDevice = device;
        }
    }

    private void getTrackingSpaceRoot()
    {
        var XRRig = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.XRRig>(); // i.e Roomscale tracking space 
        trackingSpaceRoot = XRRig.rig; // Gameobject representing the center of tracking space in virtual enviroment
    }

    private void getRighHandController()
    {
        handControllerGameObject = this.gameObject; // i.e. with this script component and an XR controller component
    }


    /// 
    ///  Update Functions 
    ///

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


    private void MoveTrackingSpaceRootWithJumping()
    {
        if (handDevice.isValid)
        {
            if (handDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool triggerButton))
            {
                if (!bButtonWasPressed && triggerButton && lastRayCastHit.collider != null)
                {
                    bButtonWasPressed = true;
                }
                if (!triggerButton && bButtonWasPressed)
                {
                    bButtonWasPressed = false;
                    trackingSpaceRoot.transform.position = lastRayCastHit.point;
                    Debug.Log("Jumping! " + Time.deltaTime);
                }
            }
        }
    }
}
