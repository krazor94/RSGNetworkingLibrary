//Author: Jake Aquilina
//Creation Date: 07/07/20 10:21 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace RealSoftGames
{
    public static class TypeExtensions
    {
        public static bool IsStatic(this Type type)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null && type.IsAbstract && type.IsSealed)
                return true;
            return false;
        }

        public static Type[] GetAssemblies()
        {
            var result = new List<System.Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("Assembly-"))
                    continue;

                Type[] types = assembly.GetTypes();
                foreach (var T in types)
                {
                    result.Add(T);
                }
            }
            return result.ToArray();
        }
    }
}