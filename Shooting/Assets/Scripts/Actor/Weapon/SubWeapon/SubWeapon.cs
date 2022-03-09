using UnityEngine;
using System.Collections;

/// <summary>
/// サブ武器
/// </summary>
public abstract class SubWeapon : Weapon
{
    /// <summary>
    /// 溜め具合
    /// </summary>
    public abstract float ChargeRatio
    {
        set;
    }
}
