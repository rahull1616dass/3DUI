using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GrabthroughtAair : MonoBehaviour
{

    private InputDevice handDeviceLeft;
    private GameObject trackingSpaceRoot;
    private GameObject handControllerGameObject;



    private bool gButtonWasPressed = false;
    public Vector3 OldPosition;
    public int ScaleValue;
    public int Scale = 5;



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
            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton))
            {
                if (!gButtonWasPressed && gripButton)
                {
                    gButtonWasPressed = true;
                    Debug.Log("Grabbing! ");
                    OldPosition = handControllerGameObject.transform.position;

                }
                if (!gripButton && gButtonWasPressed)
                {
                    gButtonWasPressed = false;

                    Debug.Log("Not Grabbing! ");
                }
                if (gButtonWasPressed && gripButton)
                {

                    trackingSpaceRoot.transform.position -=
                    (handControllerGameObject.transform.position - OldPosition) * ScaleValue;

                    OldPosition = handControllerGameObject.transform.position;
                }

            }






        }

    }

    private void ScaleUp()
    {
        Vector2 thumbstickAxisValue;
        if (handDeviceLeft.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
        {

            if (thumbstickAxisValue.y < 0.9)
            {
                trackingSpaceRoot.transform.localScale += trackingSpaceRoot.transform.localScale * Time.deltaTime * Scale;
            }

            if (thumbstickAxisValue.y > -0.9)
            {
                trackingSpaceRoot.transform.localScale -= trackingSpaceRoot.transform.localScale * Time.deltaTime * Scale;
            }


        }
    }
}
