//Author: Jake Aquilina
//Creation Date: 15/06/20 12:30 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using UnityEngine;
using RealSoftGames.Network;

namespace RealSoftGames
{
    public static class ByteConverter
    {
        public static Type GetType<T>(this object m_Type)
        {
            switch (m_Type)
            {
                case Type type when type == typeof(string):
                    return typeof(string);

                case Type type when type == typeof(int):
                    return typeof(int);

                case Type type when type == typeof(float):
                    return typeof(float);

                case Type type when type == typeof(double):
                    return typeof(double);

                case Type type when type == typeof(bool):
                    return typeof(bool);

                case Type type when type == typeof(Enum):
                    return typeof(Enum);

                case Type type when type == typeof(char):
                    return typeof(char);

                case Type type when type == typeof(byte):
                    return typeof(byte);

                case Type type when type == typeof(sbyte):
                    return typeof(sbyte);

                case Type type when type == typeof(uint):
                    return typeof(uint);

                case Type type when type == typeof(short):
                    return typeof(short);

                case Type type when type == typeof(ulong):
                    return typeof(ulong);

                case Type type when type == typeof(long):
                    return typeof(long);

                case Type type when type == typeof(Vector3):
                    return typeof(Vector3);

                case Type type when type == typeof(Vector2):
                    return typeof(Vector2);

                case Type type when type == typeof(Quaternion):
                    return typeof(Quaternion);

                default:
                    throw new ArgumentNullException(nameof(m_Type));
            }
        }
    }
}