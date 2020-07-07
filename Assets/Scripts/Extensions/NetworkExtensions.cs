//Author: Jake Aquilina
//Creation Date: 05/07/20 03:06 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Rendering;

namespace RealSoftGames.Network
{
    public static class NetworkExtensions
    {
        ///// <summary>
        ///// Send Packet Directly to a socket connection
        ///// </summary>
        ///// <param name="socket"></param>
        ///// <param name="packet"></param>
        //public static void RPC(this TcpListener socket, string methodName, params object[] parameters)
        //{
        //    socket.Server.Send(new Packet(0, methodName, parameters).Serialize());
        //    //socket.Send(packet.Serialize(), SocketFlags.None);
        //}

        ///// <summary>
        ///// Send RPC directly to a socket connection
        ///// </summary>
        ///// <param name="socket"></param>
        ///// <param name="methodName"></param>
        ///// <param name="parameters"></param>
        //public static void RPC(this TcpClient socket, string methodName, params object[] parameters)
        //{
        //    socket.Client.Send(new Packet(methodName, parameters).Serialize());
        //    //socket.Send(new Packet(methodName, parameters).Serialize());
        //}
        //
        ///// <summary>
        ///// Send RPC to a connected Client ID
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="methodName"></param>
        ///// <param name="parameters"></param>
        //public static void RPC(this Client client, string methodName, params object[] parameters)
        //{
        //    //Debug.Log($"Sending RPC to client {client.ID}");
        //    client.tcp.socket.Client.Send(new Packet(methodName, parameters).Serialize());
        //}
    }
}