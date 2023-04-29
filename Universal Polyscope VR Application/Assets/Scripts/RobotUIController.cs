using System;
using System.Text;
using TMPro;
using UnityEngine;

public class RobotUIController : MonoBehaviour
{
    public TMP_InputField ipAddressField;
    public GameObject debuggerCanvas;

    private UTF8Encoding utf8 = new UTF8Encoding();

    private String ipAddress = "172.29.43.153";

    private void Start()
    {
        ipAddressField.text = ipAddress;

        // Auxiliary first command -> Write initialization position/rotation with acceleration/time to the robot controller
        // command (string value)
        RobotConnectionManager.RobotWriteParams.auxCommand =
            "speedl([0.0,0.0,0.0,0.0,0.0,0.0], a = 0.15, t = 0.03)" + "\n";
        // get bytes from string command
        RobotConnectionManager.RobotWriteParams.command =
            utf8.GetBytes(RobotConnectionManager.RobotWriteParams.auxCommand);
    }

    private void FixedUpdate()
    {
        RobotConnectionManager.RobotReadParams.ipAddress = ipAddressField.text;
        RobotConnectionManager.RobotWriteParams.ipAddress = ipAddressField.text;

        if (RobotConnectionManager.ConnectionControlStates.connect == true)
        {
            print("CONNECTED");

            print(RobotConnectionManager.RobotReadParams.jointOrientation[0]);
        }
        else if (RobotConnectionManager.ConnectionControlStates.disconnect == true)
        {
            print("DISCONNECTED");
        }
    }

    private void OnApplicationQuit()
    {
        Destroy(this);
    }

    // method for the connect button
    [ContextMenu("ConnectButton")]
    public void ConnectButton()
    {
        RobotConnectionManager.ConnectionControlStates.connect = true;
        RobotConnectionManager.ConnectionControlStates.disconnect = false;
    }

    // method for the disconnect button
    [ContextMenu("DisconnectButton")]
    public void DisconnectButton()
    {
        RobotConnectionManager.ConnectionControlStates.connect = false;
        RobotConnectionManager.ConnectionControlStates.disconnect = true;
    }
    public void ToggleDebugger()
    {
        debuggerCanvas.SetActive(!debuggerCanvas.activeSelf);
    }
}