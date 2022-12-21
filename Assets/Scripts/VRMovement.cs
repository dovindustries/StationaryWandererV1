
// VR Movement script. Property of DovIndustries Inc.
// Unity's HMD movement API: https://docs.unity3d.com/ScriptReference/XR.XRNodeState.html

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.IO;


/*
VRMovement contains all the logic to move the player based on HMD
and controller input.
*/
public class VRMovement : MonoBehaviour
{
    public GameObject Head;
    public GameObject LeftHand, RightHand;
    GameObject hmd;
    private List<XRNodeState> mNodeStates = new List<XRNodeState>();
    private Vector3 mHeadPos, mLeftHandPos, mRightHandPos;
    private Vector3 mHeadVelocity, mLeftHandVelocity, mRightHandVeocity, mHeadAngularVelocity;
    private Quaternion mHeadRot, mLeftHandRot, mRightHandRot;

    Quaternion headRotation;

    private float minWalkingVelocity = 0.23f;
    private Vector3 cameraVelocity = Vector3.zero;
    
    private Vector3 yVectorDirection;
    private Vector3 crossProduct; // represents the direction the player moves towards

    [SerializeField] float playerSpeed = 2.0f; // increasing this value increases the movement speed
    [SerializeField] float maxForwardAngle = 110.0f;  // the maximum angle the hmd points in relative to the forward walking motion. Once this value is exceeded, the camera moves backwards.
    [SerializeField] Vector3 currDirection;
    [SerializeField] float smoothTime = 0.3F;

    StreamWriter writer; // for debugging
    int time; // for debugging


    public OVRPose trackerPose = OVRPose.identity;
    
	void Awake()
	{
		OVRCameraRig rig = GameObject.FindObjectOfType<OVRCameraRig>();

		if (rig != null)
			rig.UpdatedAnchors += OnUpdatedAnchors;
	}

	void OnUpdatedAnchors(OVRCameraRig rig)
	{
		if (!enabled)
			return;

        // problem is somehwere around here
		OVRPose pose = rig.trackerAnchor.ToOVRPose(true).Inverse();
		pose = trackerPose * pose;
		rig.trackingSpace.FromOVRPose(pose, true);
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
                    // DO ALL HMD LOGIC HERE
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

                    // print(Camera.main.transform.position + " x, z " + positionZ);

                    if (getVelocityX() > 0) 
                    {
                        // right
                        yVectorDirection = Vector3.up;
                    }
                    else if (getVelocityX() < 0)
                    {
                        // left
                        yVectorDirection = -Vector3.up;
                    }

                    // Calculate the direction the player should move
                    currDirection = Vector3.Cross(yVectorDirection, mHeadVelocity).normalized;

                    if ((getVelocityX() >= minWalkingVelocity || getVelocityX() <= -minWalkingVelocity || getVelocityZ() >= minWalkingVelocity || getVelocityZ() <= -minWalkingVelocity))
                    {
                        Debug.DrawRay(yVectorDirection, -currDirection*50, Color.red);  // horizontal velocity
                        Debug.DrawRay(Vector3.up, Camera.main.transform.forward*50, Color.green); // hmd forward direction
                        Debug.DrawLine(-currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays

                        Head.transform.position += currDirection * playerSpeed * Time.deltaTime;

                        if (Vector3.Angle(Camera.main.transform.forward, -currDirection) <= maxForwardAngle) 
                        {
                            Head.transform.Translate(-currDirection * playerSpeed * Time.deltaTime);
                        }
                        else 
                        {
                            Head.transform.Translate(currDirection * playerSpeed * Time.deltaTime);
                        }
                    }
                break;
            }
        }
        time += 1;
    }

    /*
    Returns the current headset's velocity.
    */
    private Vector3 getHMDVelocity()
    {
        return mHeadVelocity;
    }

    /*
    Returns XRNode's rotation for X axis (roll)
    */
    private float getRotationX()
    {
        return mHeadRot[0];
    }

    /*
    Returns XRNode's rotation for Z axis (pitch)
    */
    private float getRotationZ()
    {
        return mHeadRot[2];
    }

    /*
    Returns XRNode's rotation for Y axis (yaw)
    */
    private float getRotationY()
    {
        return mHeadRot[1];
    }

    /*
    Returns XRNode's position for X axis (left, right)
    */
    private float getPositionX()
    {
        return mHeadPos[0];
    }

    /*
    Returns XRNode's position for Y axis (up, down)
    */
    private float getPositionY()
    {
        return mHeadPos[1];
    }

    /*
    Returns XRNode's position for Z axis (forward, backward)
    */
    private float getPositionZ()
    {
        return mHeadPos[2];
    }

    /*
    Returns XRNode's velocity for X axis (left, right)
    */
    private float getVelocityX()
    {
        return mHeadVelocity[0];
    }


    /*
    Returns XRNode's velocity for Y axis (up, down)
    */
    private float getVelocityY()
    {
        return mHeadVelocity[1];
    }

    /*
    Returns XRNode's velocity for X axis (forward, backward)
    */
    private float getVelocityZ()
    {
        return mHeadVelocity[2];
    }

    /*
    Just closes the writing stream (for testing only)
    */
    private void OnApplicationQuit()
    {
        writer.Close();
    }

}

