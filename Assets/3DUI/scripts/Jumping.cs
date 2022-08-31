using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Jumping : MonoBehaviour
{
    [SerializeField] private string RayCollisionLayer = "Default";

    private InputDevice handDeviceLeft, handDeviceRight;
    private GameObject handControllerGameObject;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;
    private bool triggerButton;
    private bool isTriggerPressedOnCurrFrame;

    [SerializeField] private GameObject newPos, oldPos;
    [SerializeField] private GameObject blackSqr;


    /// 
    ///  Events
    ///  

    void Start()
    {
        getLeftHandDevice();
        getRightHandDevice();
        getLeftHandController();
        getTrackingSpaceRoot();
        getIndicatorLocation();
    }

    void Update()
    {
        getPointCollidingWithRayCasting();
        MoveTrackingSpaceRootWithJumping();
        updateIndicatorLocation();
    }


    /// 
    ///  Start Functions (to get VR Devices)
    /// 


    private void getLeftHandDevice()
    {
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

    private void getRightHandDevice()
    {
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
             | InputDeviceCharacteristics.Right
             | InputDeviceCharacteristics.Controller;

        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);

        foreach (var device in rightHandedControllers)
        {
            Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",
                device.name, device.characteristics.ToString()));
            handDeviceRight = device;
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


    /// 
    ///  Update Functions 
    ///

    private void getIndicatorLocation()
    {
        newPos = Instantiate(newPos, lastRayCastHit.point, Quaternion.identity);
        oldPos = Instantiate(oldPos, lastRayCastHit.point, Quaternion.identity);
    }

    Vector2 thumbstickAxisValue; //  where left (-1.0,0.0), right (1.0,0.0), up (0.0,1.0), down (0.0,-1.0)

    private void updateIndicatorLocation()
    {
        if (handDeviceRight.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
        {


            if (thumbstickAxisValue.x > 0.9f)
            {
                newPos.transform.Rotate(Vector3.up);
            }
            if (thumbstickAxisValue.x < -0.9f)
            {
                newPos.transform.Rotate(Vector3.down);
            }
        }


        newPos.transform.position = lastRayCastHit.point;


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


    private void MoveTrackingSpaceRootWithJumping()
    {
        if (handDeviceLeft.isValid)
        {
            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressedOnCurrFrame))
            {
                if (handDeviceLeft.TryGetFeatureValue(CommonUsages.gripButton, out triggerButton))
                {
                    if (!bButtonWasPressed && triggerButton && lastRayCastHit.collider != null &&!isTriggerPressedOnCurrFrame)
                    {
                        bButtonWasPressed = true;
                    }
                    if (!triggerButton && bButtonWasPressed && !isTriggerPressedOnCurrFrame)
                    {
                        bButtonWasPressed = false;

                        PlayAudio();
                        CreateHaptic();

                        FadeBlackOutSquare();
                    }
                }
            }
        }
    }

    public async void FadeBlackOutSquare(int fadeSpeed = 5)
    {
        blackSqr.SetActive(true);
        Color objectColor = blackSqr.GetComponent<SpriteRenderer>().color;
        float fadeAmount;


        while (blackSqr.GetComponent<SpriteRenderer>().color.a < 1)
        {
            fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackSqr.GetComponent<SpriteRenderer>().color = objectColor;
            await Task.Yield();
        }
        oldPos.transform.position = trackingSpaceRoot.transform.position;
        oldPos.transform.rotation = trackingSpaceRoot.transform.rotation;
        trackingSpaceRoot.transform.position = lastRayCastHit.point;
        trackingSpaceRoot.transform.rotation = newPos.transform.rotation;
        while (blackSqr.GetComponent<SpriteRenderer>().color.a > 0)
        {
            fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackSqr.GetComponent<SpriteRenderer>().color = objectColor;
            await Task.Yield();
        }
        blackSqr.SetActive(false);
    }

    public void PlayAudio()
    {

    }

    private void CreateHaptic()
    {
        HapticCapabilities hapticCapabilities;
        if (handDeviceLeft.TryGetHapticCapabilities(out hapticCapabilities))
        {
            if (hapticCapabilities.supportsImpulse)
            {
                uint channel = 0;
                float amplitude = 0.5f;
                float duration = 0.5f;
                handDeviceLeft.SendHapticImpulse(channel, amplitude, duration);
                gameObject.GetComponents<AudioSource>()[0].Play();
            }
        }
    }
}
