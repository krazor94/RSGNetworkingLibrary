//Author: Jake Aquilina
//Creation Date: 15/06/20 07:42 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RealSoftGames.Network
{
    public static class MethodInfoExtensions
    {
        public static Delegate CreateDelegate(this MethodInfo method)
        {
            var types = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var parameters = types.Select(Expression.Parameter).ToArray();
            var body = Expression.Call(method, parameters);
            var lambda = Expression.Lambda(body, parameters);
            return lambda.Compile();
        }

        public static Delegate CreateDeleage(this MethodInfo method, object target)
        {
            Func<Type[], Type> getType;
            var isAction = method.ReturnType.Equals((typeof(void)));
            var types = method.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { method.ReturnType });
            }

            if (method.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), method);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target, method.Name);
        }

        public static MethodInfo[] GetMethodInfo(this Type type)
        {
            List<MethodInfo> methodInfos = new List<MethodInfo>();

            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                try
                {
                    var att = method.GetCustomAttribute<RealSoftGames.Network.RPC>(true);

                    if (att != null)
                    {
                        //Debug.Log($"Adding Hash - {method.Name}");
                        methodInfos.Add(method);
                        //HashTable.HashSet.Add(method.Name, method.CreateDeleage(type.IsStatic() ? null : Activator.CreateInstance(type)));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            return methodInfos.ToArray();
        }
    }
}