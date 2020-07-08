///-----------------------------------------------------------------
///   Author : Jake Aquilina
///   Date   : 30/11/2019 16:19
///-----------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace RealSoftGames
{
    public enum DisablingType
    {
        ReadOnly = 1,
        Disabled = 2
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string comparedPropertyName { get; private set; }

        public object comparedValue { get; private set; }
        public DisablingType disablingType { get; private set; }

        public ShowIfAttribute(string comparedPropertyName, object comparedValue, DisablingType disablingType = DisablingType.Disabled)
        {
            this.comparedPropertyName = comparedPropertyName;
            this.comparedValue = comparedValue;
            this.disablingType = disablingType;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        private ShowIfAttribute showIf;
        private SerializedProperty comparedField;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute show = (ShowIfAttribute)attribute;

            if (Show(property))
                return EditorGUI.GetPropertyHeight(property, label, true);
            else
                return 0;
            //-EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Show(property))
            {
                EditorGUI.PropertyField(position, property, true);
            }
            else
            {
                if (showIf.disablingType == DisablingType.ReadOnly)
                {
                    GUI.enabled = false;
                    EditorGUI.PropertyField(position, property, true);
                    GUI.enabled = true;
                }
                //else if (showIf.disablingType == DisablingType.Disabled)
                //{
                //}
            }
        }

        private bool Show(SerializedProperty property)
        {
            showIf = attribute as ShowIfAttribute;
            comparedField = property.serializedObject.FindProperty(showIf.comparedPropertyName);

            string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, showIf.comparedPropertyName) : showIf.comparedPropertyName;
            object comparedFieldValue = property.serializedObject.FindProperty(path);
            // object comparedFieldValue = (object)comparedField.objectReferenceValue;

            switch (comparedField.type)
            { // Possible extend cases to support your own type
                case "bool":

                    if (comparedField.boolValue.Equals(showIf.comparedValue))
                        return true;
                    else
                        return false;

                case "Enum":

                    if (comparedField.enumValueIndex.Equals((int)showIf.comparedValue))
                        return true;
                    else
                        return false;

                case "int":
                    if (comparedField.intValue.Equals(showIf.comparedValue))
                        return true;
                    else
                        return false;

                case "float":
                    if (comparedField.floatValue.Equals(showIf.comparedValue))
                        return true;
                    else
                        return false;

                case "string":
                    if (comparedField.stringValue.Equals(showIf.comparedValue))
                        return true;
                    else
                        return false;

                default:
                    Debug.LogError($"Error: {comparedField.type} Is not supported");
                    return true;
            }
        }
    }

#endif
}