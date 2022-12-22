
// VR Movement script. Property of DovIndustries Inc.
// Unity's HMD movement API: https://docs.unity3d.com/ScriptReference/XR.XRNodeState.html

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
    private List<XRNodeState> mNodeStates = new List<XRNodeState>();
    private Vector3 mHeadPos, mLeftHandPos, mRightHandPos;
    private Vector3 mHeadVelocity, mLeftHandVelocity, mRightHandVeocity, mHeadAngularVelocity;
    private Quaternion mHeadRot, mLeftHandRot, mRightHandRot;

    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 yVectorDirection;
    private Vector3 crossProduct; // the direction the player moves towards

    [SerializeField] float playerSpeed = 2.0f; // increasing this value increases the movement speed
    [SerializeField] float maxForwardAngle = 110.0f;  // the maximum angle the hmd points in relative to the forward walking motion. Once this value is exceeded, the camera moves backwards.
    [SerializeField] Vector3 currDirection;
    [SerializeField] float smoothTime = 0.03f;
    [SerializeField] float minWalkingVelocity = 0.23f;

    StreamWriter writer; // for debugging
    int time; // for debugging

    public OVRPose trackerPose = OVRPose.identity;
    
	private void Awake()
	{
		OVRCameraRig rig = GameObject.FindObjectOfType<OVRCameraRig>();
		if (rig != null)
			rig.UpdatedAnchors += OnUpdateOrientAnchors;
	}

	private void OnUpdateOrientAnchors(OVRCameraRig rig)
	{
		if (enabled)
        {
            OVRPose pose = new OVRPose();
            // pose.position = new Vector3(1, 2, 3);
            pose.orientation = Quaternion.Euler(0, 127.9f, 0);
            rig.trackingSpace.FromOVRPose(pose, true);
        }
	}

    private void Start()
    {
        // Adds all available tracked devices to a list
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(subsystems);
        for (int i = 0; i < subsystems.Count; i++)
        {
            subsystems[i].TryRecenter();
            subsystems[i].TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
        }
        writer = new StreamWriter("C:\\Users\\tova\\Desktop\\output\\output.txt");  // just for debugging
        time = 0;  // also just for debugging
    }

    private void FixedUpdate()
    {
        InputTracking.GetNodeStates(mNodeStates);
        // xVector = new Vector2(mHeadVelocity[0], mHeadVelocity[1]);

        foreach (XRNodeState nodeState in mNodeStates)
        {
            switch (nodeState.nodeType)
            {
                case XRNode.Head:
                    nodeState.TryGetVelocity(out mHeadVelocity);
                    nodeState.TryGetAngularVelocity(out mHeadAngularVelocity);
                    nodeState.TryGetPosition(out mHeadPos);
                    nodeState.TryGetRotation(out mHeadRot);

                     // Calculate the horizontal component of the HMD velocity
                    float velocityX = mHeadVelocity.x;
                    float velocityZ = mHeadVelocity.z;
                    
                    // Calculate the horizontal component of the HMD position
                    float positionX = mHeadPos.x;
                    float positionY = mHeadPos.y;
                    float positionZ = mHeadPos.z;

                    if (velocityX > 0) 
                    {
                        // right
                        yVectorDirection = Vector3.up;
                    }
                    else if (velocityX < 0)
                    {
                        // left
                        yVectorDirection = -Vector3.up;
                    }

                    // Calculate the direction the player should move towards
                    currDirection = Vector3.Cross(yVectorDirection, mHeadVelocity).normalized;

                    if ((velocityX >= minWalkingVelocity || velocityX <= -minWalkingVelocity || velocityZ >= minWalkingVelocity || velocityZ <= -minWalkingVelocity))
                    {
                        if (Vector3.Angle(Camera.main.transform.forward, -currDirection) <= maxForwardAngle) 
                        {
                            Debug.DrawRay(yVectorDirection, -currDirection*50, Color.red);  // horizontal velocity
                            Debug.DrawRay(yVectorDirection, Camera.main.transform.forward*50, Color.green); // direction the hmd is facing
                            Debug.DrawLine(-currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays
                            
                            Vector3 targetPosition = Head.transform.position + -currDirection * playerSpeed * Time.deltaTime;
                            Head.transform.position = Vector3.SmoothDamp(Head.transform.position, targetPosition, ref cameraVelocity, smoothTime);
                        }
                        else 
                        {
                            Debug.DrawRay(yVectorDirection, currDirection*50, Color.red);  // horizontal velocity
                            Debug.DrawRay(yVectorDirection, Camera.main.transform.forward*50, Color.green); // direction the hmd is facing
                            Debug.DrawLine(currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays
                            
                            Vector3 targetPosition = Head.transform.position + currDirection * playerSpeed * Time.deltaTime;
                            Head.transform.position = Vector3.SmoothDamp(Head.transform.position, targetPosition, ref cameraVelocity, smoothTime);
                        }
                    }
                break;
            }
        }
        time += 1;
    }

 
    /*
    Just closes the writing stream (for testing)
    */
    private void OnApplicationQuit()
    {
        writer.Close();
    }

}

