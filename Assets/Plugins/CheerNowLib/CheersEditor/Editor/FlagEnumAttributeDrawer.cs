/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Cheers
{
    [CustomPropertyDrawer(typeof(FlagEnum))]
    public class FlagEnumAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            var t = this.fieldInfo.FieldType;
            if (t.IsArray)
                t = t.GetElementType();

            property.intValue = EnumFlagField(position, t, label, property.intValue);
        }

        public static int EnumFlagField (Rect position, System.Type enumType, GUIContent label, int value)
        {
            var names = GetUniqueEnumFlags(enumType).Select(e => e.ToString()).ToArray();
            return EditorGUI.MaskField(position, label, value, names);
        }

        private static IEnumerable<System.Enum> GetUniqueEnumFlags (System.Type enumType)
        {
            if (enumType == null)
                throw new System.ArgumentNullException("enumType");
            if (!enumType.IsEnum)
                throw new System.ArgumentException("Type must be an enum.", "enumType");

            var enumValues = System.Enum.GetValues(enumType);
            foreach (var e in enumValues) {
                var v = System.Convert.ToUInt64(e);
                if (v > 0 && IsPowerOfTwo(v))
                    yield return e as System.Enum;
            }
        }

        private static bool IsPowerOfTwo (ulong x)
        {
            return (x & (x - 1)) == 0;
        }
    }
}