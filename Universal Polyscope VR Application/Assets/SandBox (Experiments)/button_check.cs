// ------------------------------------------------------------------------------------------------------------------------ //
// ----------------------------------------------------- LIBRARIES -------------------------------------------------------- //
// ------------------------------------------------------------------------------------------------------------------------ //

// -------------------- System -------------------- //

using System;
using System.Text;
using _Project.Scripts.Connection;
using Unity.VisualScripting;
// -------------------- Unity -------------------- //
using UnityEngine.EventSystems;
using UnityEngine;

public class button_check: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // -------------------- String -------------------- //
    public string acceleration = "1.0";
    public string time = "0.05";
    public string[] speed_param      = new string[6] {"0.0", "0.0", "0.0", "0.0","0.0","0.0"};
    public string[] speed_param_null = new string[6] { "0.0", "0.0", "0.0", "0.0", "0.0", "0.0" };
    // -------------------- Int -------------------- //
    public int index;
    // -------------------- UTF8Encoding -------------------- //
    private UTF8Encoding utf8 = new UTF8Encoding();

    private void Update()
    {
        foreach (var button in UR5_Robot_Connection.UR5_Data_Control.button_pressed)
        {
            if (UR5_Robot_Connection.UR5_Data_Control.button_pressed[index] == true)
            {
                // create auxiliary command string for speed control UR robot
                UR5_Robot_Connection.UR5_Data_Control.auxCommand = "speedl([" + speed_param[0] +","+  speed_param[1] + "," + speed_param[2]
                                                                   + "," + speed_param[3] + "," + speed_param[4] + "," + speed_param[5] + "], a =" + acceleration + ", t =" + time + ")" + "\n";
                // get bytes from command string
                UR5_Robot_Connection.UR5_Data_Control.command = utf8.GetBytes(UR5_Robot_Connection.UR5_Data_Control.auxCommand);
            }
            else
                UR5_Robot_Connection.UR5_Data_Control.button_pressed[index] = false;
        }
        

    }

    // -------------------- Button -> Pressed -------------------- //
    [ContextMenu("OnPointerDown")]
    public void OnPointerDown(PointerEventData eventData)
    {
        // create auxiliary command string for speed control UR robot
        UR5_Robot_Connection.UR5_Data_Control.auxCommand = "speedl([" + speed_param[0] +","+  speed_param[1] + "," + speed_param[2]
                                                           + "," + speed_param[3] + "," + speed_param[4] + "," + speed_param[5] + "], a =" + acceleration + ", t =" + time + ")" + "\n";
        // get bytes from command string
        UR5_Robot_Connection.UR5_Data_Control.command = utf8.GetBytes(UR5_Robot_Connection.UR5_Data_Control.auxCommand);
        // confirmation variable -> is pressed
        UR5_Robot_Connection.UR5_Data_Control.button_pressed[index] = true;
    }

    // -------------------- Button -> Un-Pressed -------------------- //
    [ContextMenu("OnPointerUp")]
    public void OnPointerUp(PointerEventData eventData)
    {
        // confirmation variable -> is un-pressed
        UR5_Robot_Connection.UR5_Data_Control.button_pressed[index] = false;
    }

}
