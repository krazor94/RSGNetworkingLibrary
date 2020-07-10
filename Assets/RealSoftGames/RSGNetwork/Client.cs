//Author: Jake Aquilina
//Creation Date: 07/07/20 10:12 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace RealSoftGames.Network
{
    [System.Serializable]
    public class Client
    {
        public static event Action<Client> OnClientConnected;

        public static event Action<Client> OnClientDisconnected;

        public static int dataBufferSize = 4096;
        public TCP tcp;

        public Client(TcpClient client)
        {
            tcp = new TCP(client, this);
        }

        public void Disconnect()
        {
            tcp.Disconnect();
        }

        public class TCP
        {
            private readonly int id;
            public static int dataBufferSize = 4096;
            public TcpClient socket;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            private bool isConnected = false;
            public readonly Client client;

            public TCP(TcpClient socket, Client client)
            {
                this.socket = socket;
                this.client = client;
            }

            public void Connect(TcpClient tcpSocket)
            {
                socket = tcpSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;
                stream = socket.GetStream();
                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                isConnected = true;

                OnClientConnected?.Invoke(client);
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        client.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    Debug.Log($"Recieved Some Data {data.Length}");
                    MainThreadDispatcher.AddMessage(data.Deserialize<Packet>());
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Disconnect();
                }
            }

            public void RPC(string methodName, params object[] parameters)
            {
                try
                {
                    if (socket != null)
                    {
                        byte[] serializedData = new Packet(methodName, parameters).Serialize();
                        stream.BeginWrite(serializedData, 0, serializedData.Length, null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error Sending TCP Data to server {e}");
                }
            }

            public void Disconnect()
            {
                //if (isConnected)
                //{
                Debug.Log($"Client Disconnected {socket.Client.RemoteEndPoint}");
                OnClientDisconnected?.Invoke(client);
                isConnected = false;
                socket.Close();
                //}
            }
        }
    }
}