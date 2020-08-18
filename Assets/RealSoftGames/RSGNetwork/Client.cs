//Author: Jake Aquilina
//Creation Date: 07/07/20 10:12 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Net.Sockets;
using UnityEditor;

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
                socket = tcpSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;
                //stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];
                socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
                isConnected = true;

                OnClientConnected?.Invoke(client);
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    //int byteLength = stream.EndRead(result);
                    int byteLength = socket.EndReceive(result);
                    if (byteLength <= 0)
                    {
                        client.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    //Debug.Log($"Recieved Some Data {data.Length}");
                    MainThreadDispatcher.AddMessage(data.Deserialize<Packet>());
                    //stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                    socket.BeginReceive(receiveBuffer, 0, dataBufferSize, SocketFlags.None, ReceiveCallback, null);
                }
                catch (Exception e)
                {
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
                        //stream.BeginWrite(serializedData, 0, serializedData.Length, null, null);
                        socket.BeginSend(serializedData, 0, serializedData.Length, SocketFlags.None, null, null);
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
                Debug.Log($"Client Disconnected {socket.RemoteEndPoint}");
                OnClientDisconnected?.Invoke(client);
                isConnected = false;
                socket.Disconnect(false);
                //}
            }
        }
    }
}