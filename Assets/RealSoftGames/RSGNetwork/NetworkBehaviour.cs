//Author: Jake Aquilina
//Creation Date: 15/06/20 12:25 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using UnityEngine;
using NetworkView = RealSoftGames.Network.NetworkView;

namespace RealSoftGames.Network
{
    /// <summary>
    /// Inherit from Network behavior on the client connection events for the client/server
    /// </summary>
    public class NetworkBehaviour : MonoBehaviour
    {
        private NetworkView networkVIew;

        public NetworkView NetworkView
        {
            get
            {
                if (networkVIew == null)
                {
                    if (TryGetComponent<NetworkView>(out var netView))
                        networkVIew = netView;
                    else if (transform.root != null)
                    {
                        if (transform.root.TryGetComponent<NetworkView>(out netView))
                            networkVIew = netView;
                    }
                }

                return networkVIew;
            }
        }

        public static bool IsServer { get => RSGNetwork.IsServer; }
        public static bool IsConnectedToServer { get => RSGNetwork.IsConnectedToServer; }

        protected virtual void OnEnable()
        {
            RSGNetwork.OnConnected -= OnConnected;
            RSGNetwork.OnConnected += OnConnected;
            RSGNetwork.OnDisconnected -= OnDisconnected;
            RSGNetwork.OnDisconnected += OnDisconnected;
        }

        protected virtual void OnDestroy()
        {
            RSGNetwork.OnConnected -= OnConnected;
            RSGNetwork.OnDisconnected -= OnDisconnected;
        }

        /// <summary>
        /// On Connected to server
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        /// On Disconnected from server
        /// </summary>
        protected virtual void OnDisconnected()
        {
        }
    }
}