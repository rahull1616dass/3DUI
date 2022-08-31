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
    [SerializeField] private bool DeletionModeActivated = false;
    [SerializeField] private bool PickedUpObjectPositionNotControlledByPhysics = true; //otherwise object position will be still computed by physics engine, even when attached to ray

    private InputDevice rightHandDevice;
    private GameObject rightHandController;
    private GameObject trackingSpaceRoot;

    private RaycastHit lastRayCastHit;
    private bool bButtonPressedPrevFrame = false;
    private bool ButtonGripPressedPrevFrame = false;
    private GameObject objectPickedUP = null;
    private GameObject previousObjectCollidingWithRay = null;
    [SerializeField] private GameObject lastObjectCollidingWithRay = null;
    private bool IsThereAnewObjectCollidingWithRay = false;

    private bool triggerButtonWasPressed = false;
    private bool stickButtonWasPressed = false;
    public static List<GameObject> objectsSelected = new List<GameObject>();
    private ObjectCreator objectCreatorInstance;

    /// 
    ///  Events
    /// 
    public void DestroyAllSelectedBlocks()
    {
        Debug.Log("Method will start");
        CreateHeptic(0.5f, 0.5f);
        foreach (GameObject gameObject in objectsSelected)
        {
            Destroy(gameObject);
        }


    }
    void Start()
    {
        GetRightHandDevice();
        GetRighHandController();
        GetTrackingSpaceRoot();
        GetComponent<RayPicking>().enabled = false;
        GetComponent<RayPickerMod>().enabled = false;
        objectCreatorInstance = GetComponent<ObjectCreator>();
    }

    void Update()
    {
        if (objectPickedUP == null)
        {
            GetTargetedObjectCollidingWithRayCasting();
            UpdateObjectCollidingWithRay();
            UpdateFlagNewObjectCollidingWithRay();
            OutlineObjectCollidingWithRay();
            SelectAndDeselectObjects();
        }

        MoveSelectedObjects();
        MoveAndScale();
        RotateTargetObjectOnYAxis();

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
            rightHandDevice = device;
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
        if (IsThereAnewObjectCollidingWithRay && !objectsSelected.Contains(lastObjectCollidingWithRay))
        {
            //add outline to new one
            var outliner = lastObjectCollidingWithRay.GetComponent<OutlineModified>();
            if (lastObjectCollidingWithRay.tag != "VR Controller")
            {
                if (outliner == null) // if not, we will add a component to be able to outline it
                {
                    //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
                    outliner = lastObjectCollidingWithRay.AddComponent<OutlineModified>();
                }

                if (outliner != null)
                {
                    outliner.enabled = true;
                    outliner.OutlineColor = Color.blue;
                    //Debug.Log("outline new object color"+ lastObjectCollidingWithRay);
                }
                // remove outline from previous one
                //add outline new one
                if (previousObjectCollidingWithRay != null && !objectsSelected.Contains(previousObjectCollidingWithRay))
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





    private void OutlineSelectedObjectCollidingWithRay(GameObject gameObject, bool enabled)
    {
        //add outline to new one
        /*if (lastObjectCollidingWithRay != null && objectsSelected.Contains(objectPickedUP))
        {*/

        Debug.Log("OutlineSelectedObject Function started");
        var outliner = gameObject.GetComponent<OutlineModified>();
        if (outliner == null) // if not, we will add a component to be able to outline it
        {
            //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
            outliner = gameObject.AddComponent<OutlineModified>();
        }

        outliner.enabled = enabled;

        if (outliner != null)
        {
            outliner.enabled = true;
            outliner.OutlineColor = Color.green;
            Debug.Log("outline new object color" + lastObjectCollidingWithRay);
        }
    }


    private void SelectAndDeselectObjects()
    {
        if (rightHandDevice.isValid) // still connected?
        {
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool bButtonAPressedNow))
            {
                if (!bButtonPressedPrevFrame && bButtonAPressedNow && lastRayCastHit.collider != null)
                {
                    bButtonPressedPrevFrame = true;
                }

                if (!bButtonAPressedNow && bButtonPressedPrevFrame) // Button was released? 
                {
                    objectPickedUP = lastRayCastHit.collider.gameObject;

                    if (objectsSelected.Contains(objectPickedUP))
                    {
                        DiselectObjects(objectPickedUP);
                    }
                    else
                    {
                        SelectObjects(objectPickedUP);
                    }

                    objectPickedUP = null;
                    bButtonPressedPrevFrame = false; // A Button
                }
            }
        }
    }

    private void MoveSelectedObjects()
    {
        if (rightHandDevice.isValid) // still connected?
        {
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool buttonGripPressedNow))
            {
                if (!ButtonGripPressedPrevFrame && buttonGripPressedNow && lastRayCastHit.collider != null)
                {
                    ButtonGripPressedPrevFrame = true;
                }

                if (!buttonGripPressedNow && ButtonGripPressedPrevFrame) // Button was released? 
                {
                    foreach (GameObject selectedObject in objectsSelected)
                    {
                        Debug.Log("Move Methode was called");
                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                            }
                            outlineGreen();
                        }

                        selectedObject.transform.parent = null;
                    }

                    ButtonGripPressedPrevFrame = false;
                }

                if (buttonGripPressedNow && ButtonGripPressedPrevFrame)
                {
                    foreach (GameObject selectedObject in objectsSelected)
                    {
                        selectedObject.transform.parent = gameObject.transform;

                        if (PickedUpObjectPositionNotControlledByPhysics)
                        {
                            Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = true;
                            }
                            outlineColor();
                        }
                    }
                }
            }
        }
    }

    private void SelectObjects(GameObject gameObject) //Select with A Button
    {
        //neu hinzufügen des Objekts zur Liste
        objectsSelected.Add(gameObject);
        OutlineSelectedObjectCollidingWithRay(gameObject, true);
        this.gameObject.GetComponents<AudioSource>()[1].Play();
    }

    private void DiselectObjects(GameObject gameObject) //deselect current object or remove it from list
    {
        objectsSelected.Remove(gameObject);
        OutlineSelectedObjectCollidingWithRay(gameObject, false);
        this.gameObject.GetComponents<AudioSource>()[2].Play();
    }

    public void DestroyByPressingB() {  //Destroy by pressing B Button    {

        if (DeletionModeActivated)
        {
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bButtonBPressedNow))
            {
                if (bButtonBPressedNow) // if B is pressed, the object that is currently touched should be deleted
                {
                    Destroy(lastObjectCollidingWithRay);
                }
            }
        }
    }

    private void ScaleAllSelectedObjects()
    {
        if (rightHandDevice.isValid) // still connected?
        {
            Vector2 thumbStickAxisValue; //left(-1,0,0,0), right (1,0,0,0), up(0,0,1,0), down(0,0,-1,0)

            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbStickAxisValue))
            {
                if (thumbStickAxisValue.y > 0.9f)
                {
                    this.gameObject.GetComponents<AudioSource>()[0].Play();
                    foreach (GameObject gameObject in objectsSelected)
                    {
                        gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
                    }
                }
                else if (thumbStickAxisValue.y < -0.9f)
                {
                    GetComponents<AudioSource>()[0].Play();
                    foreach (GameObject gameObject in objectsSelected)
                    {
                        gameObject.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                    }
                }
            }
        }
    }

    private void MoveAndScale()
    {
        if (rightHandDevice.isValid) // still connected?
        {
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton))
            {
                //primary2DAxisClick
                if (triggerButton)
                {
                    MoveSelectedObjectsAlongRay();
                }


                if (!triggerButton)
                {
                    ScaleAllSelectedObjects();
                    GetComponents<AudioSource>()[0].Play();
                }
            }
        }
    }

    private void MoveSelectedObjectsAlongRay()
    {
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
        {
            if (thumbstickAxis.y > thumbstickDeadZone ||
                thumbstickAxis.y < -thumbstickDeadZone)
            {
                GetComponents<AudioSource>()[0].Play();
                // Liste Transformen
                foreach (GameObject gameObject in objectsSelected)
                {
                    gameObject.transform.position += transform.TransformDirection(Vector3.forward) * translationIncrement * thumbstickAxis.y;
                    //Debug.Log("Move object along ray: " + objectPickedUP + " axis: " + thumbstickAxis);
                }
            }
        }
    }



    private void RotateTargetObjectOnYAxis()
    {
        if (rightHandDevice.isValid) // still connected?
        {
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickAxis))
            {
                if (thumbstickAxis.x > thumbstickDeadZone || thumbstickAxis.x < -thumbstickDeadZone)
                {
                    foreach (GameObject gameObject in objectsSelected)
                    {
                        gameObject.transform.Rotate(Vector3.up, rotationIncrement * thumbstickAxis.x, Space.Self);
                    }
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


    public void DeletionMode(bool active)
    {
        DeletionModeActivated = true;
    }

    private void CreateHeptic(float amplitude, float duration)
    {
        HapticCapabilities capabilities;
        if (rightHandDevice.TryGetHapticCapabilities(out capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                uint channel = 0;
                rightHandDevice.SendHapticImpulse(channel, amplitude, duration);
            }
        }
    }

    private void PlayAudio()
    {
        GetComponents<AudioSource>()[1].Play();
    }


    private void outlineGreen()
    {

        foreach (GameObject selectedObject in objectsSelected)
        {

            var outliner = selectedObject.GetComponent<OutlineModified>();
            if (outliner == null) // if not, we will add a component to be able to outline it
            {
                //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
                outliner = selectedObject.AddComponent<OutlineModified>();
            }

            outliner.enabled = enabled;


            outliner.OutlineColor = Color.green;
        }
    }

    private void outlineColor()
    {

        foreach (GameObject selectedObject in objectsSelected)
        {

            var outliner = selectedObject.GetComponent<OutlineModified>();
            if (outliner == null) // if not, we will add a component to be able to outline it
            {
                //Debug.Log("Outliner added t" + lastObjectCollidingWithRay.gameObject.ToString());
                outliner = selectedObject.AddComponent<OutlineModified>();
            }

            outliner.enabled = enabled;

            if (rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton))
            {
                //primary2DAxisClick
                if (triggerButton)
                {
                    outliner.OutlineColor = Color.red;
                }
                else
                {
                    outliner.OutlineColor = Color.yellow;
                }
            }
        }
    }



}
