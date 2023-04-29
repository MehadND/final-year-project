using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class RobotJointDataTransmitter : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    // -------------------- String -------------------- //
    public string acceleration = "1.0";
    public string time = "0.05";
    private string[] speed_param      = new string[6] {"0.0", "0.0", "0.0", "0.0","0.0","0.0"};
    private string[] speed_param_null = new string[6] { "0.0", "0.0", "0.0", "0.0", "0.0", "0.0" };
    // -------------------- Int -------------------- //
    public int index;
    // -------------------- UTF8Encoding -------------------- //
    private UTF8Encoding utf8 = new UTF8Encoding();
    
    // for debugging purposes
    [Header("For Debugging Purposes")]
    [SerializeField, Range(-0.05f, 0.05f)] private float speed1;
    [SerializeField, Range(-0.05f, 0.05f)] private float speed2;
    [SerializeField, Range(-0.05f, 0.05f)] private float speed3;
    [SerializeField, Range(-0.05f, 0.05f)] private float speed4;
    [SerializeField, Range(-0.05f, 0.05f)] private float speed5;
    [SerializeField, Range(-0.05f, 0.05f)] private float speed6;

    private void Update()
    {
        // for debugging purposes
        speed_param[0] = speed1.ToString();
        speed_param[1] = speed2.ToString();
        speed_param[2] = speed3.ToString();
        speed_param[3] = speed4.ToString();
        speed_param[4] = speed5.ToString();
        speed_param[5] = speed6.ToString();
    }

    [ContextMenu("StartData")]
    public void SendData_Start()
    {
        print("Starting data transmission...");
        RobotConnectionManager.RobotWriteParams.auxCommand = "speedl([" + speed_param[0] + "," + speed_param[1]
                                                             + "," + speed_param[2] + "," + speed_param[3]
                                                             + "," + speed_param[4] + "," + speed_param[5]
                                                             + "], a =" + acceleration + ", t =" + time + ")" +
                                                             "\n";
        
        RobotConnectionManager.RobotWriteParams.command = utf8.GetBytes(RobotConnectionManager.RobotWriteParams.auxCommand);
        
        RobotConnectionManager.RobotWriteParams.button_pressed[index] = true;
        
        // for debugging purposes
        // foreach (var param in speed_param)
        // {
        //     Debug.Log("SENDING THESE SPEED PARAMS: " +
        //           "SPEED 1 = " + speed_param[0] 
        //           + "\n"
        //           + "SPEED 2 = " + speed_param[1]
        //           + "\n"
        //           + "SPEED 3 = " + speed_param[2]
        //           + "\n"
        //           + "SPEED 4 = " + speed_param[3]
        //           + "\n"
        //           + "SPEED 5 = " + speed_param[4]
        //           + "\n"
        //           + "SPEED 6 = " + speed_param[5]
        //           + "\n");
        // }

        // for (int i = 0; i <= speed_param.Length; i++)
        // {
        //     print("SENDING THESE SPEED PARAMS: " + speed_param[i] + "\n");
        // }
    }

    [ContextMenu("StopData")]
    public void SendData_Stop()
    {
        print("Stopping data transmission...");
        RobotConnectionManager.RobotWriteParams.button_pressed[index] = false;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SendData_Stop();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SendData_Start();
    }
}