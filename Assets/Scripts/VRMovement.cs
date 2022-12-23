
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

    private List<XRNodeState> mNodeStates = new List<XRNodeState>();
    Dictionary<XRNode, Vector3> XRNodes = new Dictionary<XRNode, Vector3>();
    private Vector3 mHeadPos, mLeftHandPos, mRightHandPos;
    private Vector3 mHeadVelocity, mLeftHandVelocity, mRightHandVeocity, mHeadAngularVelocity;
    private Quaternion mHeadRot, mLeftHandRot, mRightHandRot;
    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 yVectorDirection;
    private Vector3 currDirection;
    private Vector3 position;

    // Create an XRNodeState object to store the state of the XRNode.Head node
    XRNodeState hmd = new XRNodeState();
    XRNodeState leftHand = new XRNodeState();
    XRNodeState rightHand = new XRNodeState();

    [SerializeField] float playerSpeed = 2.0f; // increasing this value increases the movement speed
    [SerializeField] float maxForwardAngle = 110.0f;  // maximum angle hmd points relative to forward walking motion. If exceeded, camera moves backwards.
    [SerializeField] float smoothTime = 0.03f;
    [SerializeField] float minWalkingVelocity = 0.23f;

    // For Debugging
    StreamWriter writer; 
    int time;


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
            // pose.position = new Vector3(1, 2, 3); // uncomment to change initial hmd position
            pose.orientation = Quaternion.Euler(0, 127.9f, 0);
            rig.trackingSpace.FromOVRPose(pose, true);
        }
	}

    private void Start()
    {
        
        // Try to get the current state of the XRNode.Head node
        if (InputTracking.TryGetNodeState(XRNode.Head, out hmd))
        {
            Vector3 positionH;
            if (hmd.TryGetPosition(out positionH))
            {
                Debug.Log($"Position: {positionH}");
            }
        }

        if (InputTracking.TryGetNodeState(XRNode.LeftHand, out leftHand))
        {
            Vector3 positionL;
            if (leftHand.TryGetPosition(out positionL))
            {
                Debug.Log($"Position: {positionL}");
            }
        }

        if (InputTracking.TryGetNodeState(XRNode.RightHand, out rightHand))
        {
            Vector3 positionR;
            if (rightHand.TryGetPosition(out positionR))
            {
                Debug.Log($"Position: {positionR}");
            }
        }

        // Adds all available tracked devices to dict
        // XRNodes.Add(XRNode.Head, Vector3.zero);
        // XRNodes.Add(XRNode.LeftHand, Vector3.zero);
        // XRNodes.Add(XRNode.RightHand, Vector3.zero);

        // For Debugging
        writer = new StreamWriter("C:\\Users\\tova\\Desktop\\output\\output.txt");  
        time = 0;
    }

    private void FixedUpdate()
    {
        InputTracking.GetNodeStates(mNodeStates);
        // foreach (XRNodeState nodeState in mNodeStates)
        // {
        //     if (XRNodes.ContainsKey(nodeState.nodeType))
        //     {
        //         nodeState.TryGetPosition(out Vector3 position);
        //         nodeState.TryGetVelocity(out mHeadVelocity);
        //         nodeState.TryGetAngularVelocity(out mHeadAngularVelocity);
        //         nodeState.TryGetPosition(out mHeadPos);
        //         nodeState.TryGetRotation(out mHeadRot);
        //         XRNodes[nodeState.nodeType] = position;
        //     }
        // }
        print(XRNodes[XRNode.Head.position]);
        print(XRNodes[XRNode.LeftHand]);
        print(XRNodes[XRNode.RightHand]);
        // nodeState.TryGetVelocity(out mHeadVelocity);
        // nodeState.TryGetAngularVelocity(out mHeadAngularVelocity);
        // nodeState.TryGetPosition(out mHeadPos);
        // nodeState.TryGetRotation(out mHeadRot);

        //     switch (nodeState.nodeType)
        //     {
        //         case XRNode.Head:
        //             nodeState.TryGetVelocity(out mHeadVelocity);
        //             nodeState.TryGetAngularVelocity(out mHeadAngularVelocity);
        //             nodeState.TryGetPosition(out mHeadPos);
        //             nodeState.TryGetRotation(out mHeadRot);

        //              // Calculate horizontal velocity components of HMD 
        //             float velocityX = mHeadVelocity.x;
        //             float velocityZ = mHeadVelocity.z;
                    
        //             // Calculate horizontal position components of HMD 
        //             float positionX = mHeadPos.x;
        //             float positionY = mHeadPos.y;
        //             float positionZ = mHeadPos.z;

        //             Vector3 yVectorDirection = getEqualizedYVector(velocityX);

        //             // Calculate the direction the player should move towards
        //             currDirection = Vector3.Cross(yVectorDirection, mHeadVelocity).normalized;

        //             if ( isSideToSideMovementFastEnough(velocityX, velocityZ) )
        //             {
        //                 if ( isPlayerInForwardFacingAngle(-currDirection) ) 
        //                 {
        //                     Debug.DrawRay(yVectorDirection, -currDirection*50, Color.red);  // horizontal velocity
        //                     Debug.DrawRay(yVectorDirection, Camera.main.transform.forward*50, Color.green); // direction the hmd is facing
        //                     Debug.DrawLine(-currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays

        //                     stepForward(currDirection);
        //                 }
        //                 else 
        //                 {
        //                     Debug.DrawRay(yVectorDirection, currDirection*50, Color.red);  // horizontal velocity
        //                     Debug.DrawRay(yVectorDirection, Camera.main.transform.forward*50, Color.green); // direction the hmd is facing
        //                     Debug.DrawLine(currDirection, Camera.main.transform.forward, Color.yellow); // angle between both rays
                            
        //                     stepForward(-currDirection);
        //                 }
        //             }
        //         break;
        //     }
        // }
        // time++; // for testing
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

