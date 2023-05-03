//Author: Jake Aquilina
//Creation Date: 07/07/20 10:12 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.IO;
using System.Net.Sockets;

namespace RealSoftGames.Network
{
    [System.Serializable]
    public class Client
    {
        public Client(Socket client)
        {
            tcp = new TCP(client, this);
        }

        /// <summary>
        /// Called on the server when a new client connects
        /// </summary>
        public static event Action<Client> OnClientConnected;

        /// <summary>
        /// Called on the server when a client disconnects
        /// </summary>
        public static event Action<Client> OnClientDisconnected;

        public static int dataBufferSize = 1024;
        public TCP tcp;

        public void Disconnect()
        {
            tcp.Disconnect();
        }

        public class TCP
        {
            public TCP(Socket socket, Client client)
            {
                this.socket = socket;
                this.client = client;
            }

            public static int dataBufferSize = 1024;
            public Socket socket;
            private bool isConnected = false;
            public readonly Client client;

            public void Connect(Socket tcpSocket)
            {
                ReceiveState state = new ReceiveState();
                socket = tcpSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;
                state.Buffer = new byte[dataBufferSize];

                socket.BeginReceive(state.Buffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, state);
                isConnected = true;
                OnClientConnected?.Invoke(client);
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    ReceiveState state = (ReceiveState)result.AsyncState;
                    SocketError socketError;

                    //int byteLength = stream.EndRead(result);
                    int byteLength = socket.EndReceive(result, out socketError);
                    int dataOffset = 0;

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

                        Packet packet = state.Data.ToArray().Deserialize<Packet>();
                        ReceiveState newState = new ReceiveState();

                        if (!string.IsNullOrEmpty(packet.MethodName))
                        {
                            Debug.Log($"Received packet from {socket.RemoteEndPoint}");
                            MainThreadDispatcher.AddMessage(packet);
                        }
                        else
                            Debug.LogError($"Cant have a null method name in packet!");

                        socket.BeginReceive(newState.Buffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, newState);
                    }
                    else
                    {
                        Debug.LogError($"Has not yet received all the data, waiting for more to come in Expected:{state.DataSize} Received:{state.Data.Length + byteLength}");
                        state.Data.Write(state.Buffer, dataOffset, byteLength);
                        socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Disconnect();
                }
            }

            public void RPC(string methodName, string callback = null, params object[] parameters)
            {
                try
                {
                    if (socket != null && socket.IsConnected())
                    {
                        byte[] serializedData = new Packet(socket.RemoteEndPoint.ToString(), methodName, callback, parameters).Serialize();
                        SendState state = new SendState();
                        state.socket = socket;
                        state.dataToSend = new byte[serializedData.Length + 4];
                        byte[] prefix = BitConverter.GetBytes(serializedData.Length);

                        Buffer.BlockCopy(prefix, 0, state.dataToSend, 0, prefix.Length);
                        Buffer.BlockCopy(serializedData, 0, state.dataToSend, prefix.Length, serializedData.Length);
                        socket.BeginSend(state.dataToSend, 0, state.dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), state);
                    }
                    else
                        Debug.LogError("Not Connected to server");
                }
                catch (Exception e)
                {
                    Debug.Log($"Error Sending TCP Data to server {e}");
                }
            }

            private void SendCallback(IAsyncResult result)
            {
                SendState state = (SendState)result.AsyncState;
                SocketError socketError;
                int sentData = state.socket.EndSend(result, out socketError);

                if (socketError != SocketError.Success || !socket.IsConnected())
                {
                    Debug.LogError($"SocketError: {socketError}");
                    Disconnect();
                    return;
                }

                state.dataSent += sentData;
                //Debug.Log($"DataSent:{state.dataSent}, DataToSend:{state.dataToSend.Length}");
                if (state.dataSent != state.dataToSend.Length)
                    socket.BeginSend(state.dataToSend, state.dataSent, state.dataToSend.Length - state.dataSent, SocketFlags.None, new AsyncCallback(SendCallback), state);
            }

            public void Disconnect()
            {
                if (isConnected)
                {
                    isConnected = false;
                    Debug.Log($"Client Disconnected {socket.RemoteEndPoint}");
                    OnClientDisconnected?.Invoke(client);
                    client.tcp.socket.Disconnect(false);
                    client.tcp.socket.Close();
                }
            }
        }
    }
}