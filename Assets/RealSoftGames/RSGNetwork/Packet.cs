//Author: Jake Aquilina
//Creation Date: 07/07/20 10:14 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Dynamic;
using System.Reflection;

namespace RealSoftGames.Network
{
    [System.Serializable]
    public class Packet : IDisposable
    {
        public Packet()
        {
        }

        public Packet(string methodName, string callback = null, params object[] parameters)
        {
            this.methodName = methodName;
            this.parameters = parameters;
            this.callback = callback;
        }

        public Packet(string sender, string methodName, string callback = null, params object[] parameters)
        {
            this.methodName = methodName;
            this.parameters = parameters;
            this.callback = callback;
            this.sender = sender;
        }

        //private byte packetType;

        private string sender;
        private string callback;
        private string methodName;
        private object[] parameters;
        public string MethodName { get => methodName; }
        public object[] Parameters { get => parameters; }
        public string Callback { get => callback; }

        public void Invoke()
        {
            //switch (packetType)
            //{
            //    case 0:
            //        //What to do with a stream of bytes
            //        break;
            //
            //    case 1:
            if (HashTable.HashSet.TryGetValue(methodName, out MethodInfo value))
            {
                if (value.IsStatic)
                {
                    if (string.IsNullOrEmpty(callback)) //value.ReturnType == typeof(void) ||
                        value.Invoke(null, parameters);
                    else
                    {
                        var result = value.Invoke(null, parameters);
                        if (RSGNetwork.IsServer)
                        {
                            if (result != null)
                                RSGNetwork.RPC(sender, callback, null, result);
                            else
                                RSGNetwork.RPC(sender, callback);
                        }
                        else
                        {
                            RSGNetwork.ServerRPC(callback, null, result == null ? null : result);
                        }
                    }
                }
                else
                {
                    foreach (var view in NetworkView.Views)
                    {
                        foreach (var behaviour in view.Behaviours)
                        {
                            if (behaviour.GetType() == value.DeclaringType)
                            {
                                if (string.IsNullOrEmpty(callback)) //value.ReturnType == typeof(void) ||
                                    value.Invoke(behaviour, parameters);
                                else
                                {
                                    var result = value.Invoke(behaviour, parameters);
                                    if (RSGNetwork.IsServer)
                                    {
                                        if (result != null)
                                            RSGNetwork.RPC(sender, callback, null, result);
                                        else
                                            RSGNetwork.RPC(sender, callback, null);
                                    }
                                    else
                                    {
                                        if (result != null)
                                            RSGNetwork.ServerRPC(callback, null, result);
                                        else
                                            RSGNetwork.ServerRPC(callback);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //break;
            //}
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}