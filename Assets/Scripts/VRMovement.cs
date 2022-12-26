
/*
VR Movement script 
Property of DovIndustries Inc.
*/


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.IO;
using Oculus.VR;



/*
VRMovement contains all the logic to move the player based on HMD
and controller input.
*/
public class VRMovement : MonoBehaviour
{
    public GameObject Head;
    public GameObject LeftHand, RightHand;
    public OVRPose trackerPose = OVRPose.identity;

    private Vector3 leftControllerPosition, rightControllerPosition;
    private Quaternion leftControllerRotation, rightControllerRotation;
    
    private Vector3 rightControllerVelocity, leftControllerVelocity;

    private Vector3 rightControllerAcceleration, leftControllerAcceleration;

    private float leftControllerAngle, rightControllerAngle;
    private Vector3 leftControllerAxis, rightControllerAxis;

    private Vector3 hmdPosition;
    private float hmdAngle;
    private Vector3 hmdAxis;
    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 yVectorDirection;
    private Vector3 currDirection;
    private OVRDisplay ovrDisplay;

    [SerializeField] float playerSpeed = 2.0f; // increasing this value increases the movement speed
    [SerializeField] float maxForwardAngle = 110.0f;  // maximum angle hmd points relative to forward walking motion. If exceeded, camera moves backwards.
    [SerializeField] float smoothTime = 0.03f;
    [SerializeField] float minWalkingVelocity = 0.23f;

    // For Debugging
    StreamWriter writer; 
    int time;


	// private void Awake()
	// {
	// 	OVRCameraRig rig = GameObject.FindObjectOfType<OVRCameraRig>();
	// 	if (rig != null)
	// 		rig.UpdatedAnchors += OnUpdateOrientAnchors;
	// }

	// private void OnUpdateOrientAnchors(OVRCameraRig rig)
	// {
	// 	if (enabled)
    //     {
    //         // OVRPose pose = new OVRPose();
    //         // // pose.position = new Vector3(1, 2, 3);
    //         // pose.orientation = Quaternion.Euler(0, 127.9f, 0);
    //         // rig.trackingSpace.FromOVRPose(pose, true);
    //     }
	// }

    private void Awake()
    {
        ovrDisplay = new OVRDisplay();

        // For Debugging
        writer = new StreamWriter("C:\\Users\\tova\\Desktop\\output\\output.txt");  
        time = 0;
    }

    private void Update()
    {
        // Calculate position and rotation components of HMD
        Vector3 hmdPosition = InputTracking.GetLocalPosition(XRNode.Head);
        Quaternion hmdRotation = InputTracking.GetLocalRotation(XRNode.Head);
        hmdRotation.ToAngleAxis(out hmdAngle, out hmdAxis);
        
        // Calculate horizontal velocity components of HMD 
        float hmdVelocityX = ovrDisplay.velocity.x;
        float hmdVelocityZ = ovrDisplay.velocity.z;

        // Calculate horizontal acceleration components of HMD 
        float hmdAccelerationX = ovrDisplay.acceleration.x;
        float hmdAccelerationZ = ovrDisplay.acceleration.z;
        
        // Seperate horizontal position components of HMD 
        float hmdPositionX = hmdPosition.x;
        float hmdPositionY = hmdPosition.y;
        float hmdPositionZ = hmdPosition.z;
        
        if (OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch)) {
            leftControllerPosition =  OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            leftControllerRotation.ToAngleAxis(out leftControllerAngle, out leftControllerAxis);
            leftControllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
            leftControllerAcceleration = OVRInput.GetLocalControllerAcceleration(OVRInput.Controller.LTouch);
        }

        if (OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch)) {
            rightControllerPosition =  OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            rightControllerRotation.ToAngleAxis(out rightControllerAngle, out rightControllerAxis);
            rightControllerVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            rightControllerAcceleration = OVRInput.GetLocalControllerAcceleration(OVRInput.Controller.RTouch);
        }


        // Vector3 leftHandPosition = InputTracking.GetLocalPosition(XRNode.LeftHand);



        yVectorDirection = getEqualizedYVector(hmdVelocityX);

        // Calculate the direction the player should move towards
        currDirection = Vector3.Cross(yVectorDirection, ovrDisplay.velocity).normalized;

        if ( isSideToSideMovementFastEnough(hmdVelocityX, hmdVelocityZ) )
        {
            if ( isPlayerInForwardFacingAngle(-currDirection) ) 
            {
                Debug.DrawRay(yVectorDirection, -currDirection*50, Color.red);  // horizontal velocity
                Debug.DrawRay(yVectorDirection, Camera.main.transform.forward*50, Color.green); // direction the hmd is facing
                Debug.DrawLine(-currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays

                stepForward(currDirection);
            }
            else
            {
                Debug.DrawRay(yVectorDirection, currDirection*50, Color.red);  // horizontal velocity
                Debug.DrawRay(yVectorDirection, Camera.main.transform.forward*50, Color.green); // direction the hmd is facing
                Debug.DrawLine(currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays
                
                stepForward(-currDirection);
            }
        }
        time++; // for testing
    }

    private void stepForward(Vector3 currDirection) 
    {
        Vector3 targetPosition = Head.transform.position + (-currDirection) * playerSpeed * Time.deltaTime;
        Head.transform.position = Vector3.SmoothDamp(Head.transform.position, targetPosition, ref cameraVelocity, smoothTime);
    }

    private bool isPlayerInForwardFacingAngle(Vector3 currDirection)
    {
        return Vector3.Angle(Camera.main.transform.forward, currDirection) <= maxForwardAngle;
    }

    private Vector3 getEqualizedYVector(float velocityX)
    {                    
        if (velocityX > 0) 
        {
            // velocity to the right
            return Vector3.up;
        }
        else if (velocityX < 0)
        {
            // velocity to the left
            return -Vector3.up;
        }
        return Vector3.zero;
    }

    private bool isSideToSideMovementFastEnough(float velocityX, float velocityZ)
    {
        return  velocityX >= minWalkingVelocity ||
                velocityX <= -minWalkingVelocity ||
                velocityZ >= minWalkingVelocity ||
                velocityZ <= -minWalkingVelocity;
    }

    /*
    Closes the writing stream
    */
    private void OnApplicationQuit()
    {
        writer.Close();
    }

}
