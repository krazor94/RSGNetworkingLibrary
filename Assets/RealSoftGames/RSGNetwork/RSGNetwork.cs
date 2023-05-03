//Author: Jake Aquilina
//Creation Date: 07/07/20 10:24 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace RealSoftGames.Network
{
    public static class RSGNetwork
    {
        #region Run on client machine

        /// <summary>
        /// Called on the client when it is connected to the server
        /// </summary>
        public static event Action OnConnected;

        /// <summary>
        /// Called on the client when the client disconnects from the server
        /// </summary>
        public static event Action OnDisconnected;

        #endregion Run on client machine

        public static string IPAddress = "127.0.0.1";
        public static int PortNumber = 8095;
        private static Socket Server;
        private static TCP tcp;

        public static Dictionary<string, Client> Clients = new Dictionary<string, Client>();

        public static bool IsConnectedToServer { get; private set; }
        public static bool IsServer { get; private set; }
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Poll the server to check for connection
        /// </summary>
        public static bool IsConencted { get => tcp.socket.IsConnected(); }

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

        private static void ClientConnected(Client client)
        {
            Debug.LogError($"{client.tcp.socket.RemoteEndPoint} Connected Adding to Clients");
            Clients.Add(client.tcp.socket.RemoteEndPoint.ToString(), client);
        }

        private static void ClientDisconnected(Client client)
        {
            Debug.LogError($"{client.tcp.socket.RemoteEndPoint} Disconnected");
            Clients.Remove(client.tcp.socket.RemoteEndPoint.ToString());
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
            Server.BeginAccept(new AsyncCallback(TCPConnectCallback), null);

            IsServer = true;
            Debug.Log($"Server Started on port: {PortNumber}");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            Socket socket = Server.EndAccept(result);
            Server.BeginAccept(new AsyncCallback(TCPConnectCallback), null);

            Debug.Log($" {socket.RemoteEndPoint} Client Connecting...");
            Client newClient = new Client(socket);
            newClient.tcp.Connect(socket);
        }

        public static void StopServer()
        {
            IsServer = false;
            //Server.Stop();
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

        public static void ServerRPCCallback(string methodName, string callback, params object[] parameters)
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
        public static void RPC(Client client, string methodName, string callback = null, params object[] parameters)
        {
            if (Clients.TryGetValue(client.tcp.socket.RemoteEndPoint.ToString(), out var value))
                value.tcp.RPC(methodName, callback, parameters);
            else
                Debug.LogError($"Client:{client.tcp.socket.RemoteEndPoint} is not connected");
        }

        #endregion Client

        public class TCP
        {
            public static int dataBufferSize = 1024;
            public Socket socket;

            public void Connect()
            {
                Debug.Log("Connecting to server...");
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                //receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(IPAddress, PortNumber, new AsyncCallback(ConnectCallback), socket);
            }

            public void Connect(string IPAddress, int PortNumber)
            {
                Debug.Log("Connecting to server...");
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                socket.BeginConnect(IPAddress, PortNumber, new AsyncCallback(ConnectCallback), socket);
            }

            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                {
                    Debug.LogError("Socket is not connected");
                    return;
                }

                ReceiveState state = new ReceiveState();
                socket.BeginReceive(state.Buffer, 0, dataBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                IsConnectedToServer = true;
                OnConnected?.Invoke();
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    ReceiveState state = (ReceiveState)result.AsyncState;
                    SocketError socketError;

                    int byteLength = socket.EndReceive(result, out socketError);
                    int dataOffset = 0;

                    //int byteLength = socket.EndReceive(result);

                    if (socketError != SocketError.Success || !socket.IsConnected())
                    {
                        Debug.LogError($"Socket Error:{socketError}");
                        Disconnect();
                        return;
                    }

                    if (byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    if (!state.DataSizeReceived)
                    {
                        if (byteLength >= 4)
                        {
                            state.DataSize = BitConverter.ToInt32(state.Buffer, 0);
                            state.DataSizeReceived = true;
                            byteLength -= 4;
                            dataOffset += 4;
                        }
                    }

                    if ((state.Data.Length + byteLength) == state.DataSize)
                    {
                        state.Data.Write(state.Buffer, dataOffset, byteLength);

                        ReceiveState newState = new ReceiveState();
                        Packet packet = state.Data.ToArray().Deserialize<Packet>();

                        if (!string.IsNullOrEmpty(packet.MethodName))
                        {
                            Debug.Log($"Received Packet from {packet.RemoteEndPoint} Packet: MethodName:{packet.MethodName} Callback:{packet.Callback}");
                            MainThreadDispatcher.AddMessage(packet);
                        }
                        else
                            Debug.LogError($"Cant have a null method name in packet!");

                        socket.BeginReceive(newState.Buffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, newState);
                    }
                    else if ((state.Data.Length + byteLength) > state.DataSize)
                    {
                        Debug.LogError("Packet is messed up big time!");
                    }
                    else
                    {
                        Debug.LogError($"Has not yet received all the data, waiting for more to come in");
                        state.Data.Write(state.Buffer, dataOffset, byteLength);
                        socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"ReceiveCallback Failed: {e.Message}");
                    Disconnect();
                }
            }

            private bool sending = false;

            public void Send(string methodName, string callback, params object[] parameters)
            {
                try
                {
                    if (socket != null && socket.IsConnected())
                    {
                        if (sending)
                        {
                            Debug.LogError("Message already being sent, add next message to a qeue");
                            //Send(methodName, callback, parameters);
                            return;
                        }

                        sending = true;
                        byte[] serializedData = new Packet(tcp.socket.RemoteEndPoint.ToString(), methodName, callback, parameters).Serialize();
                        SendState state = new SendState();
                        state.socket = socket;
                        state.dataToSend = new byte[serializedData.Length + 4];
                        byte[] prefix = BitConverter.GetBytes(serializedData.Length);
                        Buffer.BlockCopy(prefix, 0, state.dataToSend, 0, prefix.Length);
                        Buffer.BlockCopy(serializedData, 0, state.dataToSend, prefix.Length, serializedData.Length);
                        socket.BeginSend(state.dataToSend, 0, state.dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), state);
                        Debug.Log($"Send RPC: {methodName}");
                        //socket.BeginSend(serializedData, 0, serializedData.Length, SocketFlags.None, null, null);
                    }
                    else
                        Debug.LogError("Socket is null! || Disconnected");
                }
                catch (Exception e)
                {
                    Debug.Log($"Error sending data to server via TCP: {e.Message}");
                }
            }

            private void SendCallback(IAsyncResult result)
            {
                SendState state = (SendState)result.AsyncState;
                SocketError socketError;
                int sentData = socket.EndSend(result, out socketError);

                if (socketError != SocketError.Success || !socket.IsConnected())
                {
                    Debug.LogError($"SocketError: {socketError}");
                    Disconnect();
                    return;
                }

                state.dataSent += sentData;
                //Debug.Log($"DataSent:{state.dataSent}, DataToSend:{state.dataToSend.Length}");

                if (state.dataSent != state.dataToSend.Length)
                {
                    //Debug.LogError($"Not all data was sent, sending the rest now: {state.dataToSend.Length - state.dataSent}");
                    socket.BeginSend(state.dataToSend, state.dataSent, state.dataToSend.Length - state.dataSent, SocketFlags.None, new AsyncCallback(SendCallback), state);
                    sending = true;
                }
                else
                    sending = false;
                //else
                //    Debug.Log($"All Data was sent {state.dataToSend.Length}");
            }

            public void Disconnect()
            {
                if (IsConnectedToServer)
                {
                    IsConnectedToServer = false;
                    Debug.LogError("Disconnecting from server");
                    socket.Disconnect(false);
                    socket.Close();
                    OnDisconnected?.Invoke();
                }
            }
        }
    }
}