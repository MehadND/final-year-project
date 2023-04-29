using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomRobotControls : MonoBehaviour
{
    public InputActionReference xplus;
    public InputActionReference xminus;
    public InputActionReference yplus;
    public InputActionReference yminus;
    public InputActionReference zplus;
    public InputActionReference zminus;

    private string acceleration = "1.0";
    private string time = "0.01";

    private UTF8Encoding encoder = new UTF8Encoding();

    private void Awake()
    {
        xplus.action.started += XPlusMovement;
        xminus.action.started += XMinusMovement;
        yplus.action.started += YPlusMovement;
        yminus.action.started += YMinusMovement;
        zplus.action.started += ZPlusMovement;
        zminus.action.started += ZMinusMovement;
    }

    private void OnApplicationQuit()
    {
        xplus.action.started -= XPlusMovement;
        xminus.action.started -= XMinusMovement;
        yplus.action.started -= YPlusMovement;
        yminus.action.started -= YMinusMovement;
        zplus.action.started -= ZPlusMovement;
        zminus.action.started -= ZMinusMovement;
    }


    private void XPlusMovement(InputAction.CallbackContext context)
    {
        print("X+");
        RobotConnectionManager.RobotWriteParams.button_pressed[0] = true;
        RobotConnectionManager.RobotWriteParams.button_pressed[1] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[2] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[3] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[4] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[5] = false;
        if (RobotConnectionManager.ConnectionControlStates.connect == true && RobotConnectionManager.RobotWriteParams.button_pressed[0] == true)
        {
            string[] speed_param = new string[6] { "0.05", "0.0", "0.0", "0.0", "0.0", "0.0" };
            RobotConnectionManager.RobotWriteParams.auxCommand = "speedl([" + speed_param[0] + "," + speed_param[1]
                                                                 + "," + speed_param[2] + "," + speed_param[3]
                                                                 + "," + speed_param[4] + "," + speed_param[5]
                                                                 + "], a =" + acceleration + ", t =" + time + ")" +
                                                                 "\n";

            RobotConnectionManager.RobotWriteParams.command =
                encoder.GetBytes(RobotConnectionManager.RobotWriteParams.auxCommand);

            RobotConnectionManager.RobotWriteParams.button_pressed[0] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[1] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[2] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[3] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[4] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[5] = false;
        }
        else
        {
            string[] speed_param = new string[6] { "0.0", "0.0", "0.0", "0.0", "0.0", "0.0" };
            RobotConnectionManager.RobotWriteParams.auxCommand = "speedl([" + speed_param[0] + "," + speed_param[1]
                                                                 + "," + speed_param[2] + "," + speed_param[3]
                                                                 + "," + speed_param[4] + "," + speed_param[5]
                                                                 + "], a =" + acceleration + ", t =" + time + ")" +
                                                                 "\n";

            RobotConnectionManager.RobotWriteParams.command =
                encoder.GetBytes(RobotConnectionManager.RobotWriteParams.auxCommand);

            RobotConnectionManager.RobotWriteParams.button_pressed[0] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[1] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[2] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[3] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[4] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[5] = false;
        }
    }

    private void XMinusMovement(InputAction.CallbackContext context)
    {
        print("X-");
        RobotConnectionManager.RobotWriteParams.button_pressed[0] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[1] = true;
        RobotConnectionManager.RobotWriteParams.button_pressed[2] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[3] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[4] = false;
        RobotConnectionManager.RobotWriteParams.button_pressed[5] = false;
        
        if (RobotConnectionManager.ConnectionControlStates.connect == true && RobotConnectionManager.RobotWriteParams.button_pressed[1] == true)
        {
            string[] speed_param = new string[6] { "-0.05", "0.0", "0.0", "0.0", "0.0", "0.0" };
            RobotConnectionManager.RobotWriteParams.auxCommand = "speedl([" + speed_param[0] + "," + speed_param[1]
                                                                 + "," + speed_param[2] + "," + speed_param[3]
                                                                 + "," + speed_param[4] + "," + speed_param[5]
                                                                 + "], a =" + acceleration + ", t =" + time + ")" +
                                                                 "\n";

            RobotConnectionManager.RobotWriteParams.command =
                encoder.GetBytes(RobotConnectionManager.RobotWriteParams.auxCommand);
            
            RobotConnectionManager.RobotWriteParams.button_pressed[0] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[1] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[2] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[3] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[4] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[5] = false;
        }
        else
        {
            string[] speed_param = new string[6] { "0.0", "0.0", "0.0", "0.0", "0.0", "0.0" };
            RobotConnectionManager.RobotWriteParams.auxCommand = "speedl([" + speed_param[0] + "," + speed_param[1]
                                                                 + "," + speed_param[2] + "," + speed_param[3]
                                                                 + "," + speed_param[4] + "," + speed_param[5]
                                                                 + "], a =" + acceleration + ", t =" + time + ")" +
                                                                 "\n";

            RobotConnectionManager.RobotWriteParams.command =
                encoder.GetBytes(RobotConnectionManager.RobotWriteParams.auxCommand);

            RobotConnectionManager.RobotWriteParams.button_pressed[0] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[1] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[2] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[3] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[4] = false;
            RobotConnectionManager.RobotWriteParams.button_pressed[5] = false;
        }
    }

    private void YPlusMovement(InputAction.CallbackContext context)
    {
        print("Y+");
    }

    private void YMinusMovement(InputAction.CallbackContext context)
    {
        print("Y-");
    }

    private void ZPlusMovement(InputAction.CallbackContext context)
    {
        print("Z+");
    }

    private void ZMinusMovement(InputAction.CallbackContext context)
    {
        print("Z-");
    }
}