///---------------------------------
///   Author : Jake Aquilina
///   Date   : 08/12/19
///   Time   : 05:39 PM
///---------------------------------

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace RealSoftGames
{
    public static class SerializedPropertyExtentions
    {
        public static bool Contains<T>(this SerializedProperty property, T item)
        {
            if (property.type == "int")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).intValue.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "float")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).floatValue.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "double")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).doubleValue.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "string")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).stringValue.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "Object")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).objectReferenceValue.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "bool")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).boolValue.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "enum")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).enumValueIndex.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "Vector2")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).vector2Value.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else if (property.type == "Vector3")
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).vector3Value.Equals(item))
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogError($"Type is not supported");
                for (int i = 0; i < property.arraySize; i++)
                {
                    if (property.GetArrayElementAtIndex(i).objectReferenceValue.Equals(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

#endif