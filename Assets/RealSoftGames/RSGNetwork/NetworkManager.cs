//Author: Jake Aquilina
//Creation Date: 07/07/20 10:15 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using UnityEngine;

namespace RealSoftGames.Network
{
    public class NetworkManager : NetworkBehaviour
    {
        private Coroutine heartBeatRoutine;

        public enum NetworkType
        {
            CLIENT,
            SERVER
        }

        [SerializeField] private bool ConnectOnStart;
        [SerializeField] private NetworkType networkType = NetworkType.SERVER;
        [SerializeField] private string ipAddress = "127.0.0.1";
        [SerializeField] private int serverPort = 8095;

        private void Awake()
        {
            if (!IsConnectedToServer)
            {
                void MyRPC()
                {
                    Debug.Log("~~~Connected To Server");
                    RSGNetwork.ServerRPC("TestABC", "TestABC");
                    RSGNetwork.ServerRPCCallback("TestCallbackRPC", "CallbackRPC", "TestCallbackRPC");
                    RSGNetwork.OnConnected -= MyRPC;
                }
                RSGNetwork.OnConnected += MyRPC;
                //RSGNetwork.ConnectToServer();
            }
        }

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
                    StartCoroutine(HeartBeat());
                    break;

                case NetworkType.CLIENT:
                    RSGNetwork.ConnectToServer();
                    break;
            }
        }

        protected override void OnConnected()
        {
            MainThreadDispatcher.ExecuteOnmainThread(() => InitCoroutine(true));
            Debug.Log("OnConnected to server, sending ServerRPC TestABC");

            RSGNetwork.ServerRPC("TestABC", "~~~~~~OnConnected~~~~~~ TestABC");
            RSGNetwork.ServerRPCCallback("TestCallbackRPC", "CallbackRPC", "~~~~~~OnConnected~~~~~~ TestCallbackRPC");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //if (!IsConnectedToServer)
                //{
                //    void MyRPC()
                //    {
                //        Debug.Log("~~~Connected To Server");
                //        RSGNetwork.ServerRPC("TestRPCMessage", "HELLO WORLD!");
                //        RSGNetwork.ServerRPCCallback("TestRPCMessage", "TestCallback", "Hello second world");
                //        //RSGNetwork.ServerRPC("TestRPCMessage", "ABCDEFGHIJKLM12345");
                //        RSGNetwork.OnConnected -= MyRPC;
                //    }
                //    RSGNetwork.OnConnected += MyRPC;
                //    RSGNetwork.ConnectToServer();
                //}
                //else
                //{
                Debug.Log("Sending ServerRPC {TestABC, Hello World}");
                RSGNetwork.ServerRPC("TestABC", "Hello World");
                //RSGNetwork.ServerRPCCallback("TestCallbackRPC", "CallbackRPC", "!!!Hello second world");
                //}
            }
        }

        [RPC]
        public static void TestABC(string a)
        {
            Debug.Log(a);
        }

        [RPC]
        public static void TestCallbackRPC(string a)
        {
            Debug.Log(a);
        }

        [RPC]
        public static void CallbackRPC()
        {
            Debug.Log("CallbackRPC");
        }

        protected override void OnDisconnected()
        {
            InitCoroutine(false);
        }

        private void InitCoroutine(bool value)
        {
            if (value)
                heartBeatRoutine = StartCoroutine(HeartBeat());
            else
                StopCoroutine(heartBeatRoutine);
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

        [RPC]
        private static void Ping()
        {
            //if (IsServer)
            //    Debug.Log("Received ping from client");
            //else
            //    Debug.Log("Received Ping from server");
        }

        private IEnumerator HeartBeat()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                if (IsServer)
                {
                    foreach (var client in RSGNetwork.Clients)
                        if (!client.Value.tcp.socket.IsConnected())
                        {
                            //Debug.Log("Client Disconnected (HB)");
                            client.Value.Disconnect();
                        }
                        else
                            client.Value.tcp.RPC("Ping");
                }
                else
                {
                    if (IsConnectedToServer)
                    {
                        if (!RSGNetwork.IsConencted)
                        {
                            //Debug.Log("Client Disconnected (HB)");
                            RSGNetwork.DisconnectFromServer();
                        }

                        //Debug.Log("Sending ping to server");
                        RSGNetwork.ServerRPC("Ping");
                    }
                }
            }
        }
    }
}