using UnityEngine;
using System.Collections;

public class AimerEnemy : Enemy
{
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public override void OnGenerated()
    {
        base.OnGenerated();

        // プレイヤーを狙う
        LookAt(Player.Instance.transform.position);
    }

    protected override void OnDead()
    {
        base.OnDead();
        Player.Instance.AddDestroyCount(Mission.EnemyKind.Aimer, Weakness);
    }
}
