/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

//#define VERBOSE

using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cheers
{
    public static class EditorHelper
    {
        #region Initialize

#if UNITY_EDITOR

        static string _scriptFolder;

        [InitializeOnLoadMethod]
        static void InitializeAssets()
        {
#pragma warning disable 0219

            var assetPathRet = GetEditorAssetPath();

#pragma warning restore 0219
#if VERBOSE
        StringBuilder log = new StringBuilder("<color=maroon>[ Initializing Editor Assets... ]</color>\n");
        log.AppendLine(assetPathRet);
        PrettyLog.Log(log.ToString());
#endif //VERBOSE
        }

        static string GetEditorAssetPath()
        {
            var scriptPath = AssetDatabase.FindAssets("l:CheersInspector t:Script")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .FirstOrDefault(path =>
            {
                return System.IO.Path.GetFileName(path) == "EditorHelper.cs";
            });
            _scriptFolder = GetFolderOfAsset(scriptPath);
            return string.Format(">>> Editor assets @ {0}", _scriptFolder);
        }

#else // UNITY_EDTIOR
        static void InitializeAssets(){
            PrettyLog.Warn("EditorHelper is not for runtime environment");
        }
#endif // UNITY_EDITOR
        #endregion

        #region Utils

        public static string GetFolderOfAsset(string assetPath)
        {
            var lastSeparatorIndex = assetPath.LastIndexOf('/');
            return assetPath.Substring(0, lastSeparatorIndex + 1);
        }

#if UNITY_EDITOR
        public static string GetControlNameByProperty(SerializedProperty prop)
        {
            return prop.serializedObject.targetObject.GetInstanceID() + prop.propertyPath;
        }
#endif
        #endregion

#if UNITY_EDITOR
        #region Access Property

        const string ArrayDataPrefix = "Array.data[";

        public static Type GetPropertyFieldType(SerializedProperty prop)
        {
            var field = GetObjectFieldInfo(prop.serializedObject.targetObject.GetType(), prop.propertyPath);
            if (field == null)
                return null;
            return field.FieldType;
        }

        public static object GetSerializedPropertyValue(SerializedProperty prop)
        {
            return GetObjectFieldValue(prop.serializedObject.targetObject, prop.propertyPath);
        }

        static FieldInfo GetFieldInfoByType(Type type, string fieldName)
        {
            var flag = BindingFlags.NonPublic
                       | BindingFlags.Public
                       | BindingFlags.Instance
                       | BindingFlags.FlattenHierarchy;
            var field = type.GetField(fieldName, flag);
            if (field == null)
            {
                if (type.BaseType != null)
                {
                    return GetFieldInfoByType(type.BaseType, fieldName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return field;
            }
        }

        /// <summary>
        /// 解析数组路径，格式为Array.data[n]...
        /// </summary>
        /// <param name="arrayPath">数组路径</param>
        /// <param name="nextPath">下一级路径，若没有下级路径则返回空字符串（""），若解析错误则返回null</param>
        /// <returns>数组下标，若解析错误则返回-1</returns>
        static int ParseArrayPath(string arrayPath, out string nextPath)
        {
            var rightBracketIndex = arrayPath.IndexOf("]");
            if (rightBracketIndex < 0)
            {
                nextPath = null;
                return -1;
            }
            nextPath =  arrayPath.Substring(rightBracketIndex + 1).TrimStart('.');
            var indexStr = arrayPath.Substring(ArrayDataPrefix.Length, rightBracketIndex - ArrayDataPrefix.Length);
            int index;
            if (!int.TryParse(indexStr, out index))
            {
                index = -1;
            }
            return index;
        }

        /// <summary>
        /// 获取字段信息，支持使用“.”分隔的多级路径
        /// </summary>
        /// <returns>The object field info.</returns>
        /// <param name="objType">Object type.</param>
        /// <param name="fieldPath">Field path.</param>
        public static FieldInfo GetObjectFieldInfo(System.Type objType, string fieldPath)
        {
            var hierarchy = fieldPath.Split(new[] { '.' }, 2);
            var field = GetFieldInfoByType(objType, hierarchy[0]);
            if (field == null)
            {
                return null;
            }
            if (hierarchy.Length > 1)
            {
                var nextPath = hierarchy[1];
                if (!nextPath.StartsWith(ArrayDataPrefix))
                {
                    return GetObjectFieldInfo(field.FieldType, nextPath);
                }
                else if (field.FieldType.IsArray || typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    ParseArrayPath(nextPath, out nextPath);
                    if (string.IsNullOrEmpty(nextPath))
                    {
                        return field;
                    }
                    else
                    {
                        var fieldType = field.FieldType;
                        Type elementType = null;
                        if (fieldType.IsArray)
                        {
                            elementType = fieldType.GetElementType();
                        }
                        else if (fieldType.IsGenericType)
                        {
                            var gTypes = fieldType.GetGenericArguments();
                            if(gTypes.Length != 1)
                            {
                                PrettyLog.Error("Unknown array or list type: {0}", fieldType);
                                return null;
                            }
                            elementType = gTypes[0];
                        }
                        else
                        {
                            PrettyLog.Error("Only array and List<T> can retrieve FieldInfo by propertyPath (not {0})", fieldType);
                            return null;
                        }
                        return GetObjectFieldInfo(elementType, nextPath);
                    }
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return field;
            }
        }

        /// <summary>
        /// 获取字段值，支持使用“.”分隔的多级路径
        /// </summary>
        /// <returns>The object field value.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="fieldPath">Field path.</param>
        public static object GetObjectFieldValue(object obj, string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
                return obj;
            var hierarchy = fieldPath.Split(new[] { '.' }, 2);
            var field = GetFieldInfoByType(obj.GetType(), hierarchy[0]);
            if (field == null)
            {
                return null;
            }
            var val = field.GetValue(obj);
            if (hierarchy.Length > 1)
            {
                var nextPath = hierarchy[1];
                if (!nextPath.StartsWith(ArrayDataPrefix))
                {
                    return GetObjectFieldValue(val, nextPath);
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    var index = ParseArrayPath(nextPath, out nextPath);
                    if (index < 0)
                    {
                        return null;
                    }
                    val = ((IList)val)[index];
                    return GetObjectFieldValue(val, nextPath);
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return val;
            }
        }
        
        #endregion

#endif

        #region Styling

        public struct Icons
        {
            public const string InfoFileName = "info.png";
            public const string InfoHighlightName = "info-highlight.png";
            public const string WarningFileName = "warning.png";
            public const string ErrorFileName = "error.png";

            public Texture2D info { get { return LoadEditorTexture(InfoFileName); } }

            public Texture2D infoHighlight { get { return LoadEditorTexture(InfoHighlightName); } }

            public Texture2D warning { get { return LoadEditorTexture(WarningFileName); } }

            public Texture2D error { get { return LoadEditorTexture(ErrorFileName); } }
        }

        const string ImagesPath = "Images/";
        public static readonly string loadedStr = string.Format("<color={0}>loaded</color>", PassedColorName);
        public static readonly string notFoundStr = string.Format("<color={0}>not found</color>", FailedColorName);

        public static Icons icons;
        static Dictionary<string, Texture2D> _cachedTextures = new Dictionary<string, Texture2D>();

        public static Texture2D LoadEditorTexture(string relativePath, StringBuilder log = null)
        {
#if UNITY_EDITOR
            var path = _scriptFolder + ImagesPath + relativePath;
            Texture2D tex;
            if (_cachedTextures.TryGetValue(path, out tex))
                return tex;

            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _cachedTextures[path] = tex;
#if VERBOSE
        string logText = string.Format(" > {0} -> {1}",
            ImagesPath + relativePath,
            tex != null ? loadedStr : notFoundStr);
        if (log != null)
        {
            log.AppendLine(logText);
        }
        else
        {
            PrettyLog.Log(logText);
        }
#endif
            return tex;
#else
        return null;
#endif
        }

        public static string PassedColorName
        {
            get
            {
                return
#if UNITY_EDITOR
                EditorGUIUtility.isProSkin ? "lime" :
#endif
                "green";
            }
        }

        public static string FailedColorName { get { return "red"; } }

        public static string ReadyColorName
        {
            get
            {
                return
#if UNITY_EDITOR
                EditorGUIUtility.isProSkin ? "yellow" :
#endif
                "orange";
            }
        }

        public static Color PassedColor { get { return ColorHelper.ByWeb(PassedColorName); } }

        public static Color FailedColor { get { return ColorHelper.ByWeb(FailedColorName); } }

        public static Color ReadyColor { get { return ColorHelper.ByWeb(ReadyColorName); } }

        #endregion
    }
}