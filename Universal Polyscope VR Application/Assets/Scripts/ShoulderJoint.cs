using System;
using UnityEngine;

public class ShoulderJoint : MonoBehaviour
{
    private void FixedUpdate()
    {
        try
        {
            transform.localEulerAngles = new Vector3(0.0f, (-1.0f) * (float)(RobotConnectionManager.RobotReadParams.jointOrientation[0] * (180.0 / Math.PI)), 0.0f);
        }
        catch (Exception e)
        {
            Debug.Log("Exception:" + e);
        }
    }

    private void OnApplicationQuit()
    {
        Destroy(this);
    }
}