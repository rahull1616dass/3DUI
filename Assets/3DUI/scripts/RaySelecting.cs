using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RaySelecting : MonoBehaviour
{
    [SerializeField] private float translationIncrement = 0.1f;
    [SerializeField] private float rotationIncrement = 1.0f;
    [SerializeField] private float thumbstickDeadZone = 0.5f;  // a bit of a dead zone (make it less sensitive to axis movement)
    [SerializeField] private string rayCollisionLayer = "Default";
    [SerializeField] private bool PickedUpObjectPositionNotControlledByPhysics = true; //otherwise object position will be still computed by physics engine, even when attached to ray

    private InputDevice righHandDevice;
    private GameObject rightHandController;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonPressedPrevFrame = false;
    private bool ButtonGripWasPressed = false;
    private GameObject objectPickedUP = null;
    private GameObject objectMarked = null;
    private GameObject previousObjectCollidingWithRay = null;
    [SerializeField] private GameObject lastObjectCollidingWithRay = null;
    private bool IsThereAnewObjectCollidingWithRay = false;

    private bool triggerButtonWasPressed = false;
    private bool stickButtonWasPressed = false;
    public static List<GameObject> objectsSelected = new List<GameObject>();

    private ObjectCreator objectCreatorInstance;

    List<GameObject> listOfSelectedObjects = new List<GameObject>();

    /// 
    ///  Events
    /// 

    void Start()
    {
        GetRightHandDevice();
        GetRighHandController();
        GetTrackingSpaceRoot();
        objectCreatorInstance = GetComponent<ObjectCreator>();
    }

    void Update()
    {
        if (objectPickedUP == null)
        {
            GetTargetedObjectCollidingWithRayCasting();
            GetTargetedObjectGrabbingByHand();
            UpdateObjectCollidingWithRay();
            UpdateFlagNewObjectCollidingWithRay();
            OutlineObjectCollidingWithRay();
        }

        MarkAndReMarkTargetObject();

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

    private void GetTargetedObjectCollidingWithRayCasting()
    {
        // see raycast example from https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(transform.position,
            transform.TransformDirection(Vector3.forward),
            out RaycastHit hit,
            Mathf.Infinity,
            1 << LayerMask.NameToLayer(rayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            // Debug.Log("Ray collided with:  " + hit.collider.gameObject + " collision point: " + hit.point);
            Debug.DrawLine(hit.point, (hit.point + hit.normal * 2));
            lastRayCastHit = hit;
        }
    }


    private void GetTargetedObjectGrabbingByHand()
    {
        // see raycast example from https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(transform.position,
            transform.TransformDirection(Vector3.forward),
            out RaycastHit hit,
            Mathf.Infinity,
            1 << LayerMask.NameToLayer(rayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
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





    private void MarkAndReMarkTargetObject()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool bButtonAPressedCurrFrame))
            {
                if (!bButtonPressedPrevFrame && bButtonAPressedCurrFrame && lastRayCastHit.collider != null)
                {
                    bButtonPressedPrevFrame = true;
                }

                if (!bButtonAPressedCurrFrame && bButtonPressedPrevFrame) // Button was released?
                {
                    objectPickedUP = lastRayCastHit.collider.gameObject;

                    if (listOfSelectedObjects.Contains(objectPickedUP) && listOfSelectedObjects.Count > 0)
                    {
                        PlayAudio();
                        CreateHeptic();
                        UnMarkObject(objectPickedUP);
                        listOfSelectedObjects.RemoveAll(x => x.Equals(objectPickedUP));

                        if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gButtonPressedCurrFrame))
                        {
                            if (!gButtonPressedCurrFrame)
                            {
                                foreach (GameObject actualObject in listOfSelectedObjects)
                                {
                                    actualObject.transform.parent = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        PlayAudio();
                        CreateHeptic();
                        listOfSelectedObjects.Add(objectPickedUP);
                        MarkObject(objectPickedUP);

                        if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool goButtonPressedNow))
                        {
                            if (goButtonPressedNow)
                            {
                                foreach (GameObject actualObject in listOfSelectedObjects)
                                {
                                    actualObject.transform.parent = gameObject.transform;
                                }
                            }
                        }

                    }

                    bButtonPressedPrevFrame = false;
                    objectPickedUP = null;
                }

            }


        }
    }

    private void MarkObject(GameObject actualObject)
    {
        var marker = actualObject.GetComponent<OutlineSelected>();
        if (marker == null) // if not, we will add a component to be able to outline it
        {
            //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
            marker = lastObjectCollidingWithRay.AddComponent<OutlineSelected>();
        }

        if (marker != null)
        {
            marker.enabled = true;
            //Debug.Log("outline new object color"+ lastObjectCollidingWithRay);
        }

    }
    private void UnMarkObject(GameObject actualObject)
    {

        var marker = actualObject.GetComponent<OutlineSelected>();
        if (marker != null)
        {
            marker.enabled = false;
            //Debug.Log("outline new object color"+ previousObjectCollidingWithRay);
        }

    }





    private void CreateHeptic()
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

    private void PlayAudio()
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
