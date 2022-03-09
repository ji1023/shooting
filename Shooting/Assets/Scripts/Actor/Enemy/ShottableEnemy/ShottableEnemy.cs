using UnityEngine;
using System.Collections;

[System.Serializable]
public class BulletPool : Pool<Bullet> { }

/// <summary>
/// 弾を撃てる敵
/// </summary>
public class ShottableEnemy : Enemy
{
    /// <summary>
    /// 弾発射までの時間
    /// </summary>
    [SerializeField]
    protected Timer toShot = new Timer(true, true);

    [SerializeField]
    private BulletPool bullets = new BulletPool();

    /// <summary>
    /// 生成時の弾との距離
    /// </summary>
    [SerializeField]
    private float bulletDistance = 1.0f;

    /// <summary>
    /// 弾を破棄するか否か
    /// </summary>
    private bool isDestroyBullets = false;

    /// <summary>
    /// 弾を撃つ
    /// </summary>
    public void Shot()
    {
        if (toShot.Advance())
        {
            // 弾の生成
            var bullet = bullets.Generate(transform.position + Direction * bulletDistance);
            bullet.Color = EnemyManager.Instance.BulletColor;

            // イベント呼び出し
            OnShoted(bullet);

            // 効果音再生
            AudioManager.Instance.PlaySE(EnemyManager.Instance.ShottedSE);
        }
    }

    /// <summary>
    /// 弾を撃った直後
    /// </summary>
    /// <param name="bullet">撃った弾</param>
    protected virtual void OnShoted(Bullet bullet)
    {

    }

    protected override void OnDead()
    {
        isDestroyBullets = false;
    }

    public override void OnGenerated()
    {
        base.OnGenerated();
        toShot.Reset();
        isDestroyBullets = true;
    }

    public override void OnInstantiated()
    {
        base.OnInstantiated();
        bullets.beforeOnInstantiated += (Bullet bullet) =>
        {
            bullet.Angle = Degree.MID;
            bullet.tag = Tags.EnemyWeapon;
        };
    }

    public override void OnUnused()
    {
        base.OnUnused();
        if (isDestroyBullets)
        {// 倒されずに未使用化された場合
            bullets.AllToUnused();
        }
    }
}
