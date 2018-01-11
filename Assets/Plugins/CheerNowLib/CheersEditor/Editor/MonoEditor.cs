/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using Obj = UnityEngine.Object;
using System.Text;

namespace Cheers
{
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class MonoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Type refType;
            Obj[] targets;
            if (!serializedObject.isEditingMultipleObjects)
            {
                refType = serializedObject.targetObject.GetType();
                targets = new[] { serializedObject.targetObject };
            }
            else
            {
                refType = TypeUtility.FindCommonTypeWithin(serializedObject.targetObjects.Select(obj => obj.GetType()).ToArray());
                targets = serializedObject.targetObjects;
            }
            if (refType == null)
                return;

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = refType.GetMembers(flags).Where(m => GetCheersAttribute(m) != null)
            .Union(
                          refType.GetInterfaceHierarchy().SelectMany(i => i.GetMembers(flags))
                      );
            foreach (var member in members.Where(m => GetCheersAttribute(m) != null))
            {
                var attr = GetCheersAttribute(member);
                if (attr == null)
                    continue;
                switch (member.MemberType)
                {
                    case MemberTypes.Method:
                        DrawMethod((MethodInfo)member, attr, targets);
                        break;
                    case MemberTypes.Property:
                        DrawProperty((PropertyInfo)member, attr, targets);
                        break;
                    default:
                        PrettyLog.Log("TODO...Or never to be done. >_< Just use SerializedField instead");
                        break;
                }
            }
        }

        See GetCheersAttribute(MemberInfo member)
        {
            var attrs = member.GetCustomAttributes(typeof(See), true);
            if (attrs == null || attrs.Length == 0)
                return null;
            return (See)attrs[0];
        }

        void DrawField()
        {
            // todo
        }

        void DrawProperty(PropertyInfo prop, See attr, Obj[] targets)
        {
            Undo.RecordObjects(targets, "PropertyChange-" + prop.Name);

            var propType = prop.PropertyType;
            var getter = prop.GetGetMethod(true);
            var setter = prop.GetSetMethod(true);

            var label = GetCheersTitle(attr, prop);

            bool hasMultiValues;
            bool isReadOnly = setter == null;
            // 获取所有选中项的属性值的最小集合
            var values = targets.Select(t => GetPropValue(propType, getter, t)).Distinct();
            // 用于绘制编辑器的回调
            var editorFunc = GetValuesAndEditor(propType, getter, values, label, out hasMultiValues);

            if (editorFunc == null)
                return;

            // 选中项的属性值是否各不相同，如果有则在编辑器中画 "-"
            EditorGUI.showMixedValue = hasMultiValues;
            // 如果为只读属性则置灰
            if (isReadOnly)
            {
                EditorGUI.BeginDisabledGroup(true);
            }
            EditorGUI.BeginChangeCheck();
            var commonValue = editorFunc();
            if (EditorGUI.EndChangeCheck())
            { // 有改动才为属性赋值
                targets.ForEach(t => setter.Invoke(t, new[] { commonValue }));
            }
            if (isReadOnly)
            {
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.showMixedValue = false;
        }

        object GetPropValue(Type propType, MethodInfo getter, Obj t)
        {
            if (getter == null)
                return propType.DefaultValue();
            return getter.Invoke(t, null);
        }

        Func<object> GetValuesAndEditor(Type propType, MethodInfo getter, IEnumerable<object> values, string label, out bool hasMultipleValues)
        {
            Func<object> editorCb = null;
            if (values == null)
            {
                hasMultipleValues = false;
                return null;
            }
            var firstVal = values.First();
            if (propType == typeof(int))
            {
                editorCb = () => EditorGUILayout.IntField(label, (int)firstVal);
            }
            else if (propType == typeof(float))
            {
                editorCb = () => EditorGUILayout.FloatField(label, (float)firstVal);
            }
            else if (propType == typeof(float))
            {
                editorCb = () => EditorGUILayout.LongField(label, (long)firstVal);
            }
            else if (propType == typeof(string))
            {
                editorCb = () => EditorGUILayout.TextField(label, firstVal.ToString());
            }
            else if (propType == typeof(Vector2))
            {
                editorCb = () => EditorGUILayout.Vector2Field(label, (Vector2)firstVal);
            }
            else if (propType == typeof(Vector3))
            {
                editorCb = () => EditorGUILayout.Vector3Field(label, (Vector3)firstVal);
            }
            else if (propType == typeof(Vector4))
            {
                editorCb = () => EditorGUILayout.Vector4Field(label, (Vector4)firstVal);
            }
            else if (propType == typeof(Rect))
            {
                editorCb = () => EditorGUILayout.RectField(label, (Rect)firstVal);
            }
            else
            {
                var header = new GUIContent(label);
                var text = string.Format("{0} not supported (Property)", propType);
                var msg = new GUIContent(text, EditorHelper.icons.warning);
                EditorGUILayout.LabelField(header, msg);
                hasMultipleValues = false;

                return null;
            }
            hasMultipleValues = values.Count() > 1;

            return editorCb;
        }

        void DrawMethod(MethodInfo method, See attr, Obj[] targets)
        {
            var @params = method.GetParameters();
            var btnText = new GUIContent(GetCheersTitle(attr, method));
            var canPress = true;
            if (@params.Length > 0)
            {
                btnText.text += "\n(Parameters are not supported yet)";
                btnText.image = EditorHelper.icons.warning;
                canPress = false;
            }
            EditorGUI.BeginDisabledGroup(!canPress);

            var bgColor = attr.BackgroundColor == Color.clear ? GUI.backgroundColor : attr.BackgroundColor;
            var oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;
            var btnStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).button);
            if (attr.TextColor != Color.clear)
            {
                btnStyle.normal.textColor = attr.TextColor;
                btnStyle.hover.textColor = attr.TextColor;
                btnStyle.focused.textColor = attr.TextColor;
                btnStyle.active.textColor = attr.TextColor;
            }
            if (GUILayout.Button(btnText, btnStyle))
            {
                StringBuilder retSb = new StringBuilder("=== Call Returns ===\n");
                foreach (var t in targets)
                {
                    var ret = method.Invoke(t, null);
                    retSb.AppendLine(string.Format(" - {0} >> {1}", t.name, ret ?? "(null)"));
                }
                //PrettyLog.Log(retSb.ToString());
            }
            GUI.backgroundColor = oldBgColor;

            EditorGUI.EndDisabledGroup();
        }

        string GetCheersTitle(See attr, MemberInfo member)
        {
            if (attr.Title == null || string.IsNullOrEmpty(attr.Title.Trim()))
            {
                return ObjectNames.NicifyVariableName(member.Name);
            }
            return attr.Title;
        }
    }
}
