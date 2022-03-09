using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseBullet : Bullet
{
    /// <summary>
    /// 種類
    /// </summary>
    public override Player.WeaponKind Kind => Player.WeaponKind.Chase;

    /// <summary>
    /// 追う対象
    /// </summary>
    [System.NonSerialized]
    public GameObject target = null;
    
    /// <summary>
    /// 追尾の精度（1に近いほど良い）
    /// </summary>
    [SerializeField]
    private Ratio chaseAccuracy = 0.7f;

    // Update is called once per frame
    void Update()
    {
        if (target == null || !target.activeSelf)
        {
            target = Player.Instance.NearEnemy.gameObject;
        }
        else
        {
            // 目標への方向ベクトル
            var toTarget = (target.transform.position - transform.position).normalized;
            Direction = Vector3.Lerp(Direction, toTarget, chaseAccuracy);
        }
        Move();
    }
}
