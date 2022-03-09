using UnityEngine;
using System.Collections;

public class StraightEnemy : Enemy
{
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    protected override void OnDead()
    {
        base.OnDead();
        Player.Instance.AddDestroyCount(Mission.EnemyKind.Straight, Weakness);
    }
}
