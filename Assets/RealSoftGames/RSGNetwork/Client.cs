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
        public Client(Socket client, string guid)
        {
            tcp = new TCP(client, this);
            GUID = guid;
        }

        public static event Action<Client> OnClientConnected;

        public static event Action<Client> OnClientDisconnected;

        public static int dataBufferSize = 4096;
        public TCP tcp;
        public readonly string GUID;

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

            public static int dataBufferSize = 4096;
            public Socket socket;
            private string guid;
            private Packet receivedData;
            private byte[] receiveBuffer;
            private bool isConnected = false;
            public readonly Client client;

            public void Connect(Socket tcpSocket)
            {
                ReceiveState state = new ReceiveState();
                socket = tcpSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;
                //stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];
                socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, state);
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

                    if (socketError != SocketError.Success)
                    {
                        Debug.LogError($"Socket Error:{socketError}");
                        client.Disconnect();
                        return;
                    }

                    if (byteLength <= 0)
                    {
                        client.Disconnect();
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

                        using (BinaryReader reader = new BinaryReader(state.Data))
                        {
                            byte[] data = reader.ReadBytes(state.DataSize);
                            MainThreadDispatcher.AddMessage(data.Deserialize<Packet>());
                            socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, new ReceiveState());
                        }

                        //byte[] data = new byte[byteLength];
                        //Array.Copy(receiveBuffer, data, byteLength);
                        //Debug.Log($"Recieved Some Data {data.Length}");
                        //MainThreadDispatcher.AddMessage(data.Deserialize<Packet>());
                        //stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                        //socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
                    }
                    else
                    {
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
                    if (socket != null)
                    {
                        //Send back to client and send servers GUID as well
                        byte[] serializedData = new Packet(methodName, callback, parameters).Serialize();
                        SendState state = new SendState();

                        state.dataToSend = new byte[serializedData.Length + 4];
                        byte[] prefix = BitConverter.GetBytes(serializedData.Length);

                        Buffer.BlockCopy(prefix, 0, state.dataToSend, 0, prefix.Length);
                        Buffer.BlockCopy(serializedData, 0, state.dataToSend, prefix.Length, serializedData.Length);
                        socket.BeginSend(state.dataToSend, 0, state.dataToSend.Length, SocketFlags.None, new AsyncCallback(SendCallback), state);

                        //stream.BeginWrite(serializedData, 0, serializedData.Length, null, null);
                        //socket.BeginSend(serializedData, 0, serializedData.Length, SocketFlags.None, null, null);
                    }
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
                int sentData = socket.EndSend(result, out socketError);

                if (socketError != SocketError.Success)
                {
                    Debug.LogError($"SocketError: {socketError}");
                    socket.Close();
                    return;
                }
                state.dataSent += sentData;
                if (state.dataSent != state.dataToSend.Length)
                    socket.BeginSend(state.dataToSend, state.dataSent, state.dataToSend.Length - state.dataSent, SocketFlags.None, new AsyncCallback(SendCallback), state);
            }

            public void Disconnect()
            {
                //if (isConnected)
                //{
                Debug.Log($"Client Disconnected {socket.RemoteEndPoint}");
                OnClientDisconnected?.Invoke(client);
                isConnected = false;
                socket.Disconnect(false);
                //}
            }
        }
    }
}