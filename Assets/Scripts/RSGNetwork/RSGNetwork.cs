//Author: Jake Aquilina
//Creation Date: 07/07/20 10:24 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;

namespace RealSoftGames.Network
{
    public static class RSGNetwork
    {
        public static event Action OnConnected;

        public static event Action OnClientConnected;

        public static event Action OnDisconnected;

        private static int instanceID = 1;

        public static string IPAddress = "127.0.0.1";
        public static int PortNumber = 8080;
        private static TcpListener Server;
        private static TCP tcp;

        //private static TcpClient ClientSocket;
        public static List<Client> Clients = new List<Client>();

        public static int InstanceID { get; private set; }

        private static int GetNewInstanceID
        {
            get
            {
                int newID = instanceID;
                instanceID++;
                return newID;
            }
        }

        public static bool IsServer { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static Type[] GetAssemblies()
        {
            var result = new List<System.Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("Assembly-"))
                    continue;

                Type[] types = assembly.GetTypes();
                foreach (var T in types)
                {
                    result.Add(T);
                }
            }
            return result.ToArray();
        }

        private static void GetRPCs()
        {
            //HashTable.HashSet.Clear();
            var types = GetAssemblies();
            foreach (var type in types)
            {
                //MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    try
                    {
                        var att = method.GetCustomAttribute<RealSoftGames.Network.RPC>(true);

                        if (att != null && type.IsStatic() || att != null && method.IsStatic)
                        {
                            if (!HashTable.HashSet.ContainsKey(method.Name))
                            {
                                Debug.Log($"Adding Hash - {method.Name}");
                                HashTable.HashSet.Add(method.Name, method);
                                //HashTable.HashSet.Add(method.Name, method.CreateDeleage(null));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
            IsInitialized = true;
        }

        #region Server

        public static void StartServer()
        {
            Debug.Log("Starting server...");
            GetRPCs();

            Server = new TcpListener(System.Net.IPAddress.Any, PortNumber);
            Server.Start();
            Server.BeginAcceptTcpClient(TCPConnectCallback, null);
            InstanceID = 0;
            IsServer = true;
            Debug.Log($"Server Started on port: {PortNumber}");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = Server.EndAcceptTcpClient(result);
            Server.BeginAcceptTcpClient(TCPConnectCallback, null);
            Debug.Log($"Client Connecting {client.Client.RemoteEndPoint}...");
            Client newClient = new Client(client, GetNewInstanceID);
            newClient.tcp.Connect(client);
            Clients.Add(newClient);
            newClient.tcp.RPC("AssignInstanceID", newClient.InstanceID);
            Debug.Log($"{newClient.InstanceID} Connected");
            OnClientConnected?.Invoke();
        }

        public static void StopServer()
        {
            IsServer = false;
            Server.Stop();
        }

        #endregion Server

        #region Client

        public static void ConnectToServer(string serverIP, int portNumber)
        {
            GetRPCs();
            try
            {
                tcp = new TCP();
                tcp.Connect(serverIP, portNumber);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static void ConnectToServer()
        {
            GetRPCs();
            try
            {
                tcp = new TCP();
                tcp.Connect();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static void DisconnectFromServer()
        {
            tcp.Disconnect();
        }

        /// <summary>
        /// Send RPC to the server
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public static void ServerRPC(string methodName, params object[] parameters)
        {
            tcp.Send(methodName, parameters);
        }

        public static void RPC(int clientInstanceID, string methodName, params object[] parameters)
        {
            Clients.Find(i => i.InstanceID == clientInstanceID).tcp.RPC(methodName, parameters);
        }

        [RPC]
        private static void AssignInstanceID(int id)
        {
            Debug.Log($"Assigning Instance ID {id}");
            InstanceID = id;
        }

        #endregion Client

        public class TCP
        {
            public static int dataBufferSize = 4096;
            public TcpClient socket;
            private int id = 0;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(RSGNetwork.IPAddress, RSGNetwork.PortNumber, ConnectCallback, socket);
            }

            public void Connect(string IPAddress, int PortNumber)
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(IPAddress, PortNumber, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                    return;

                Debug.Log("Connected to server");
                OnConnected?.Invoke();
                stream = socket.GetStream();
                receivedData = new Packet();
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void Send(string methodName, params object[] parameters)
            {
                try
                {
                    if (socket != null)
                    {
                        byte[] serializedData = new Packet(InstanceID, methodName, parameters).Serialize();
                        stream.BeginWrite(serializedData, 0, serializedData.Length, null, null); // send to server
                    }
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error sending data to server via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Debug.Log($"Received some data {data.Length}");
                    Array.Copy(receiveBuffer, data, byteLength);
                    MainThreadDispatcher.AddMessage(data.Deserialize<Packet>());
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch
                {
                    Disconnect();
                }
            }

            public void Disconnect()
            {
                socket.Close();
                OnDisconnected?.Invoke();
            }
        }
    }
}