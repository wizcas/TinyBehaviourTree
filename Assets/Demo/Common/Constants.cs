/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tags
{
    public const string Player = "Player";
    public const string Potion = "Potion";
}

public enum Essence
{
    Gold,
    Wood,
    Water,
    Fire,
    Earth
}

public enum DamageType
{
    None,
    Normal,
    Drown,
    InstantDeath
}

public enum Surface
{
    None = 0,
    Ground = 8,
    Bridge = 10
}

public enum Area
{
    Normal = 0,
    Water
}

[System.Flags]
public enum Enhancements
{
    None = 0,
    /// <summary>
    /// 增加范围
    /// </summary>
    Extend = 1 << 0,
    /// <summary>
    /// 附带灼烧效果
    /// </summary>
    Burn = 1 << 1,
    /// <summary>
    /// 附带治疗效果
    /// </summary>
    Heal = 1 << 2,
    /// <summary>
    /// 石化
    /// </summary>
    Petrify = 1 << 3,
    /// <summary>
    /// 绿化/藤蔓生长
    /// </summary>
    Green = 1 << 4,
    /// <summary>
    /// 附带切割伤害
    /// </summary>
    Cut = 1 << 5,
    /// <summary>
    /// 飞弹效果
    /// </summary>
    Missiles = 1 << 6,
}

public static class PoolID
{
    public const int Ingredient = 1;
    public const int Potion = 2;
    public const int EliminateFX = 3;
    public const int ExplodeFX = 4;
    public const int PotionBreakFx = 5;
}

public static class PoolName
{
    static PoolName()
    {
        var type = typeof(PoolName);
        var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        foreach (var f in fields)
        {
            if (f.FieldType == typeof(string))
                f.SetValue(null, f.Name);
        }
    }

    public static readonly string Potion = "";
    public static readonly string PotionBreakFX = "";
    public static readonly string ExplodeFX = "";
}