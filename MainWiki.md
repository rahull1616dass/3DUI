# Main Wiki for 3DUI

## Getting Started
Hi! This project used to show case the 3DUI and some basic setup has been done on this project by the HCI chair. The Basic setup consist of 
1. 3D Environment with 3D objects
2. XR-Rig
3. Some basic controller
4. Basic object creating UI

## Software used

1. Unity 2019.4.8f1
2. Visual Studio 2022
3. Oculus Rift application  

## How to use

 - Start the **3dui-getting-started** scene.
 - User can check the help menu with the **Menu** button of the left hand controller
 - User can see the current FPS status e.g. Frame Rate and the system status with the **Y** button of the left hand controller.
 - User can see the basic UI by which we can create and destroy 3D Objects. To see the UI, user needs to click **X** button and to interact with the UI, the user can use controller ray interaction.
 - The **left controller's** joystick the user to move around the Virtual Environment.
 - The main move direction is going to control via Left hand controller. For example, if user wants to moves upwards, then user needs to point upwards with the **left hand controller** and push the **left controller's joystick** forward to move.
 - If the user just want to rotate then just click the **Left controller Joystick** and then functionality will be changed to rotation and click again to move around through scene.
 - User can also use the **Left controller Trigger** button to control if the movement has to have an acceleration or not.
 - User can **Teleport** to another position. It can simply done by the ball at the end of the left controller Ray. 
 - User can also change the his/her direction after teleport using **Right hand joystick** there is a indicator which will show the angle after rotation.
 - User can place that ball on the surface user wants to move and then click **Grab** button on left controller to teleport to the new position.
 - To select any object in the environment, user simply just need to point to that object and click the **A** button to grab the object.
 - Once the object is grabbed, user can use the **Right controller joystick** to manipulate the object.
 - User can move the **Right hand joystick** *left-right* to rotate the selected object and *up-down* to scale the object.
 - Once the object is grabbed then user simply move around the **right hand controller** to move the object.

## Move around the Virtual environment

- The `Assets\3DUI\scripts\HandSteering.cs` is the code by which we are moving around the scene.
- It is attached with the **Left Hand** of the **XR Rig**
- First it is getting the current hand by the `GetHandDevice()` function. 

   
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
 - Then it is getting the **current hand GameObject** and **XRRig** using `GetHandControllerGameObject` and `GetTrackingSpaceRoot` function.
 - On Update, two main responsible functions are getting called. `getPointCollidingWithRayCasting` by which the script is detecting on which point the left hand ray is colliding and storing that point on `private RaycastHit lastRayCastHit;` so the movement would be respect to that point.
 - The other one is `MoveTrackingSpaceRootWithHandSteering`
 - `{if (handDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool isStickPressedNow))
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
            }` this is used to swap between move around and rotate.
- This is used to disable the acceleration: 

                else if (isTriggerWasPressed) // release
                {
            
                 bModeSpeed = !bModeSpeed;
                 isTriggerWasPressed = false;
                    speedInMeterPerSecond = 1;
                 if (bModeSpeed) Debug.Log("SpeedMode Is ON");
                 else Debug.Log("SpeedMode Is OFF ");
                }

 - The main movement math is:
 

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



## Help menu

- Help menu is comparatively simple
- We have a prefab name **helpmenu** which have a raw image of the menu
- The `Assets\3DUI\scripts\HelpMenuController.cs` is responsible to get the menu in the scene. Script is also attached with the **Left Hand** of the **XRRig**
- The two main functionality of this script is the **Instantiate** the prefab in front of the **Left Hand** and assign the **XRRig** camera to the Canvas.

## Teleport/Jumping through world
- The script `Assets\3DUI\scripts\Jumping.cs` is responsible for the teleportation.
- First it is talking the left and right hand controller by ` getLeftHandDevice(); and 
        getRightHandDevice();`function.
- Then it is getting the **XRRig** using the `getTrackingSpaceRoot();` function and `getIndicatorLocation();` this is used to instantiate the position prefabs.
- On Update it uses 3 function
		1. `getPointCollidingWithRayCasting();`: is used to do the ray-cast and get the hit position of the jumping position.
		2. `MoveTrackingSpaceRootWithJumping();`: is used to do the **Grabing** button and do the **Jumping**. 
		3. `updateIndicatorLocation();`: is used to update the rotation angle after the teleportation.
- This script is also attached with the **left hand** of the **XRRig**

## Object Menu
- The script `Assets\3DUI\scripts\ObjectFactoryMenuController.cs` is responsible for the instantiating the `ObjectFactoryMenu_New` prefab. This script is also attached with the **left hand** of the **XRRig**. The two main functionality of this script is the **Instantiate** the prefab in front of the **Left Hand** and assign the **XRRig** camera to the Canvas.
- The script `Assets\3DUI\scripts\ObjectCreator300.cs` is attached with one of the sub GameObject of the **ObjectFactoryMenu_New** prefab and it is the main responsible to instantiate the **Cylinderprefab**, **CapsulePrefa**, **SpherePrefab** and **CubePrefab**
- Via this script we are also maining a **List** of gameObjects and removing those when user clicks **DestroyAll**

## FPS Display

- The script `Assets\3DUI\scripts\FPSMenuController.cs` is responsible for the instantiating the `FPSMenuLiteFPSCounter` prefab. This script is also attached with the **left hand** of the **XRRig**. The two main functionality of this script is the **Instantiate** the prefab in front of the **Left Hand** and assign the **XRRig** camera to the Canvas. 
- The `LiteFPSController` is attached with the prefab which is showing the FPS.
## Selecting Objects by Ray

- The script `Assets\3DUI\scripts\RayPicking.cs` is responsible for the controlling the object selection on the the VR Environment. This script is also attached with the **Right hand** of the **XRRig**. The two main functionality of this script is the to Select the GameObject which is hit by the **Right Hand Ray** and set the gameObject as a child of the **Right Hand** of the **XRRig**.
- The `MoveTargetedObjectAlongRay();` will be giving user an effect of scaling the object. By changing it's position towards user or backwards from user.
- The `RotateTargetedObjectOnLocalUpAxis` is responsible rotate the object along it's axis.
- And other functions are working same as the other functionalities above like `GetRightHandDevice();
        GetRighHandController();
        GetTrackingSpaceRoot();`

- Also the object selecting will only work if the ray is hitting any object with collider. 
        