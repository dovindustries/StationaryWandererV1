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
    public Rigidbody LeftHand, RightHand;
    private List<XRNodeState> mNodeStates = new List<XRNodeState>();
    private Vector3 mHeadPos, mLeftHandPos, mRightHandPos;
    private Vector3 mHeadVelocity, mLeftHandVelocity, mRightHandVeocity;
    private Vector3 crossProduct;
    private Quaternion mHeadRot, mLeftHandRot, mRightHandRot;
    private float speed = 4f; // increasing this value increases the movement speed
    StreamWriter writer; // for debugging
    int time; // for debugging

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

    void Update()
    {
        OVRInput.FixedUpdate();
        OVRInput.Update();
    }

    private void FixedUpdate()
    {
        InputTracking.GetNodeStates(mNodeStates);

        foreach (XRNodeState nodeState in mNodeStates)
        {
            switch (nodeState.nodeType)
            {
                case XRNode.Head:
                    // DO ALL HMD LOGIC HERE
                    nodeState.TryGetVelocity(out mHeadVelocity);
                    nodeState.TryGetPosition(out mHeadPos);
                    nodeState.TryGetRotation(out mHeadRot);

                    Vector3 yVectorDirection;

                    void Update()
                    {
                        if (getVelocityX() > 0) 
                        {
                            yVectorDirection = transform.up;
                        }
                        else if (getVelocityX() < 0)
                        {
                            yVectorDirection = -transform.up;
                        }

                        // Get the cross product of the headset's horizontal velocity
                        crossProduct = Vector3.Cross(mHeadVelocity, yVectorDirection);

                        Debug.Debug.DrawRay(yVectorDirection, crossProduct*100, color = Color.red, bool depthTest = true);

                        // Convert the headset's velocity into a direction relative to the player's transform
                        // Vector3 direction = transform.TransformDirection(mHeadVelocity);
                        
                        // Test: draw ray to make sure cross product vector faces the correct direction

                        // // Calculate the new position of the player
                        // Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;

                        // // Update the player's position
                        // transform.position = newPosition;
    }

                    // QUADRANT A 
                    // ==============
                    // if (isAngleInQuadA(getVelocityX(), getVelocityZ() )  &&  OVRInput.Get(OVRInput.Button.Four))
                    // {
                    //     if (isLeftStepInQuadA()) // Left step
                    //     {
                    //         stepForward(getVelocityX(), getVelocityZ());
                    //         break;
                    //     }
                    // }
                    // if ( isAngleInQuadA(-getVelocityX(), -getVelocityZ())  && OVRInput.Get(OVRInput.Button.Four))
                    // {
                    //     if (isRightStepInQuadA()) // Right step
                    //     {
                    //         stepForward(-getVelocityX(), -getVelocityZ());
                    //         break;
                    //     }
                    // }

                    // // QUADRANT B
                    // // ==============
                    // if (isAngleInQuadB(-getVelocityX(), -getVelocityZ()) && OVRInput.Get(OVRInput.Button.Two))
                    // {
                    //     if (isLeftStepInQuadB()) // Left step
                    //     {
                    //         stepForward(-getVelocityX(), -getVelocityZ());
                    //         break;
                    //     }
                    // }
                    // if (isAngleInQuadB(getVelocityX(), getVelocityZ()) && OVRInput.Get(OVRInput.Button.Two))
                    // {
                    //     if (isRightStepInQuadB()) // Right step
                    //     {
                    //         stepForward(getVelocityX(), getVelocityZ());
                    //         break;
                    //     }
                    // }

                    // // QUADRANT C
                    // // ==============
                    // if (isAngleInQuadC(getVelocityX(), getVelocityZ()))
                    // {
                    //     if (isLeftStepInQuadC()) // Left step
                    //     {   
                    //         stepForward(getVelocityX(), getVelocityZ());
                    //         break;
                    //     }
                    // }
                    // if (isAngleInQuadC(-getVelocityX(), -getVelocityZ()))
                    // {
                    //     if (isRightStepInQuadC()) // Right step
                    //     {
                    //         stepForward(-getVelocityX(), -getVelocityZ());
                    //         break;
                    //     }
                    // }

                    // // QUADRANT D
                    // // ==============
                    // if (isAngleInQuadD(getVelocityX(), getVelocityZ()))
                    // {
                    //     if (isLeftStepInQuadD()) // Left step
                    //     {   
                    //         stepForward(getVelocityX(), getVelocityZ());
                    //         break;
                    //     }
                    // }
                    // if (isAngleInQuadD(-getVelocityX(), -getVelocityZ()))
                    // {
                    //     if (isRightStepInQuadD()) // Right step
                    //     {
                    //         stepForward(-getVelocityX(), -getVelocityZ());
                    //         break;
                    //     }
                    // }

                    break;

                // Note to future self:
                // If we notice that head if rotated left or right,
                // then check what direction hands are in. If hands
                // are still in forward direction, this means player
                // is looking in a different direction then their
                // moving. In this case, move player in direction
                // of their hands.

                // case XRNode.LeftHand:
                //     // DO ALL LEFT HAND LOGIC HERE
                //     //  Debug.Log(m_MainCamera.transform.forward);
                //     Vector3 m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                //     nodeState.TryGetVelocity(out mLeftHandPos);
                //     LeftHand.MovePosition(mLeftHandPos  + m_Input * Time.deltaTime * speed);
                //     LeftHand.MoveRotation(mLeftHandRot.normalized);
                //     break;

                // case XRNode.RightHand:
                //     // DO ALL RIGHT HAND LOGIC HERE
                //     nodeState.TryGetPosition(out mRightHandPos);
                //     RightHand.MovePosition(mRightHandPos);
                //     RightHand.MoveRotation(mRightHandRot.normalized);
                //     break;
            }
        }
        time += 1;
    }


    /*
    THESE METHODS RETURN WHETHER THE HMD IS MOVING TO THE LEFT OR RIGHT
    WITHIN THE CURRENT QUADRANT.

    Here, if the HMD's velocity meets or exceeds 0.25 units, then that
    signifies one step forwards/backwards.
    */
    private bool isLeftStepInQuadA() 
    {
        return  getVelocityX() <= -0.25f || getVelocityZ() <= -0.25f;
    }
    private bool isRightStepInQuadA() 
    {
        return  getVelocityX() >= 0.25f || getVelocityZ() >= 0.25f;
    }
    private bool isLeftStepInQuadB() 
    {
        return  getVelocityX() >= 0.25f || getVelocityZ() <= -0.25f;        
    }
    private bool isRightStepInQuadB() 
    {
        return getVelocityX() <= -0.25f || getVelocityZ() >= 0.25f;
    }
    private bool isLeftStepInQuadC() 
    {
        return  getVelocityX() >= 0.25f || getVelocityZ() <= -0.25f;                
    }
    private bool isRightStepInQuadC() 
    {
        return  getVelocityX() <= -0.25f || getVelocityZ() >= 0.25f;
    }
    private bool isLeftStepInQuadD() 
    {
        return  getVelocityX() >= 0.25f || getVelocityZ() >= 0.25f;            
    }
    private bool isRightStepInQuadD() 
    {
        return  getVelocityX() <= -0.25f || getVelocityZ() >= 0.25f;
    }

    
    /*
    Returns the current angle as a signed value from [0,180] U [-180,0] 
    given from, to, and normal vectors.
    */
    private static float SignedAngle( Vector3 from, Vector3 to, Vector3 normal )
    {
        float angle = Vector3.Angle( from, to );
        float sign = Mathf.Sign( Vector3.Dot( normal, Vector3.Cross( from, to ) ) );
        return angle * sign;
    }


    /*
    THE FOLLOWING METHODS RETURN WHETHER THE CURRENT VELOCITY VECTOR
    IS IN THE REQUESTED QUADRANT.
    */
    private bool isAngleInQuadA(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.Normalized);
        float signedAngle = SignedAngle(Vector3.forward, localZAxis.Normalized, transform.up);
        if (signedAngle < 0f && signedAngle >= -90f)
        {
            return true;
        }
        return false;
    }
    private bool isAngleInQuadB(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis).Normalized;
        float signedAngle = SignedAngle(Vector3.forward, localZAxis, transform.up);
        // 0 is vertical and 90 is right angle on the right
        if (signedAngle >= 0f && signedAngle <= 90f)
        {
            return true;
        }
        return false;
    }
    private bool isAngleInQuadC(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.Normalized);
        float signedAngle = SignedAngle(Vector3.forward, localZAxis.Normalized, transform.up);
        if (signedAngle < -90f && signedAngle >= -180)
        {
            return true;
        }
        return false;
    }
    private bool isAngleInQuadD(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.Normalized);
        float signedAngle = SignedAngle(Vector3.forward, localZAxis.Normalized, transform.up);
        if (signedAngle > 90f && signedAngle <= 180f)
        {
            return true;
        }
        return false;
    }

    /*
    Moves player object forward one unit.
    */
    private void stepForward(float velocityX, float velocityZ)
    {
        Vector3 xAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 crossProduct = Vector3.Cross(transform.down, xAxis);
        Vector3 direction = crossProduct.Normalized;
        Head.transform.position = Vector3.MoveTowards(Head.transform.position, direction, speed * Time.deltaTime);
        Head.transform.rotation = Quaternion.LookRotation(direction);
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

