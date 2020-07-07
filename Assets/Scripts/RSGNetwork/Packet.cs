//Author: Jake Aquilina
//Creation Date: 07/07/20 10:14 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Reflection;

namespace RealSoftGames.Network
{
    [System.Serializable]
    public class Packet : IDisposable
    {
        public Packet()
        {
        }

        public Packet(int sender, string methodName, params object[] parameters)
        {
            this.sender = sender;
            this.methodName = methodName;
            this.parameters = parameters;
        }

        //private byte packetType;

        private int sender;
        //private string callback;

        private string methodName;
        private object[] parameters;
        public string MethodName { get => methodName; }
        public object[] Parameters { get => parameters; }

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
                    value.Invoke(null, parameters);
                else
                {
                    foreach (var view in NetworkView.Views)
                    {
                        foreach (var behaviour in view.Behaviours)
                        {
                            if (behaviour.GetType() == value.DeclaringType)
                                value.Invoke(behaviour, parameters);
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