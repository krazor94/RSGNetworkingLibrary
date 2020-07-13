//Author: Jake Aquilina
//Creation Date: 07/07/20 10:15 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RealSoftGames.Network
{
    public class NetworkManager : NetworkBehaviour
    {
        public enum NetworkType
        {
            CLIENT,
            SERVER
        }

        [SerializeField] private bool ConnectOnStart;
        [SerializeField] private NetworkType networkType = NetworkType.SERVER;
        [SerializeField] private string ipAddress = "127.0.0.1";
        [SerializeField] private int serverPort = 8080;

        private void Start()
        {
            RSGNetwork.IPAddress = ipAddress;
            RSGNetwork.PortNumber = serverPort;

            if (!ConnectOnStart)
                return;

            switch (networkType)
            {
                case NetworkType.SERVER:
                    RSGNetwork.StartServer();

                    break;

                case NetworkType.CLIENT:

                    void OnConnected()
                    {
                        if (!IsConnectedToServer)
                        {
                            Debug.LogError("Not Connected to server");
                            return;
                        }

                        Debug.Log("OnConnected");
                        RSGNetwork.ServerRPC("TestRPC", "ResultRPC", 1f, 1f);
                        RSGNetwork.OnConnected -= OnConnected;
                    }

                    void OnDisconnected()
                    {
                        Debug.Log("OnDisconnected");
                        RSGNetwork.OnDisconnected -= OnDisconnected;
                    }

                    RSGNetwork.OnConnected += OnConnected;
                    RSGNetwork.OnDisconnected += OnDisconnected;

                    RSGNetwork.ConnectToServer();
                    break;
            }
        }

        [RPC]
        private static float TestRPC(float a, float b)
        {
            Debug.Log($"{a + b} = {a} + {b}");
            return a + b;
        }

        [RPC]
        private static void ResultRPC(float result)
        {
            Debug.Log($"Result = {result}");
        }

        private void OnApplicationQuit()
        {
            switch (networkType)
            {
                case NetworkType.SERVER:
                    RSGNetwork.StopServer();
                    break;

                case NetworkType.CLIENT:
                    RSGNetwork.DisconnectFromServer();
                    break;
            }
        }
    }
}