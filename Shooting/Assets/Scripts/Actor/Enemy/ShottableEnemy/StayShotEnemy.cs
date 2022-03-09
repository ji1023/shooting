using UnityEngine;
using System.Collections;

public class StayShotEnemy : ShottableEnemy
{
    /// <summary>
    /// 停止までの移動距離の幅
    /// </summary>
    [SerializeField]
    private Limit toStayDistanceRange = new Limit(2.0f, 5.0f);

    /// <summary>
    /// 移動予定の距離
    /// </summary>
    private float moveDistance = 0.0f;

    /// <summary>
    /// 移動した距離
    /// </summary>
    private float movedDistance = 0.0f;

    /// <summary>
    /// 移動が終わっているか否か
    /// </summary>
    private bool isMoved = false;

    // Update is called once per frame
    void Update()
    {
        if (isMoved)
        {// 移動が終わっている場合
            Shot();
        }
        else
        {// 移動中の場合
            // 移動
            Move();

            // 移動終了
            movedDistance += SpeedPerFrame;
            if (movedDistance >= moveDistance)
            {
                isMoved = true;
            }
        }
    }

    public override void OnGenerated()
    {
        base.OnGenerated();
        moveDistance = toStayDistanceRange.Rand;
        movedDistance = 0.0f;
        isMoved = false;
    }

    protected override void OnDead()
    {
        base.OnDead();
        Player.Instance.AddDestroyCount(Mission.EnemyKind.StayShot, Weakness);
    }
}
