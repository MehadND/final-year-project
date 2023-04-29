using System;
using System.Text;
using _Project.Scripts.Connection;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    /****************************************************************************
     * 
     * Add this script to Canvas or Main (Parent) Pane
     * 
    ****************************************************************************/
    
    /// <summary>
    /// This class handles the main UI controls
    /// </summary>
    public class UI_Controller : MonoBehaviour
    {
        // BUG: an issue around inputfields in VR builds where keyboard wont show on field select
        public TMP_InputField ipAddressField;

        // 
        //public TextMeshProUGUI position_x_txt, position_y_txt, position_z_txt;
        //public TextMeshProUGUI position_rx_txt, position_ry_txt, position_rz_txt;
        //public TextMeshProUGUI position_j1_txt, position_j2_txt, position_j3_txt;
        //public TextMeshProUGUI position_j4_txt, position_j5_txt, position_j6_txt;

        // Text element for displaying the connection status
        public TextMeshProUGUI connectionStatusText;

        private UTF8Encoding utf8 = new UTF8Encoding();

        private String ipAddress = "172.29.43.153";

        /// <summary>
        /// Initializing variables to default values
        /// </summary>
        private void Start()
        {
            connectionStatusText.text = "Disconnect";

            // BUG: hard coding the ip addreses becuase of an issue around inputfields in VR builds
            ipAddressField.text = "172.29.43.153";

            // Auxiliary first command -> Write initialization position/rotation with acceleration/time to the robot controller
            // command (string value)
            UR5_Robot_Connection.UR5_Data_Control.auxCommand =
                "speedl([0.0,0.0,0.0,0.0,0.0,0.0], a = 0.15, t = 0.03)" + "\n";
            // get bytes from string command
            UR5_Robot_Connection.UR5_Data_Control.command =
                utf8.GetBytes(UR5_Robot_Connection.UR5_Data_Control.auxCommand);
        }

        private void FixedUpdate()
        {
            // Robot IP Address (Read) -> TCP/IP 
            UR5_Robot_Connection.UR5_Data_Stream.ipAddress = ipAddressField.text;
            // Robot IP Address (Write) -> TCP/IP 
            UR5_Robot_Connection.UR5_Data_Control.ipAddress = ipAddressField.text;

            // if connection is made, then change text of status to "connect", else set it to "disconnet
            if (UR5_Robot_Connection.ConnectionControlStates.connect == true)
            {
                // green color
                //connection_info_img.GetComponent<Image>().color = new Color32(135, 255, 0, 50);
                connectionStatusText.text = "Connect";
            }
            else if (UR5_Robot_Connection.ConnectionControlStates.disconnect == true)
            {
                // red color
                //connection_info_img.GetComponent<Image>().color = new Color32(255, 0, 48, 50);
                connectionStatusText.text = "Disconnect";
            }
            
            for(int i = 0; i < 6; i++)
                    Debug.Log("Joint #"+i+": " + RobotConnectionManager.RobotReadParams.jointOrientation[i].ToString());
            
        }

        private void OnApplicationQuit()
        {
            // Destroy all
            Destroy(this);
        }

        public void ConnectionPanelControl()
        {
        }

        /// <summary>
        /// This method sets the connection status to connect so that a connection can be established between the two devices.
        /// <remarks>This is usually added on a button click event</remarks>
        /// </summary>
        public void ConnectButton()
        {
            UR5_Robot_Connection.ConnectionControlStates.connect    = true;
            UR5_Robot_Connection.ConnectionControlStates.disconnect = false;
        }

        /// <summary>
        /// This method sets the connection status to disconnect so that a connection can be stopped between the two devices.
        /// <remarks>This is usually added on a button click event</remarks>
        /// </summary>
        public void DisconnectButton()
        {
            UR5_Robot_Connection.ConnectionControlStates.connect    = false;
            UR5_Robot_Connection.ConnectionControlStates.disconnect = true;
        }
    }
}