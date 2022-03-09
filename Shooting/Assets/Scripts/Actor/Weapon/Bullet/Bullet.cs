using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bullet : Weapon
{
    /// <summary>
    /// 実際の速度
    /// </summary>
    public override float Speed
    {
        get
        {
            return speed * speedBuff;
        }
    }

    /// <summary>
    /// 移動速度の倍率
    /// </summary>
    [System.NonSerialized]
    public float speedBuff = 1.0f;

    /// <summary>
    /// 溜められたか否か
    /// </summary>
    [System.NonSerialized]
    public bool isCharged = false;

    /// <summary>
    /// 大きさの規定値
    /// </summary>
    [SerializeField]
    private float defaultScale = 1.0f;

    public override void OnGenerated()
    {
        transform.localScale = new Vector2(defaultScale, defaultScale);
        isCharged = false;
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();
        transform.parent = Player.Instance.BulletsAggregator.transform;
    }
}
