
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
    private List<XRNodeState> mNodeStates = new List<XRNodeState>();
    private Vector3 mHeadPos, mLeftHandPos, mRightHandPos;
    private Vector3 mHeadVelocity, mLeftHandVelocity, mRightHandVeocity;
    private Quaternion mHeadRot, mLeftHandRot, mRightHandRot;
    private float speed = 1.0f; // increasing this value increases the movement speed
    private float signedRotAngle;
    private Vector3 yVectorDirection;
    private Vector3 crossProduct;
    [SerializeField] float minWalkingVelocity = 0.23f;
    [SerializeField] Vector3 currDirection;


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
        signedRotAngle = 1;

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

        UnityEngine.XR.InputDevice handRDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        UnityEngine.XR.InputDevice handLDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        UnityEngine.XR.InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        foreach (XRNodeState nodeState in mNodeStates)
        {
            switch (nodeState.nodeType)
            {
                case XRNode.Head:
                    // DO ALL HMD LOGIC HERE
                    nodeState.TryGetVelocity(out mHeadVelocity);
                    nodeState.TryGetPosition(out mHeadPos);
                    nodeState.TryGetRotation(out mHeadRot);

                    if (getVelocityX() > 0) 
                    {
                        yVectorDirection = transform.up;
                    }
                    else if (getVelocityX() < 0)
                    {
                        yVectorDirection = -transform.up;
                    }
                    // Move the camera forward
                    // Head.transform.position = Vector3.MoveTowards(Head.transform.position, crossProduct.normalized, speed * Time.deltaTime);
                    
                    // Not sure if use Head or just transform. Experiment with both pls!
                    // Store the current camera position vector and forward vector
                    // Vector3 playerForward = Head.transform.rotation;

                    // GET ANGLE BETWEEN CROSS PRODUCT AND HEADSET Y ROTATION



                    // print(getRotationY() + " head.transform.position");

                    // Convert the direction from the player's local space to world space
                    // Vector3 direction = Head.transform.TransformDirection(playerForward);
                    
                    // Move the player in the direction of the player's pitch
                    // transform.position = currentPosition + direction * speed * Time.deltaTime;


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

                    // Get the cross product of the headset's horizontal velocity
                    crossProduct = Vector3.Cross(yVectorDirection, mHeadVelocity);

                    // Convert the headset's velocity into a direction relative to the player's transform
                    currDirection = crossProduct.normalized;


                    if (getVelocityX() >= minWalkingVelocity || getVelocityX() <= -minWalkingVelocity || getVelocityZ() >= minWalkingVelocity || getVelocityZ() <= -minWalkingVelocity)
                    {
                        Debug.DrawRay(yVectorDirection, -currDirection*50, Color.red);
                        Debug.DrawRay(Vector3.up, Camera.main.transform.forward, Color.green);
                        Debug.DrawLine(-currDirection, Camera.main.transform.forward, Color.yellow);
                        
                        // NOW, FIGURE OUT HOW TO GET THESE LINES!!

                        print(Vector3.Angle(Camera.main.transform.forward, -currDirection));

                        // Update the player's position
                        // Head.transform.position = Vector3.MoveTowards(Head.transform.position, crossProduct, speed * Time.deltaTime);
                        // Head.transform.Translate(-direction * Time.deltaTime);
                        if (Vector3.Angle(Camera.main.transform.forward, -currDirection) <= 90) 
                        {
                            Head.transform.Translate(-currDirection * Time.deltaTime);
                        }
                        else 
                        {
                            Head.transform.Translate(currDirection * Time.deltaTime);
                        }
                    }
                    // if (getVelocityX() >= minWalkingVelocity || getVelocityX() <= -minWalkingVelocity || getVelocityZ() >= minWalkingVelocity || getVelocityZ() <= -minWalkingVelocity)
                    // {
                    //     Debug.DrawRay(yVectorDirection, crossProduct*50, Color.green);
                    //     // Update the player's position
                    //     // Head.transform.position = Vector3.MoveTowards(Head.transform.position, currDirection, speed * Time.deltaTime);
                    //     Head.transform.Translate(direction * Time.deltaTime);
                    // }



                    // }
                    // else {
                    //     Vector3 direction = crossProduct.normalized;

                    //     if (getVelocityX() >= minWalkingVelocity || getVelocityX() <= -minWalkingVelocity || getVelocityZ() >= minWalkingVelocity || getVelocityZ() <= -minWalkingVelocity)
                    //     {
                    //         // Debug.Log(getVelocityX());
                    //                             Debug.DrawRay(yVectorDirection, crossProduct*50, Color.red);
                    //         // Update the player's position
                    //         // Head.transform.position = Vector3.MoveTowards(Head.transform.position, crossProduct, speed * Time.deltaTime);
                    //         Head.transform.Translate(direction * Time.deltaTime);
                    //     }
                    // }
                
                    // else 
                    // {
                    //     // Get the cross product of the headset's horizontal velocity
                    //     crossProduct = Vector3.Cross(yVectorDirection, mHeadVelocity);

                    //     Debug.DrawRay(yVectorDirection, -crossProduct*50, Color.red);


                    //     // Convert the headset's velocity into a direction relative to the player's transform
                    //     Vector3 direction = -crossProduct.normalized;

                    //     if (getVelocityX() >= minWalkingVelocity || getVelocityX() <= -minWalkingVelocity || getVelocityZ() >= minWalkingVelocity || getVelocityZ() <= -minWalkingVelocity)
                    //     {
                    //         Debug.Log(getVelocityX());
                    //         // Update the player's position
                    //         // Head.transform.position = Vector3.MoveTowards(Head.transform.position, crossProduct, speed * Time.deltaTime);
                    //         Head.transform.Translate(direction * Time.deltaTime);
                    //     }
                    // }


                    

               


                    // Debug.Log(Camera.main.transform.localEulerAngles.y + " " + isHMDInQuadrantD());
                    // QUADRANT A 
                    // ==============
                    // && Camera.main.transform.localEulerAngles.y  <= 270 ||  FIGURE OUT FOR x AXIS TOO!
                    // if ( Camera.main.transform.localEulerAngles.y >= 90 )
                    // {
                    //     Debug.Log(mHeadRot.eulerAngles[0]);
                    // }
                    // else {
                    //     Debug.Log(-mHeadRot.eulerAngles[0]);
                    // }

                    // if(!isHMDInQuadrantD()) 
                    // {
                    //     if (isAngleInQuadA(getVelocityX(), getVelocityZ())  )
                    //     {
                    //         if (isLeftStepInQuadA()) // Left step
                    //         {
                    //             stepForward(getVelocityX(), getVelocityZ());
                    //             Debug.Log("forward A");
                    //             break;
                    //         }
                    //     }
                    //     if ( isAngleInQuadA(-getVelocityX(), -getVelocityZ()))
                    //     {
                    //         if (isRightStepInQuadA()) // Right step
                    //         {
                    //             stepForward(-getVelocityX(), -getVelocityZ());
                    //             // Debug.Log(Camera.main.transform.localEulerAngles.y + " A");
                    //             Debug.Log("forward A");
                    //             break;
                    //         }
                    //     }
                    // }
                    

                    // // QUADRANT B
                    // // ==============
                    // if  (!isHMDInQuadrantC()) {
                    //     if (isAngleInQuadB(-getVelocityX(), -getVelocityZ()))
                    //     {
                    //         if (isLeftStepInQuadB()) // Left step
                    //         {
                    //             stepForward(-getVelocityX(), -getVelocityZ());
                    //             // Debug.Log("forward BL " +  Camera.main.transform.localEulerAngles.y);
                    //             break;
                    //         }
                    //     }
                    //     if (isAngleInQuadB(getVelocityX(), getVelocityZ()) && !isHMDInQuadrantC())
                    //     {
                    //         if (isRightStepInQuadB()) // Right step
                    //         {
                    //             stepForward(getVelocityX(), getVelocityZ());
                    //             // Debug.Log("forward BR " +  Camera.main.transform.localEulerAngles.y);
                    //             break;
                    //         }
                    //     }
                    // }
                    

                    // // QUADRANT C
                    // // ==============
                    // if (isAngleInQuadC(getVelocityX(), getVelocityZ()))
                    // {
                    //     if (isLeftStepInQuadC()) // Left step
                    //     {   
                    //         stepForward(getVelocityX(), getVelocityZ());
                    //         // Debug.Log("forward C");
                    //         break;
                    //     }
                    // }
                    // if (isAngleInQuadC(-getVelocityX(), -getVelocityZ()))
                    // {
                    //     if (isRightStepInQuadC()) // Right step
                    //     {
                    //         stepForward(-getVelocityX(), -getVelocityZ());
                    //         // Debug.Log("forward C");
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
                    //         // Debug.Log("forward D");
                    //         break;
                    //     }
                        
                    // }
                    // if (isAngleInQuadD(-getVelocityX(), -getVelocityZ()))
                    // {
                    //     if (isRightStepInQuadD()) // Right step
                    //     {
                    //         stepForward(-getVelocityX(), -getVelocityZ());
                    //         // Debug.Log("forward D");
                    //         break;
                    //     }
                    // }
                    // // else {
                    // //     stepForward(Mathf.Abs(getVelocityX()), Mathf.Abs(getVelocityZ()));
                    // //     Vector3 localXAxis = new Vector3(getVelocityX(), 0f, getVelocityZ());
                    // //     Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.normalized);
                    // //     Debug.Log("out " + SignedAngle(Vector3.forward, localZAxis.normalized ,transform.up));
                    // // }
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



    private bool isHMDInQuadrantC()
    {
        return Camera.main.transform.localEulerAngles.y >= 180 && Camera.main.transform.localEulerAngles.y <= 270;
    }

    private bool isHMDInQuadrantD()
    {
        return Camera.main.transform.localEulerAngles.y >= 90 && Camera.main.transform.localEulerAngles.y <= 180;
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

    
    // /*
    // Returns the current angle as a signed value from [0,180] U [-180,0]
    // given from, to, and normal vectors.
    // */
    // private static float SignedAngle( Vector3 from, Vector3 to, Vector3 normal )
    // {
    //     float angle = Vector3.Angle( from, to );

    //     float sign = Mathf.Sign( Vector3.Dot( normal, Vector3.Cross( from, to ) ) );
    //     Debug.Log((sign) + " angles");

    //     return angle * sign;
    // }


    /*
    THE FOLLOWING METHODS RETURN WHETHER THE CURRENT VELOCITY VECTOR
    IS IN THE REQUESTED QUADRANT.

    I REMOVED THE NORMALIZE FROM localZAxis.normalize!!! if it does not work, add normalized back!!!!
    */
    private bool isAngleInQuadA(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.normalized);
        float signedAngle = Vector3.SignedAngle( localZAxis, transform.forward, Vector3.up);
        if (signedAngle <= 0f && signedAngle >= -90f)
        {
            // Debug.Log(signedAngle + " AAAA");
            // Debug.Log(signedAngle + " " + Camera.main.transform.localEulerAngles.y + " A" + !isHMDInQuadrantD());
            return true;
        }
        return false;
    }
    private bool isAngleInQuadB(float velocityX, float velocityZ)
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.normalized);
        float signedAngle = Vector3.SignedAngle(localZAxis, transform.forward, Vector3.up);
        // 0 is vertical and 90 is right angle on the right
        if (signedAngle >= 0f && signedAngle <= 90f)
        {
            // writer.WriteLine("in B " + signedAngle + " " + time +  " " + Camera.main.transform.localEulerAngles.y);
            // Debug.Log(signedAngle + " BBBB");/
            Debug.DrawRay(Camera.main.transform.localEulerAngles, localZAxis * 100);
            return true;
        }
        return false;
    }
    private bool isAngleInQuadC(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.normalized);
        float signedAngle = Vector3.SignedAngle(localZAxis, transform.forward, Vector3.up);
        if (signedAngle < -90f && signedAngle >= -180)
        {
            // Debug.Log(signedAngle + " CCCC");
            return true;
        }
        return false;
    }
    private bool isAngleInQuadD(float velocityX, float velocityZ) 
    {
        Vector3 localXAxis = new Vector3(velocityX, 0f, velocityZ);
        Vector3 localZAxis = Vector3.Cross(transform.up, localXAxis.normalized);
        float signedAngle = Vector3.SignedAngle(localZAxis, transform.forward, Vector3.up);
        if (signedAngle > 90f && signedAngle <= 180f)
        {
            // Debug.Log(signedAngle + " DDDD");
            return true;
        }
        return false;
    }

    /*
    Moves player object forward. Distance is based on 
    the speed speed variable set above.
    */
    private void stepForward()
    {
        Vector3 crossProduct = Vector3.Cross(transform.up, mHeadVelocity);
        Head.transform.position = Vector3.MoveTowards(Head.transform.position, crossProduct.normalized, speed * Time.deltaTime);
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

