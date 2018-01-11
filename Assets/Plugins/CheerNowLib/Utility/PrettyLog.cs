/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.Linq;

public class PrettyLog
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class Provider : Attribute
    {
        /// <summary>
        /// 模块名称，如“Room”、“Insurance”等
        /// </summary>
        public string Module;
        /// <summary>
        /// 角色名称，用于表示在业务逻辑中的角色，如“controller”、“view”等
        /// </summary>
        public string Role;

        /// <summary>
        /// 模块颜色值（Unity内置<color>标签颜色名，或十六进制）
        /// </summary>
        public string ModuleColor
        {
            get { return _moduleColorString; }
            set { _moduleColorString = FormatColorString(value); }
        }

        private string _moduleColorString;

        /// <summary>
        /// 角色颜色值（Unity内置<color>标签颜色名，或十六进制）
        /// </summary>
        public string RoleColor
        {
            get { return _roleColorString; }
            set { _roleColorString = FormatColorString(value); }
        }

        private string _roleColorString;

        public Provider(string module)
        {
            Module = module;
        }

        public Provider (string module, string role, string moduleColorString = "white", string roleColorString = "white") : this(module)
        {
            Module = module;
            Role = role;
            ModuleColor = FormatColorString(moduleColorString);
            RoleColor = FormatColorString(roleColorString);
        }

        private string FormatColorString (string colorString)
        {
            if (string.IsNullOrEmpty(colorString))
                return "white";
                
            var ret = colorString.ToLower();
            if (!caAcceptableBuiltInColors.Contains(ret) && !ret.StartsWith("#")) {
                ret = "#" + ret;
            }
            return ret;
        }

        readonly string[] caAcceptableBuiltInColors = new string[] {
            "aqua", "black", "blue", "brown", "cyan", "darkblue", "fuchsia", "green", "grey",
            "lightblue", "lime", "magenta", "maroon", "navy", "olive", "orange", "purple",
            "red", "silver", "teal", "white", "yellow"
        };
    }

    private static string RetrieveCallerInfo ()
    {
        var stackTrace = new StackTrace();
        string noInfoReturn = "{0}";
        if (stackTrace.FrameCount < 3)
            return noInfoReturn;
        var frame = stackTrace.GetFrame(2);
        if (frame == null)
            return noInfoReturn;
        var method = frame.GetMethod();
        if (method == null)
            return noInfoReturn;
        var callerType = method.DeclaringType;
        var attributes = callerType.GetCustomAttributes(typeof(Provider), true);
        if (attributes == null || attributes.Length == 0)
            return noInfoReturn;
        var provider = attributes[0] as Provider;
        if (provider == null)
            return "";
        string module = string.IsNullOrEmpty(provider.Module) ? callerType.Name : provider.Module;
        string role = string.IsNullOrEmpty(provider.Role) ? null : string.Format("({0})", provider.Role);
        string moduleFormat = string.IsNullOrEmpty(provider.ModuleColor) ? 
                "[{0}]" : 
                string.Format("<color={0}>[{{0}}]</color>", provider.ModuleColor);
        string roleFormat = string.IsNullOrEmpty(provider.RoleColor) ? 
                "{0}" : 
                string.Format("<color={0}>{{0}}</color>", provider.RoleColor);

        return string.Format(
            "{0}{1}{{0}}",
            string.Format(moduleFormat, module),
            string.IsNullOrEmpty(role) ? "" : string.Format(roleFormat, role)
        );
    }
    
    static string MakeFormatOfParams(int count)
    {
        var list = new string[count];
        for (int i = 0; i < count; i++)
        {
            list[i] = string.Format("{{{0}}}", i);
        }
        return string.Join("\t", list);
    }

    public static void Log(object obj){
        Log("{0}", obj);
    }

    public static void Log (string format, params object[] args)
    {
        UnityEngine.Debug.LogFormat(RetrieveCallerInfo(), string.Format(format, args));
    }

    public static void LogEasy(params object[] args)
    {
        Log(MakeFormatOfParams(args.Length), args);
    }

    public static void Warn(object obj)
    {
        Warn("{0}", obj);
    }

    public static void Warn (string format, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(RetrieveCallerInfo(), string.Format(format, args));
    }

    public static void WarnEasy(params object[] args)
    {
        Warn(MakeFormatOfParams(args.Length), args);
    }

    public static void Error(object obj)
    {
        Error("{0}", obj);
    }

    public static void Error (string format, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(RetrieveCallerInfo(), string.Format(format, args));
    }

    public static void ErrorEasy(params object[] args)
    {
        Error(MakeFormatOfParams(args.Length), args);
    }

}