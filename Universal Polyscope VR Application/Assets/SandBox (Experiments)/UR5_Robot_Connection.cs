using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace _Project.Scripts.Connection
{
    /****************************************************************************
     * 
     * Add this script to TODO: Enter the gameObject's name in which this script needs to be attach/add
     * 
    ****************************************************************************/
    
    // TODO: Connect UR5 Robot (virtual + real)
    /// <summary>
    /// A class which allows users to create a connection between a real UR5 robot and a virtual UR5 robot.
    /// This connection needs to be created so that data can be transferred/shared between the two things.
    /// </summary>
    public class UR5_Robot_Connection : MonoBehaviour
    {
        /// <summary>
        /// A class which contains static global variables for easy access for all classes.
        /// The variables are like enum variables and they act as states.
        /// </summary>
        public static class ConnectionControlStates
        {
            public static bool connect;
            public static bool disconnect;
        }

        /// <summary>
        /// A class which initializes all the variables used when reading data over TCP connection
        /// </summary>
        public static class UR5_Data_Stream
        {
            // ip address
            public static string ipAddress;

            // port number
            public const ushort portNumber = 30013;

            /// <summary>
            /// TimeSteps are ticks of time. It is how long in time each of your samples is.
            /// For example, a sample can contain 128-time steps, where each time steps could be a 30th of a second for signal processing.
            /// </summary>
            /// <remarks>Communication Speed</remarks>
            public static int timeStep;

            // An array of all joints' orientations (in joint space)
            public static double[] jointOrientation = new double[6];

            // An array of all joints' position (in cartesian space)
            public static double[] cartesianPosition = new double[3];

            // An array of all joints' orientations (in cartesian space)
            public static double[] cartesianOrientation = new double[3];

            // Class thread information (is alive or not)
            public static bool isAlive = false;
        }

        /// <summary>
        /// A class which initializes all the variables used when writing data over TCP connection
        /// </summary>
        public static class UR5_Data_Control
        {
            // ip address
            public static string ipAddress;

            // port number
            public const ushort portNumber = 30003;

            /// <summary>
            /// TimeSteps are ticks of time. It is how long in time each of your samples is.
            /// For example, a sample can contain 128-time steps, where each time steps could be a 30th of a second for signal processing.
            /// </summary>
            /// <remarks>Communication Speed</remarks>
            public static int timeStep;

            // Control Parameters UR3/UR3e:
            public static string auxCommand;

            public static byte[] command;

            public static bool[] button_pressed = new bool[12];
            public static bool joystick_button_pressed;

            // Class thread information (is alive or not)
            public static bool isAlive = false;
        }

        private UR5_Stream dataStream;
        private UR5_Control dataControl;

        private int connectionStates = 0;
        private int aux_counter_pressed_btn = 0;

        /// <summary>
        /// Initializing variables at start of application
        /// <remarks>Start is called before the first frame update</remarks>
        /// </summary>
        private void Start()
        {
            // BUG: hard coding the ip addreses becuase of an issue around inputfields in VR builds
            UR5_Data_Stream.ipAddress = "172.29.43.153";
            UR5_Data_Control.ipAddress = "172.29.43.153";

            //  Communication speed: CB-Series 125 Hz (8 ms), E-Series 500 Hz (2 ms)
            UR5_Data_Stream.timeStep = 8;
            //  Communication speed: CB-Series 125 Hz (8 ms), E-Series 500 Hz (2 ms)
            UR5_Data_Control.timeStep = 8;

            // Start Data Stream & Control
            dataStream = new UR5_Stream();
            dataControl = new UR5_Control();
        }
        
        private void FixedUpdate()
        {
            // checks if in connect or disconnect state
            // if in connect state, then start processes of read and write data
            // if in disconnect state, then stop all processes of read and write data
            switch (connectionStates)
            {
                case 0:
                {
                    // ------------------------ Wait State {Disconnect State} ------------------------//

                    if (ConnectionControlStates.connect == true)
                    {
                        print("CONNECTED");
                        // Start Stream {Universal Robots TCP/IP}
                        dataStream.Start();
                        // Start Control {Universal Robots TCP/IP}
                        dataControl.Start();

                        // go to connect state
                        connectionStates = 1;
                    }
                }
                    break;
                case 1:
                {
                    // ------------------------ Data Processing State {Connect State} ------------------------//

                    for (int i = 0; i < UR5_Data_Control.button_pressed.Length; i++)
                    {
                        // check the pressed button in joystick control mode
                        if (UR5_Data_Control.button_pressed[i] == true)
                        {
                            aux_counter_pressed_btn++;
                        }
                    }

                    // at least one button pressed
                    if (aux_counter_pressed_btn > 0)
                    {
                        // start move -> speed control
                        UR5_Data_Control.joystick_button_pressed = true;
                    }
                    else
                    {
                        // stop move -> speed control
                        UR5_Data_Control.joystick_button_pressed = false;
                    }

                    // null auxiliary variable
                    aux_counter_pressed_btn = 0;

                    if (ConnectionControlStates.disconnect == true)
                    {
                        print("DISCONNECTED");
                        // Stop threading block {TCP/Ip -> read data}
                        if (UR5_Data_Stream.isAlive == true)
                        {
                            dataStream.Stop();
                        }

                        // Stop threading block {TCP/Ip  -> write data}
                        if (UR5_Data_Control.isAlive == true)
                        {
                            dataControl.Stop();
                        }

                        if (UR5_Data_Stream.isAlive == false && UR5_Data_Control.isAlive == false)
                        {
                            // go to initialization state {wait state -> disconnect state}
                            connectionStates = 0;
                        }
                    }
                }
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            try
            {
                // Destroy Stream {Universal Robots TCP/IP}
                dataStream.Destroy();
                // Destroy Control {Universal Robots TCP/IP}
                dataControl.Destroy();

                Destroy(this);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        /// <summary>
        /// A class that initiates the tcp connection between the virtual and real robot.
        /// </summary>
        class UR5_Stream
        {
            private Thread robotThread = null;
            private bool exitThread = false;
            private TcpClient tcpClient = new TcpClient();
            private NetworkStream networkStream = null;
            private byte[] packet = new byte[1116];
            private const byte firstPacketSize = 4;
            // offset: size of otehr packets in bytes (Double)
            private const byte offset = 8;
            private const UInt32 totalMsgLength = 3288596480;

            public void UR5_Stream_Thread()
            {
                try
                {
                    // Connect if not connected already
                    if (tcpClient.Connected == false)
                    {
                        tcpClient.Connect(UR5_Data_Stream.ipAddress, UR5_Data_Stream.portNumber);
                    }

                    // Initialization TCP/IP Communication (Stream)
                    networkStream = tcpClient.GetStream();

                    // Initialization timer
                    var timer = new Stopwatch();

                    while (exitThread == false)
                    {
                        // Get the data from the robot
                        if (networkStream.Read(packet, 0, packet.Length) != 0)
                        {
                            if (BitConverter.ToUInt32(packet, firstPacketSize - 4) == totalMsgLength)
                            {
                                // t_{0}: Timer start.
                                timer.Start();

                                // Reverses the order of elements in a one-dimensional array or part of an array.
                                Array.Reverse(packet);

                                // Read Joint Values in radians
                                UR5_Data_Stream.jointOrientation[0] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (32 * offset));
                                UR5_Data_Stream.jointOrientation[1] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (33 * offset));
                                UR5_Data_Stream.jointOrientation[2] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (34 * offset));
                                UR5_Data_Stream.jointOrientation[3] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (35 * offset));
                                UR5_Data_Stream.jointOrientation[4] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (36 * offset));
                                UR5_Data_Stream.jointOrientation[5] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (37 * offset));
                                
                                // Read Cartesian (Position) Values in metres
                                UR5_Data_Stream.cartesianPosition[0] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (56 * offset));
                                UR5_Data_Stream.cartesianPosition[1] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (57 * offset));
                                UR5_Data_Stream.cartesianPosition[2] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (58 * offset));
                                
                                // Read Cartesian (Orientation) Values in metres 
                                UR5_Data_Stream.cartesianOrientation[0] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (59 * offset));
                                UR5_Data_Stream.cartesianOrientation[1] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (60 * offset));
                                UR5_Data_Stream.cartesianOrientation[2] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (61 * offset));

                                // t_{1}: Timer stop.
                                timer.Stop();

                                // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                                if (timer.ElapsedMilliseconds < UR5_Data_Stream.timeStep)
                                {
                                    Thread.Sleep(UR5_Data_Stream.timeStep - (int)timer.ElapsedMilliseconds);
                                }

                                // Reset (Restart) timer.
                                timer.Restart();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            public void Start()
            {
                // Start thread
                exitThread = false;
                // Start a thread and listen to incoming messages
                robotThread = new Thread(new ThreadStart(UR5_Stream_Thread));
                robotThread.IsBackground = true;
                robotThread.Start();
                // Thread is active
                UR5_Data_Stream.isAlive = true;
            }

            public void Stop()
            {
                exitThread = true;
                // Stop a thread
                Thread.Sleep(100);
                UR5_Data_Stream.isAlive = robotThread.IsAlive;
                robotThread.Abort();
            }

            public void Destroy()
            {
                if (tcpClient.Connected == true)
                {
                    // Disconnect communication
                    networkStream.Dispose();
                    tcpClient.Close();
                }

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// A class that handles the controls side of the application
        /// Such as when user uses joystick, then this class deals with all packets for handling the read/write bits of the joysticks
        /// </summary>
        class UR5_Control
        {
            private Thread robotThread = null;
            private bool exitThread = false;
            private TcpClient tcpClient = new TcpClient();
            private NetworkStream networkStream = null;

            public void UR_Control_Thread()
            {
                try
                {
                    // if controller is disconnected, then connect it
                    if (tcpClient.Connected != true)
                    {
                        tcpClient.Connect(UR5_Data_Control.ipAddress, UR5_Data_Control.portNumber);
                    }

                    // Initialization TCP/IP Communication (Stream)
                    networkStream = tcpClient.GetStream();

                    // Initialization timer
                    var timer = new Stopwatch();

                    while (exitThread == false)
                    {
                        // t_{0}: Timer start.
                        timer.Start();
                        
                        if (UR5_Data_Control.joystick_button_pressed == true)
                        {
                            // Send command (byte) -> speed control of the robot (X,Y,Z and EA{RX, RY, RZ})
                            networkStream.Write(UR5_Data_Control.command, 0, UR5_Data_Control.command.Length);
                        }

                        // t_{1}: Timer stop.
                        timer.Stop();

                        // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                        if (timer.ElapsedMilliseconds < UR5_Data_Control.timeStep)
                        {
                            Thread.Sleep(UR5_Data_Control.timeStep - (int)timer.ElapsedMilliseconds);
                        }

                        // Reset (Restart) timer.
                        timer.Restart();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            public void Start()
            {
                // Start thread
                exitThread = false;
                // Start a thread and listen to incoming messages
                robotThread = new Thread(new ThreadStart(UR_Control_Thread));
                robotThread.IsBackground = true;
                robotThread.Start();
                // Thread is active
                UR5_Data_Control.isAlive = true;
            }

            public void Stop()
            {
                exitThread = true;
                // Stop a thread
                Thread.Sleep(100);
                UR5_Data_Control.isAlive = robotThread.IsAlive;
                robotThread.Abort();
            }

            public void Destroy()
            {
                if (tcpClient.Connected == true)
                {
                    // Disconnect communication
                    networkStream.Dispose();
                    tcpClient.Close();
                }

                Thread.Sleep(100);
            }
        }
    }
}