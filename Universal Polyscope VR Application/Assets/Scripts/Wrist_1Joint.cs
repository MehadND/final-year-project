using System;
using UnityEngine;

public class Wrist_1Joint : MonoBehaviour
{
    private void FixedUpdate()
    {
        try
        {
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, (float)(RobotConnectionManager.RobotReadParams.jointOrientation[2] * (180.0 / Math.PI)));
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