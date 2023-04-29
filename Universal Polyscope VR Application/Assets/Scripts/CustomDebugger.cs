using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class CustomDebugger : MonoBehaviour
{
    public TMP_Text textArea;
    public TMP_Text connectionStatus;

    // public static string GetLocalIPAddress()
    // {
    //     if (Application.isEditor || !Application.isPlaying)
    //     {
    //         var host = Dns.GetHostEntry(Dns.GetHostName());
    //         foreach (var ip in host.AddressList)
    //         {
    //             if (ip.AddressFamily == AddressFamily.InterNetwork)
    //             {
    //                 return ip.ToString();
    //             }
    //         }
    //     }
    //     throw new Exception("No network adapters with an IPv4 address in the system!");
    // }

    private void Update()
    {
        textArea.SetText("joint 1: " + RobotConnectionManager.RobotReadParams.jointOrientation[0] + "\n" +
                         "joint 2: " + RobotConnectionManager.RobotReadParams.jointOrientation[1] + "\n" +
                         "joint 3: " + RobotConnectionManager.RobotReadParams.jointOrientation[2] + "\n" +
                         "joint 4: " + RobotConnectionManager.RobotReadParams.jointOrientation[3] + "\n" +
                         "joint 5: " + RobotConnectionManager.RobotReadParams.jointOrientation[4] + "\n" +
                         "joint 6: " + RobotConnectionManager.RobotReadParams.jointOrientation[5] + "\n");
                         // "IP Address (Local): " + GetLocalIPAddress() + "\n");
        
        if(RobotConnectionManager.ConnectionControlStates.connect == true)
            connectionStatus.SetText("CONNECTION STATUS: CONNECTED");
        if(RobotConnectionManager.ConnectionControlStates.disconnect == true)
            connectionStatus.SetText("CONNECTION STATUS: DISCONNECTED");
    }
}
