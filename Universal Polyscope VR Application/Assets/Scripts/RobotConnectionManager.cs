using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// A class that handles connectivity between the virtual and real robot over a IP/TCP connection
/// </summary>
/// <remarks>It is also responsible for reading and writing data between the two robots</remarks>
public class RobotConnectionManager : MonoBehaviour
{
    /// <summary>
    /// A class which initializes all the variables used when connecting/disconnecting to the robot
    /// </summary>
    public static class ConnectionControlStates
    {
        public static bool connect;
        public static bool disconnect;
    }

    /// <summary>
    /// A class which initializes all the variables used when reading data over TCP connection
    /// </summary>
    public static class RobotReadParams
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
    public static class RobotWriteParams
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

        // Control Parameters UR3/UR5:
        public static string auxCommand;

        public static byte[] command;

        // triggers for sending data
        public static bool[] button_pressed = new bool[12];
        public static bool joystick_button_pressed;

        // Class thread information (is alive or not)
        public static bool isAlive = false;
    }

    private RobotRead readData;
    private RobotWrite writeData;

    private int connectionStates = 0;
    private int aux_counter_pressed_btn = 0;

    private void Start()
    {
        // BUG: hard coding the ip addresses because of an issue around inputfields in VR builds
        RobotReadParams.ipAddress = "172.29.43.153";
        RobotWriteParams.ipAddress = "172.29.43.153";

        RobotReadParams.timeStep = 8;
        RobotWriteParams.timeStep = 8;

        // Start Data Stream & Control
        readData = new RobotRead();
        writeData = new RobotWrite();
    }

    private void FixedUpdate()
    {
        // checks if in connect or disconnect state
        switch (connectionStates)
        {
            // if in connect state, then start processes of read and write data
            case 0:
            {
                // ------------------------ Wait State {Disconnect State} ------------------------//
                if (ConnectionControlStates.connect == true)
                {
                    print("CONNECTED");
                    // Start Stream {Universal Robots TCP/IP}
                    readData.Start();
                    // Start Control {Universal Robots TCP/IP}
                    writeData.Start();

                    // go to connect state
                    connectionStates = 1;
                }
            }
                break;
            // if in disconnect state, then stop all processes of read and write data
            case 1:
            {
                // ------------------------ Data Processing State {Connect State} ------------------------//

                for (int i = 0; i < RobotWriteParams.button_pressed.Length; i++)
                {
                    // check the pressed button in joystick control mode
                    if (RobotWriteParams.button_pressed[i] == true)
                    {
                        aux_counter_pressed_btn++;
                    }
                }

                // at least one button pressed
                if (aux_counter_pressed_btn > 0)
                {
                    // start move -> speed control
                    RobotWriteParams.joystick_button_pressed = true;
                }
                else
                {
                    // stop move -> speed control
                    RobotWriteParams.joystick_button_pressed = false;
                }

                // null auxiliary variable
                aux_counter_pressed_btn = 0;

                if (ConnectionControlStates.disconnect == true)
                {
                    print("DISCONNECTED");
                    // Stop threading block {TCP/Ip -> read data}
                    if (RobotReadParams.isAlive == true)
                    {
                        readData.Stop();
                    }

                    // Stop threading block {TCP/Ip  -> write data}
                    if (RobotWriteParams.isAlive == true)
                    {
                        writeData.Stop();
                    }

                    if (RobotReadParams.isAlive == false && RobotWriteParams.isAlive == false)
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
            readData.Destroy();
            // Destroy Control {Universal Robots TCP/IP}
            writeData.Destroy();

            Destroy(this);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    /// <summary>
    /// A class which initializes all the variables used when reading data over TCP connection
    /// </summary>
    /// <remarks>
    /// The class is responsible for reading data from the robot and storing it in the appropriate variables.
    /// </remarks>
    class RobotRead
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
                    tcpClient.Connect(RobotReadParams.ipAddress, RobotReadParams.portNumber);
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
                                RobotReadParams.jointOrientation[0] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (32 * offset));
                                RobotReadParams.jointOrientation[1] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (33 * offset));
                                RobotReadParams.jointOrientation[2] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (34 * offset));
                                RobotReadParams.jointOrientation[3] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (35 * offset));
                                RobotReadParams.jointOrientation[4] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (36 * offset));
                                RobotReadParams.jointOrientation[5] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (37 * offset));

                                // Read Cartesian (Position) Values in metres
                                RobotReadParams.cartesianPosition[0] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (56 * offset));
                                RobotReadParams.cartesianPosition[1] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (57 * offset));
                                RobotReadParams.cartesianPosition[2] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (58 * offset));

                                // Read Cartesian (Orientation) Values in metres 
                                RobotReadParams.cartesianOrientation[0] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (59 * offset));
                                RobotReadParams.cartesianOrientation[1] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (60 * offset));
                                RobotReadParams.cartesianOrientation[2] = BitConverter.ToDouble(packet,
                                    packet.Length - firstPacketSize - (61 * offset));

                                // t_{1}: Timer stop.
                                timer.Stop();

                                // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                                if (timer.ElapsedMilliseconds < RobotReadParams.timeStep)
                                {
                                    Thread.Sleep(RobotReadParams.timeStep - (int)timer.ElapsedMilliseconds);
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
            RobotReadParams.isAlive = true;
        }

        public void Stop()
        {
            exitThread = true;
            // Stop a thread
            Thread.Sleep(100);
            RobotReadParams.isAlive = robotThread.IsAlive;
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
    /// A class which initializes all the variables used when sending data over TCP connection
    /// </summary>
    /// <remarks>
    /// The class is used to send commands to the robot
    /// </remarks>
    class RobotWrite
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
                    tcpClient.Connect(RobotWriteParams.ipAddress, RobotWriteParams.portNumber);
                }

                // Initialization TCP/IP Communication (Stream)
                networkStream = tcpClient.GetStream();

                // Initialization timer
                var timer = new Stopwatch();

                while (exitThread == false)
                {
                    // t_{0}: Timer start.
                    timer.Start();

                    if (RobotWriteParams.joystick_button_pressed == true)
                    {
                        // Send command (byte) -> speed control of the robot (X,Y,Z and EA{RX, RY, RZ})
                        networkStream.Write(RobotWriteParams.command, 0, RobotWriteParams.command.Length);
                    }

                    // t_{1}: Timer stop.
                    timer.Stop();

                    // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                    if (timer.ElapsedMilliseconds < RobotWriteParams.timeStep)
                    {
                        Thread.Sleep(RobotWriteParams.timeStep - (int)timer.ElapsedMilliseconds);
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
            RobotWriteParams.isAlive = true;
        }

        public void Stop()
        {
            exitThread = true;
            // Stop a thread
            Thread.Sleep(100);
            RobotWriteParams.isAlive = robotThread.IsAlive;
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