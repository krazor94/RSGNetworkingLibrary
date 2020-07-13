//Author: Jake Aquilina
//Creation Date: 07/07/20 10:24 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace RealSoftGames.Network
{
    public static class RSGNetwork
    {
        #region Run on client machine

        public static event Action OnConnected;

        public static event Action OnDisconnected;

        #endregion Run on client machine

        public static string IPAddress = "127.0.0.1";
        public static int PortNumber = 8080;
        private static Socket Server;
        private static TCP tcp;
        private static string guid;

        public static Dictionary<string, Client> Clients = new Dictionary<string, Client>();

        public static string GUID { get => guid; }
        public static bool IsConnectedToServer { get; private set; }
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

        private static string GetGUID()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }

        private static void ClientConnected(Client client)
        {
            Debug.LogError($"{client.GUID} - {client.tcp.socket.RemoteEndPoint} Connected");
            Clients.Add(client.GUID, client);
        }

        private static void ClientDisconnected(Client client)
        {
            Debug.LogError($"{client.GUID} - {client.tcp.socket.RemoteEndPoint} Disconnected");
            Clients.Remove(client.GUID);
        }

        public static void StartServer()
        {
            Debug.Log("Starting server...");
            GetRPCs();
            Client.OnClientConnected += ClientConnected;
            Client.OnClientDisconnected += ClientDisconnected;

            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// System.Net.IPAddress.Any, PortNumber);
            Server.Bind(new IPEndPoint(System.Net.IPAddress.Any, PortNumber));
            Server.Listen(0);
            Server.BeginAccept(TCPConnectCallback, null);

            //Server.Start();
            //Server.BeginAcceptTcpClient(TCPConnectCallback, null);
            //Server.Server.BeginAccept(Server.Server.ReceiveBufferSize, TCPConnectCallback, null);
            IsServer = true;
            Debug.Log($"Server Started on port: {PortNumber}");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            string clientsGUID = GetGUID();
            //TcpClient client = Server.EndAcceptTcpClient(result);
            //Server.BeginAcceptTcpClient(TCPConnectCallback, null);
            Socket socket = Server.EndAccept(result);

            byte[] buff = clientsGUID.Serialize();
            socket.Send(buff, 0, buff.Length, SocketFlags.None);
            //socket.BeginSend(clientsGUID.Serialize(), 0, 32, SocketFlags.None, null, null);

            Server.BeginAccept(TCPConnectCallback, null);

            Debug.Log($"Client Connecting {socket.RemoteEndPoint}...");
            Client newClient = new Client(socket, clientsGUID);
            newClient.tcp.Connect(socket);
        }

        public static void StopServer()
        {
            IsServer = false;
            //Server.Stop();
            Server.Disconnect(false);
            Server.Close();

            Client.OnClientDisconnected -= ClientDisconnected;
            Client.OnClientConnected -= ClientConnected;
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
            tcp.socket.Close();
        }

        /// <summary>
        /// Send RPC to the server
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public static void ServerRPC(string methodName, params object[] parameters)
        {
            tcp.Send(methodName, null, parameters);
        }

        public static void ServerRPC(string methodName, string callback = null, params object[] parameters)
        {
            tcp.Send(methodName, callback, parameters);
        }

        /// <summary>
        /// Send RPC to Client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="methodName"></param>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        public static void RPC(string guid, string methodName, string callback = null, params object[] parameters)
        {
            if (Clients.TryGetValue(guid, out var value))
                value.tcp.RPC(methodName, callback, parameters);
            else
                Debug.LogError($"Client:{guid} is not connected");
        }

        #endregion Client

        public class TCP
        {
            public static int dataBufferSize = 4096;
            public Socket socket;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(IPAddress, PortNumber, ConnectCallback, socket);
            }

            public void Connect(string IPAddress, int PortNumber)
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(IPAddress, PortNumber, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                    return;

                receivedData = new Packet();
                socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, InitReceiveCallback, null);

                //socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
            }

            private void InitReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = socket.EndReceive(result);
                    if (byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }
                    guid = receiveBuffer.Deserialize<string>();
                    //Debug.Log($"Received:{receiveBuffer.Length} bytes: {guid}");
                    //byte[] data = new byte[receiveBuffer.Length];
                    //Array.Copy(receiveBuffer, data, receiveBuffer.Length);
                    socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);

                    Debug.Log("Connected to server");
                    IsConnectedToServer = true;
                    OnConnected?.Invoke();
                }
                catch
                {
                    Disconnect();
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = socket.EndReceive(result);
                    if (byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    //Debug.Log($"Received some data {byteLength}");
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData = data.Deserialize<Packet>();
                    if (!string.IsNullOrEmpty(receivedData.MethodName))
                        MainThreadDispatcher.AddMessage(receivedData);
                    else
                        Debug.LogError($"Cant have a null method name in packet!");

                    socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
                }
                catch
                {
                    Disconnect();
                }
            }

            public void Send(string methodName, string callback, params object[] parameters)
            {
                try
                {
                    if (socket != null)
                    {
                        byte[] serializedData = new Packet(guid, methodName, callback, parameters).Serialize();
                        socket.BeginSend(serializedData, 0, serializedData.Length, SocketFlags.None, null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data to server via TCP: {ex}");
                }
            }

            public void Disconnect()
            {
                socket.Close();
                OnDisconnected?.Invoke();
                IsConnectedToServer = false;
            }
        }
    }
}