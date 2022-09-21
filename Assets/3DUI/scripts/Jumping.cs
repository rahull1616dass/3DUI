using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Jumping: MonoBehaviour
{
    public string RayCollisionLayer = "Default";

    private InputDevice handDeviceLeft;
    private InputDevice handDeviceRight;
    private GameObject handControllerGameObject;
    private GameObject trackingSpaceRoot; 
   
    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;
    public GameObject NewPosition;
    public GameObject OldPosition;

    public GameObject blackOutSquare;
    

    
   
        

    /// 
    ///  Events
    ///  

    void Start()
    {
        getLeftHandDevice();
        getLeftHandController();
        getRightHandDevice();
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

    private void getRightHandDevice()
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
        NewPosition = Instantiate(NewPosition, lastRayCastHit.point, Quaternion.identity);
        OldPosition = Instantiate(OldPosition, lastRayCastHit.point, Quaternion.identity);
    }

    Vector2 thumbstickAxisValue; //  where left (-1.0,0.0), right (1.0,0.0), up (0.0,1.0), down (0.0,-1.0)


 
    
    
    private void updateIndicatorLocation()
    {
        if (handDeviceRight.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickAxisValue))
        {

       
            if (thumbstickAxisValue.x > 0.9f)
            {
                NewPosition.transform.Rotate(Vector3.up);
            }
            if (thumbstickAxisValue.x < -0.9f)
            {
                NewPosition.transform.Rotate(Vector3.down);
            }
        }       


        NewPosition.transform.position = lastRayCastHit.point;


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
            if (handDeviceLeft.TryGetFeatureValue(CommonUsages.triggerButton, out bool isTriggerPressedNow))
            {
                if (handDeviceLeft.TryGetFeatureValue(CommonUsages.gripButton, out bool triggerButton))
                {
                    if (!bButtonWasPressed && triggerButton && lastRayCastHit.collider != null && !isTriggerPressedNow)
                    {
                        bButtonWasPressed = true;
                    }
                    if (!triggerButton && bButtonWasPressed && !isTriggerPressedNow)
                    {
                        bButtonWasPressed = false;

                       
                        GenerateVibrations();

                        StartCoroutine(FadeBlackOutSquare());
                        OldPosition.transform.position = trackingSpaceRoot.transform.position;
                        OldPosition.transform.rotation = trackingSpaceRoot.transform.rotation;
                        trackingSpaceRoot.transform.position = lastRayCastHit.point;
                        trackingSpaceRoot.transform.rotation = NewPosition.transform.rotation;
                        

                        Debug.Log("Jumping! " + Time.deltaTime);
                    }
                }
            }
        }
    }
 

    public IEnumerator FadeBlackOutSquare(int fadeSpeed = 5)
    {
        
        blackOutSquare.SetActive(true);
        Color objectColor = blackOutSquare.GetComponent<SpriteRenderer>().color;
        float fadeAmount;

       
            while (blackOutSquare.GetComponent<SpriteRenderer>().color.a < 1)
            {
                
                
                
                fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);
                
                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<SpriteRenderer>().color = objectColor;
                yield return null;
            }
           // OldPosition.transform.position = trackingSpaceRoot.transform.position;
            //OldPosition.transform.rotation = trackingSpaceRoot.transform.rotation;
            //trackingSpaceRoot.transform.position = lastRayCastHit.point;
            //trackingSpaceRoot.transform.rotation = NewPosition.transform.rotation;
            while (blackOutSquare.GetComponent<SpriteRenderer>().color.a > 0)
            {

                fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<SpriteRenderer>().color = objectColor;
                
                yield return null;
               
            }
        blackOutSquare.SetActive(false);
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
               float duration = 0.5f;
               handDeviceLeft.SendHapticImpulse(channel, amplitude, duration);
               gameObject.GetComponents<AudioSource>()[0].Play(); 
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
