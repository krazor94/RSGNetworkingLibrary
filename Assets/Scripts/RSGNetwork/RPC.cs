//Author: Jake Aquilina
//Creation Date: 15/06/20 12:33 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;

namespace RealSoftGames.Network
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RPC : Attribute
    {
        private static byte rpcIdCounter = 0;
        public byte RpcID { get; private set; }

        public RPC()
        {
            RpcID = rpcIdCounter++;
        }
    }
}