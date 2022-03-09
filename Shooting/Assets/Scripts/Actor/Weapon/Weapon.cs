using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Actor
{
    /// <summary>
    /// 種類
    /// </summary>
    public abstract Player.WeaponKind Kind { get; }

    /// <summary>
    /// 攻撃状態か否か
    /// </summary>
    public bool IsAttacking { get; protected set; } = true;

    /// <summary>
    /// 与えるダメージ
    /// </summary>
    public float Damage
    {
        get
        {
            return damage * damageBuff;
        }
    }

    /// <summary>
    /// 攻撃力
    /// </summary>
    [SerializeField]
    private float damage = 0.0f;

    /// <summary>
    /// ダメージ倍率
    /// </summary>
    [System.NonSerialized]
    public float damageBuff = 1.0f;
}
