//Author: Jake Aquilina
//Creation Date: 28/06/20 07:05 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using RealSoftGames.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealSoftGames
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        public static List<Packet> packets = new List<Packet>();
        private readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

        private void Update()
        {
            if (packets.Count > 0)
            {
                lock (packets)
                {
                    foreach (var packet in packets)
                        Invoker(packet);
                }

                packets.Clear();
            }

            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue().Invoke();
            }
        }

        private void Invoker(Packet packet)
        {
            try
            {
                packet.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static void AddMessage(Packet packet)
        {
            if (packet != null)
            {
                lock (packets)
                    packets.Add(packet);
            }
            else
                Debug.LogError("Cant add a null message!");
        }

        public static void ExecuteOnmainThread(Action action)
        {
            ExecuteOnMainThread.Enqueue(() => action?.Invoke());
        }
    }
}