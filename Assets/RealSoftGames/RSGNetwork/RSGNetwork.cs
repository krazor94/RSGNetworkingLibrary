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
        private static string guid;

        public static Dictionary<string, Client> Clients = new Dictionary<string, Client>();

        public static string GUID { get => guid; }
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
            Server.BeginAccept(new AsyncCallback(TCPConnectCallback), null);

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
            SocketError socketError;
            //socket.Send(buff, 0, buff.Length, SocketFlags.None, out socketError);
            SendState state = new SendState();
            state.dataToSend = new byte[buff.Length + 4];
            state.socket = socket;
            byte[] prefix = BitConverter.GetBytes(buff.Length);
            Buffer.BlockCopy(prefix, 0, state.dataToSend, 0, prefix.Length);
            Buffer.BlockCopy(buff, 0, state.dataToSend, prefix.Length, buff.Length);
            Debug.Log($"Sending GUID to Client {clientsGUID} - Size:{state.dataToSend.Length}");
            socket.BeginSend(state.dataToSend, 0, state.dataToSend.Length, SocketFlags.None, out socketError, new AsyncCallback(TCPConnectSendCallback), state);
            //socket.Send(state.dataToSend, 0, state.dataToSend.Length, SocketFlags.None, out socketError);
            if (socketError != SocketError.Success)
                Debug.LogError($"TCPConnectCallback SocketError: {socketError}");
            //socket.BeginSend(clientsGUID.Serialize(), 0, 32, SocketFlags.None, null, null);

            Server.BeginAccept(new AsyncCallback(TCPConnectCallback), null);

            Debug.Log($" {socket.RemoteEndPoint} Client Connecting...");
            Client newClient = new Client(socket, clientsGUID);
            newClient.tcp.Connect(socket);
        }

        private static void TCPConnectSendCallback(IAsyncResult result)
        {
            SendState state = (SendState)result.AsyncState;
            SocketError socketError;
            int sentData = state.socket.EndSend(result, out socketError);

            if (socketError != SocketError.Success)
            {
                Debug.LogError($"SocketError: {socketError}");
                tcp.Disconnect();
                return;
            }

            state.dataSent += sentData;
            //Debug.Log($"DataSent:{state.dataSent}, DataToSend:{state.dataToSend.Length}");
            if (state.dataSent != state.dataToSend.Length)
                state.socket.BeginSend(state.dataToSend, state.dataSent, state.dataToSend.Length - state.dataSent, SocketFlags.None, new AsyncCallback(TCPConnectSendCallback), state);
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
            public static int dataBufferSize = 1024;
            public Socket socket;
            //private Packet receivedData;
            //private byte[] receiveBuffer;

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

                //receiveBuffer = new byte[dataBufferSize];
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
                //receivedData = new Packet();
                socket.BeginReceive(state.Buffer, 0, dataBufferSize, SocketFlags.None, new AsyncCallback(InitReceiveCallback), state);

                //socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
            }

            private void InitReceiveCallback(IAsyncResult result)
            {
                try
                {
                    Debug.Log("InitReceiveCallback Client");
                    ReceiveState state = (ReceiveState)result.AsyncState;
                    SocketError socketError;

                    int byteLength = socket.EndReceive(result, out socketError);
                    int dataOffset = 0;

                    if (socketError != SocketError.Success)
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
                        Debug.Log("Received full packet");
                        state.Data.Write(state.Buffer, dataOffset, byteLength);

                        guid = state.Data.ToArray().Deserialize<string>();
                        ReceiveState newState = new ReceiveState();
                        socket.BeginReceive(newState.Buffer, 0, dataBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), newState);
                        Debug.Log($"Connected to server - MyGUID: {guid}");
                        IsConnectedToServer = true;
                        OnConnected?.Invoke();
                    }
                    else
                    {
                        //Debug.Log($"Has not finished receiving data, begin receiving again, Received: {state.Data.Length + byteLength}, Expected:{state.DataSize}");
                        state.Data.Write(state.Buffer, dataOffset, byteLength);
                        socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(InitReceiveCallback), state);
                    }

                    //guid = receiveBuffer.Deserialize<string>();
                    //Debug.Log($"Received:{receiveBuffer.Length} bytes: {guid}");
                    //byte[] data = new byte[receiveBuffer.Length];
                    //Array.Copy(receiveBuffer, data, receiveBuffer.Length);
                    //socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);

                    //Debug.Log("Connected to server");
                    //IsConnectedToServer = true;
                    //OnConnected?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"InitReceiveCallback Failed: {e.Message}");
                    Disconnect();
                }
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
                            MainThreadDispatcher.AddMessage(packet);
                        else
                            Debug.LogError($"Cant have a null method name in packet!");

                        //MainThreadDispatcher.AddMessage(data.Deserialize<Packet>());

                        socket.BeginReceive(newState.Buffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, newState);
                    }
                    else
                    {
                        //Debug.LogError($"Has not yet received all the data, waiting for more to come in");
                        state.Data.Write(state.Buffer, dataOffset, byteLength);
                        socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }

                    //byte[] data = new byte[byteLength];
                    //Debug.Log($"Received some data {byteLength}");
                    //Array.Copy(receiveBuffer, data, byteLength);

                    //receivedData = data.Deserialize<Packet>();
                    //if (!string.IsNullOrEmpty(receivedData.MethodName))
                    //    MainThreadDispatcher.AddMessage(receivedData);
                    //else
                    //    Debug.LogError($"Cant have a null method name in packet!");
                    //
                    //socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ReceiveCallback Failed: {e.Message}");
                    Disconnect();
                }
            }

            public void Send(string methodName, string callback, params object[] parameters)
            {
                try
                {
                    if (socket != null && socket.IsConnected())
                    {
                        byte[] serializedData = new Packet(guid, methodName, callback, parameters).Serialize();
                        SendState state = new SendState();
                        state.socket = socket;
                        state.dataToSend = new byte[serializedData.Length + 4];
                        byte[] prefix = BitConverter.GetBytes(serializedData.Length);
                        Buffer.BlockCopy(prefix, 0, state.dataToSend, 0, prefix.Length);
                        Buffer.BlockCopy(serializedData, 0, state.dataToSend, prefix.Length, serializedData.Length);
                        socket.BeginSend(state.dataToSend, 0, state.dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), state);
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
                }
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