using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;



public class ObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject BlockPrefab;
    [SerializeField] private GameObject CylinderPrefab;
    private List<GameObject> BlocksCreated = new List<GameObject>();
    [SerializeField] private GameObject CapsulePrefab;
    [SerializeField] private GameObject SpherePrefab;



    public void CreateBlockPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(BlockPrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }

    public void CreateCylinderPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(CylinderPrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }
    public void CreateCapsulePrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(CapsulePrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }
    public void CreateSpherePrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(SpherePrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }

    public void DestroyAllSelectedBlocks()
    {
        Debug.Log("Object Creator 300 is working fine");
        DestroyMarkedObjects();

    }



    //old Script


    public float translationIncrement = 0.1f;
    public float rotationIncrement = 1.0f;
    public float thumbstickDeadZone = 0.1f; // a bit of a dead zone (make it less sensitive to axis movement)
    public string RayCollisionLayer = "Default";
    public bool PickedUpObjectPositionNotControlledByPhysics = true; //otherwise object position will be still computed by physics engine, even when attached to ray

    private InputDevice righHandDevice;
    private GameObject rightHandController;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonWasPressed = false;
    private GameObject objectPickedUP = null;
    private GameObject objectMarked = null;
    private GameObject previousObjectCollidingWithRay = null;
    public GameObject lastObjectCollidingWithRay = null;
    private bool IsThereAnewObjectCollidingWithRay = false;

    private ObjectCreator ObjectCreationScript;

    static List<GameObject> listOfSelectedObjects = new List<GameObject>();

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
        if (objectPickedUP == null)
        {
            GetTargetedObjectCollidingWithRayCasting();
            GetTargetedObjectGrabbingByHand();
            UpdateObjectCollidingWithRay();
            UpdateFlagNewObjectCollidingWithRay();
            OutlineObjectCollidingWithRay();
        }

        TryMoveObjects();
        TryMoveSelectedObjects();
        TryMoveAlongRayObjects();
        TryRotateObjects();
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
            // Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'",device.name, device.characteristics.ToString()));
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
            1 << LayerMask.NameToLayer(RayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
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
            1 << LayerMask.NameToLayer(RayCollisionLayer))) // 1 << because must use bit shifting to get final mask!
        {
            // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance,Color.yellow);
            // Debug.Log("Ray collided with:  " + hit.collider.gameObject + " collision point: " + hit.point);
            //Debug.DrawLine(hit.point, (hit.point + hit.normal * 2));
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
            if (righHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool bButtonAPressedNow))
            {
                if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed))
                {
                    if (!gripPressed)
                    {
                        if (!bButtonWasPressed && bButtonAPressedNow && lastRayCastHit.collider != null)
                        {
                            bButtonWasPressed = true;
                        }


                        if (!bButtonAPressedNow && bButtonWasPressed) // Button was released?
                        {
                            objectPickedUP = lastRayCastHit.collider.gameObject;

                            if (listOfSelectedObjects.Contains(objectPickedUP) && listOfSelectedObjects.Count > 0)
                            {
                                GenerateSound();
                                GenerateVibrations();
                                UnMarkObject(objectPickedUP);
                                listOfSelectedObjects.RemoveAll(x => x.Equals(objectPickedUP));


                            }
                            else
                            {

                                //Destroy(objectPickedUP);
                                GenerateSound();
                                GenerateVibrations();
                                listOfSelectedObjects.Add(objectPickedUP);
                                foreach (GameObject actualObject in listOfSelectedObjects)
                                {
                                    Debug.Log(actualObject);
                                }

                                MarkObject(objectPickedUP);


                            }

                            bButtonWasPressed = false;
                            objectPickedUP = null;
                        }
                    }

                }
            }


        }
    }

    private void TryMoveObjects()
    {
        if (righHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gButtonPressedNow))
        {
            if (!gButtonPressedNow)
            {
                foreach (GameObject actualObject in listOfSelectedObjects)
                {
                    actualObject.transform.parent = null;

                }
            }
            if (gButtonPressedNow)
            {
                foreach (GameObject actualObject in listOfSelectedObjects)
                {
                    actualObject.transform.parent = gameObject.transform;
                }

            }
        }
    }

    private void TryMoveAlongRayObjects()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed))
            {
                if (triggerPressed)
                {
                    if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
                    {
                        if (thumbstickAxis.y > thumbstickDeadZone || thumbstickAxis.y < -thumbstickDeadZone)
                        {
                            foreach (GameObject actualObject in listOfSelectedObjects)
                            {
                                actualObject.transform.position += transform.TransformDirection(Vector3.forward) * translationIncrement * thumbstickAxis.y;
                                //Debug.Log("Move object along ray: " + objectPickedUP + " axis: " + thumbstickAxis);
                            }
                        }
                    }
                }
            }
        }
    }

    private void TryMoveSelectedObjects()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {

                if (thumbstickAxis.y > thumbstickDeadZone)
                {
                    foreach (GameObject actualObject in listOfSelectedObjects)
                    {
                        actualObject.transform.localScale += new Vector3(0.02f, 0.02f, 0.02f);
                    }
                }
                if (thumbstickAxis.y < -thumbstickDeadZone)
                {
                    foreach (GameObject actualObject in listOfSelectedObjects)
                    {
                        actualObject.transform.localScale += new Vector3(-0.02f, -0.02f, -0.02f);
                    }
                }


            }
        }
    }
    private void TryRotateObjects()
    {
        if (righHandDevice.isValid) // still connected?
        {
            if (righHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {

                if (thumbstickAxis.x > thumbstickDeadZone || thumbstickAxis.x < -thumbstickDeadZone)
                {
                    foreach (GameObject actualObject in listOfSelectedObjects)
                    {
                        actualObject.transform.Rotate(Vector3.up, rotationIncrement * thumbstickAxis.x, Space.Self);
                    }
                }
                //Debug.Log("Rotate Object: " + objectPickedUP + "axis " + thumbstickAxis);

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

    private void DestroyMarkedObjects()
    {
        Debug.LogError("selected objecte versucht zu zerstören");
        foreach (GameObject actualObject in listOfSelectedObjects)
        {
            Destroy(actualObject);
        }

        listOfSelectedObjects.Clear();
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
            Debug.Log("No Audio Source Found!");
        }
    }


}