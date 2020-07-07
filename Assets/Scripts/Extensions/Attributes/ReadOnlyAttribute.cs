///-----------------------------------------------------------------
///   Author : It3ration
///   Source: https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
///   Date   : 10/10/2019 18:29
///-----------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

namespace RealSoftGames
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }

    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}

#endif