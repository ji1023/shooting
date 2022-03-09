using UnityEngine;
using System.Collections;

public class MoveShotEnemy : ShottableEnemy
{
    /// <summary>
    /// 視野角
    /// </summary>
    [SerializeField]
    private Degree viewingAngle = 45.0f;

    // Update is called once per frame
    void Update()
    {
        // 移動
        Move();

        // 発射
        var toPlayer = Player.Instance.transform.position - transform.position;
        var angle = Degree.TwoVector(Direction, toPlayer);
        if (angle <= viewingAngle)
        {// プレイヤーが視野内にいる場合
            Shot();
        }
    }

    protected override void OnShoted(Bullet bullet)
    {
        // プレイヤー狙いの弾
        bullet.LookAt(Player.Instance.transform.position);
    }

    protected override void OnDead()
    {
        base.OnDead();
        Player.Instance.AddDestroyCount(Mission.EnemyKind.MoveShot, Weakness);
    }
}
