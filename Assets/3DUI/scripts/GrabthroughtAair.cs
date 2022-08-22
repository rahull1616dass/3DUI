using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GrabthroughtAair : MonoBehaviour
{

    private InputDevice handDeviceLeft;
    private GameObject trackingSpaceRoot;
    private GameObject handControllerGameObject;



    private bool bButtonWasPressed = false;
    [SerializeField] private Vector3 OldPosition;
    [SerializeField] private int ScaleValue;
    [SerializeField] private int Scale = 1;
    [SerializeField] private int velocity;
    private bool isTriggerPressedOnCurrFrame;
    private bool gripButton;



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
            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressedOnCurrFrame))
            {
                if (handDeviceLeft.TryGetFeatureValue(CommonUsages.gripButton, out gripButton))
                {
                    if (!bButtonWasPressed && gripButton && isTriggerPressedOnCurrFrame)
                    {
                        bButtonWasPressed = true;
                        Debug.Log("Grabbing! ");
                        OldPosition = handControllerGameObject.transform.position;

                    }
                    if (!gripButton && bButtonWasPressed && isTriggerPressedOnCurrFrame)
                    {
                        bButtonWasPressed = false;

                        Debug.Log("Not Grabbing! ");
                    }
                    if (bButtonWasPressed && gripButton && isTriggerPressedOnCurrFrame)
                    {

                        trackingSpaceRoot.transform.position -=
                        (handControllerGameObject.transform.position - OldPosition) * velocity;

                        OldPosition = handControllerGameObject.transform.position;
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
            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressedOnCurrFrame))
            {
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
    }
}
