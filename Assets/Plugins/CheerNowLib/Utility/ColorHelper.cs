/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ColorHelper
{
    public static readonly Color InfoColor = ByWeb("#21b1ed");
    public static readonly Color WarningColor = ByWeb("#f6c311");
    public static readonly Color ErrorColor = ByWeb("#cd3535");

    public static Color ByRGBA (int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color ByWeb (string htmlString)
    {
        Color ret;
        if (string.IsNullOrEmpty(htmlString))
            ret = Color.clear;
        else if (!ColorUtility.TryParseHtmlString(htmlString, out ret)) {
            PrettyLog.Error(
                "Invalid HTML color string: {0}. It needs to be a web color name, or a hex color with a leading '#'",
                htmlString);
            ret = Color.clear;
        }
        return ret;
    }

    public static Color ByObject(object obj)
    {
        var val = obj.GetHashCode() & 0x00FFFFFF;
        return ByWeb("#" + val.ToString("X6"));
    }
}

public static class ColorNames
{
    public const string white = "white";
    public const string silver = "silver";
    public const string gray = "gray";
    public const string black = "black";

    public const string maroon = "maroon";
    public const string red = "red";
    public const string brown = "brown";
    public const string orange = "orange";
    public const string yellow = "yellow";

    public const string olive = "olive";
    public const string lime = "lime";
    public const string green = "green";
    public const string cyan = "cyan";
    public const string teal = "teal";

    public const string aqua = "aqua";
    public const string lightblue = "lightblue";
    public const string blue = "blue";
    public const string navy = "navy";
    public const string darkblue = "darkblue";

    public const string fuchsia = "fuchsia";
    public const string magenta = "magenta";
    public const string purple = "purple";

    static string[] _availableNames;
    public static bool IsNameAvailable(string colorName){
        if (_availableNames == null) {
            var t = typeof(ColorNames);
            var fields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f=>f.FieldType == typeof(string)).ToArray();
            _availableNames = new string[fields.Length];
            for (int i = 0; i < _availableNames.Length; i++) {
                _availableNames[i] = (string)fields[i].GetValue(null);
            }
        }
        return _availableNames.Contains(colorName);
    }
}