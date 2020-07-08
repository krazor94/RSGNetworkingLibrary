//Author: Jake Aquilina
//Creation Date: 15/06/20 12:52 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealSoftGames
{
    public class Debug : UnityEngine.Debug
    {
        public static new void Log(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message.ToString());
#endif

#if UNITY_STANDALONE
            Console.WriteLine(message.ToString());
#endif
        }

        public static new void Log(object message, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message.ToString(), context);
#endif

#if UNITY_STANDALONE
            Console.WriteLine(message.ToString());
#endif
        }

        public static new void LogError(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message.ToString());
#endif

#if UNITY_STANDALONE
            Console.WriteLine(message.ToString());
#endif
        }

        public static new void LogError(object message, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(message.ToString(), context);
#endif

#if UNITY_STANDALONE
            Console.WriteLine(message.ToString());
#endif
        }

        public static new void LogWarning(object message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(message.ToString());
#endif

#if UNITY_STANDALONE
            Console.WriteLine(message.ToString());
#endif
        }

        public static new void LogWarning(object message, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(message.ToString());
#endif

#if UNITY_STANDALONE
            Console.WriteLine(message.ToString());
#endif
        }
    }
}